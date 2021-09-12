using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Paths;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours
{
    public enum PedAIAction
    {
        Idle = 0,
        WalkingAround,
        Chasing,
        Escaping,
        Following,
    }

    public class PedAI : MonoBehaviour
    {
        private static readonly List<PedAI> s_allPedAIs = new List<PedAI>();
        public static IReadOnlyList<PedAI> AllPedAIs => s_allPedAIs;

        public PedAIAction Action { get; private set; }

        private Vector3 _moveDestination;

        /// <summary>
        /// The node where the Ped starts
        /// </summary>
        public PathNode CurrentNode { get; private set; }

        public bool HasCurrentNode { get; private set; } = false;

        /// <summary>
        /// The node the Ped is targeting
        /// </summary>
        public PathNode TargetNode { get; private set; }

        public bool HasTargetNode { get; private set; } = false;

        /// <summary>
        /// The ped that this ped is chasing
        /// </summary>
        public Ped TargetPed { get; private set; }

        public Ped MyPed { get; private set; }

        public PedestrianType PedestrianType => this.MyPed.PedDef.DefaultType;

        private List<Ped> _enemyPeds = new List<Ped>();
        private Ped _currentlyEngagedPed;

        private bool _isFindingPathNodeDelayed = false;


        private void Awake()
        {
            this.MyPed = this.GetComponentOrThrow<Ped>();
        }

        private void OnEnable()
        {
            s_allPedAIs.Add(this);
            Ped.onDamaged += OnPedDamaged;
        }

        private void OnDisable()
        {
            s_allPedAIs.Remove(this);
            Ped.onDamaged -= OnPedDamaged;
        }

        private void OnPedDamaged(Ped hitPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            if (this.MyPed == hitPed)
            {
                this.OnMyPedDamaged(dmgInfo, dmgResult);
                return;
            }

            this.OnOtherPedDamaged(hitPed, dmgInfo, dmgResult);
        }

        void OnMyPedDamaged(DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();

            if (this.Action == PedAIAction.Following)
            {
                if (null == attackerPed)
                    return;

                if (attackerPed == this.TargetPed)
                {
                    // our leader attacked us
                    // stop following him
                    this.TargetPed = null;
                    return;
                }

                if (!this.IsMemberOfOurGroup(attackerPed))
                {
                    _enemyPeds.AddIfNotPresent(attackerPed);
                    return;
                }

                return;
            }

            if (attackerPed != null)
                _enemyPeds.AddIfNotPresent(attackerPed);

            if (this.Action == PedAIAction.Idle || this.Action == PedAIAction.WalkingAround)
            {
                var hitPed = this.MyPed;

                if (hitPed.PedDef != null &&
                    (hitPed.PedDef.DefaultType.IsCriminal() ||
                     hitPed.PedDef.DefaultType.IsCop() ||
                     hitPed.PedDef.DefaultType.IsGangMember()))
                {
                    if (attackerPed != null)
                        this.Action = PedAIAction.Chasing;
                }
                else
                    this.Action = PedAIAction.Escaping;

                return;
            }

        }

        void OnOtherPedDamaged(Ped otherPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();
            if (null == attackerPed)
                return;

            if (this.Action == PedAIAction.Following)
            {
                if (this.IsMemberOfOurGroup(attackerPed))
                    return;

                if (this.TargetPed == otherPed)
                {
                    // our leader was attacked
                    // his enemies are also our enemies
                    _enemyPeds.AddIfNotPresent(attackerPed);
                    return;
                }

                if (this.IsMemberOfOurGroup(otherPed))
                {
                    // attacked ped is member of our group
                    // his enemy will be also our enemy
                    _enemyPeds.AddIfNotPresent(attackerPed);
                    return;
                }

                return;
            }

        }

        bool IsMemberOfOurGroup(Ped ped)
        {
            if (this.Action != PedAIAction.Following || this.TargetPed == null) // we are not part of any group
                return false;

            if (this.TargetPed == ped) // our leader
                return true;

            var pedAI = ped.GetComponent<PedAI>();
            if (pedAI != null && pedAI.Action == PedAIAction.Following && pedAI.TargetPed == this.TargetPed)
                return true;

            return false;
        }

        void Update()
        {
            this.MyPed.ResetInput();
            if (NetStatus.IsServer)
            {
                switch (this.Action)
                {
                    case PedAIAction.Idle:
                        this.UpdateIdle();
                        break;
                    case PedAIAction.WalkingAround:
                        this.UpdateWalkingAround();
                        break;
                    case PedAIAction.Chasing:
                        this.UpdateChasing();
                        break;
                    case PedAIAction.Escaping:
                        this.UpdateEscaping();
                        break;
                    case PedAIAction.Following:
                        this.UpdateFollowing();
                        break;
                }
            }
        }

        bool ArrivedAtDestinationNode()
        {
            if (!this.HasTargetNode)
                return false;
            if (Vector2.Distance(this.transform.position.ToVec2WithXAndZ(), this.TargetNode.Position.ToVec2WithXAndZ())
                < this.TargetNode.PathWidth / 2f)
                return true;
            return false;
        }

        void OnArrivedToDestinationNode()
        {
            PathNode previousNode = CurrentNode;
            CurrentNode = TargetNode;
            TargetNode = GetNextPathNode(previousNode, CurrentNode);
            this.AssignMoveDestinationBasedOnTargetNode();
        }

        void AssignMoveDestinationBasedOnTargetNode()
        {
            Vector2 offset = Random.insideUnitCircle * TargetNode.PathWidth / 2f * 0.9f;
            _moveDestination = TargetNode.Position + offset.ToVector3XZ();
        }

        void UpdateIdle()
        {
            this.StartWalkingAround();
        }

        void UpdateWalkingAround()
        {
            if (this.MyPed.IsInVehicleSeat)
            {
                // exit vehicle
                this.MyPed.OnSubmitPressed();
                return;
            }

            if (this.MyPed.IsInVehicle) // wait until we exit vehicle
                return;

            // check if we gained some enemies
            _enemyPeds.RemoveDeadObjectsIfNotEmpty();
            if (_enemyPeds.Count > 0)
            {
                this.Action = PedAIAction.Chasing;
                return;
            }

            if (this.ArrivedAtDestinationNode())
                this.OnArrivedToDestinationNode();

            if (!this.HasTargetNode)
            {
                this.FindNextNodeDelayed();
                return;
            }

            this.MyPed.IsWalkOn = true;
            this.MyPed.Movement = (_moveDestination - this.MyPed.transform.position).normalized;
            this.MyPed.Heading = this.MyPed.Movement;
        }

        void UpdateChasing()
        {
            if (null == this.TargetPed)
                this.TargetPed = this.GetNextPedToAttack();

            if (null == this.TargetPed)
            {
                // we finished attacking all enemies, now start walking
                this.StartWalkingAround();
                return;
            }

            this.UpdateAttackOnPed(this.TargetPed);
        }

        void UpdateEscaping()
        {
            if (this.ArrivedAtDestinationNode())
                this.OnArrivedToDestinationNode();

            if (!this.HasTargetNode)
            {
                this.FindNextNodeDelayed();
                return;
            }

            this.MyPed.IsSprintOn = true;
            this.MyPed.Movement = (_moveDestination - this.MyPed.transform.position).normalized;
            this.MyPed.Heading = this.MyPed.Movement;
        }

        void UpdateFollowing()
        {
            if (null == this.TargetPed)
            {
                this.StartIdling();
                return;
            }

            // handle vehicle logic - follow ped in or out of vehicle
            if (this.MyPed.IsInVehicle || this.TargetPed.IsInVehicle)
                this.UpdateFollowingMovementPart();

            if (this.Action != PedAIAction.Following)
                return;

            if (null == _currentlyEngagedPed)
                _currentlyEngagedPed = this.GetNextPedToAttack();

            if (_currentlyEngagedPed != null)
            {
                this.UpdateAttackOnPed(_currentlyEngagedPed);
            }
            else
            {
                // no peds to attack
                // follow our leader
                this.UpdateFollowingMovementPart();
            }
        }

        void UpdateFollowingMovementPart()
        {
            // follow target ped

            if (null == this.TargetPed)
            {
                this.StartIdling();
                return;
            }

            Vector3 targetPos = this.TargetPed.transform.position;
            float currentStoppingDistance = 3f;

            if (this.TargetPed.IsInVehicleSeat && !this.MyPed.IsInVehicle)
            {
                // find a free vehicle seat to enter vehicle

                var vehicle = this.TargetPed.CurrentVehicle;

                var closestfreeSeat = Ped.GetFreeSeats(vehicle).Select(sa => new {sa = sa, tr = vehicle.GetSeatTransform(sa)})
                    .OrderBy(s => s.tr.Distance(this.transform.position))
                    .FirstOrDefault();

                if (closestfreeSeat != null)
                {
                    // check if we would enter this seat on attempt
                    var vehicleThatPedWouldEnter = this.MyPed.GetVehicleThatPedWouldEnterOnAttempt();
                    if (vehicleThatPedWouldEnter.vehicle == vehicle && vehicleThatPedWouldEnter.seatAlignment == closestfreeSeat.sa)
                    {
                        // we would enter this seat
                        // go ahead and enter it
                        this.MyPed.OnSubmitPressed();
                        return;
                    }
                    else
                    {
                        // we would not enter this seat - it's not close enough, or maybe some other seat (occupied one) is closer
                        // move toward the seat
                        targetPos = closestfreeSeat.tr.position;
                        currentStoppingDistance = 0.01f;
                    }
                }

            }
            else if (!this.TargetPed.IsInVehicle && this.MyPed.IsInVehicleSeat)
            {
                // target player is not in vehicle, and ours is
                // exit the vehicle

                this.MyPed.OnSubmitPressed();
                return;
            }


            if (this.MyPed.IsInVehicle)
                return;

            Vector3 diff = targetPos - this.transform.position;
            float distance = diff.ToVec2WithXAndZ().magnitude;

            if (distance > currentStoppingDistance)
            {
                Vector3 diffDir = diff.normalized;

                this.MyPed.IsRunOn = true;
                this.MyPed.Movement = diffDir;
                this.MyPed.Heading = diffDir;
            }
        }

        private void UpdateAttackOnPed(Ped ped)
        {
            Vector3 diff = GetHeadOrTransform(ped).position - GetHeadOrTransform(this.MyPed).position;
            Vector3 dir = diff.normalized;
            this.MyPed.Heading = dir;

            if (this.MyPed.IsInVehicle)
            {
                if (diff.magnitude < 10f)
                {
                    this.MyPed.AimDirection = dir;
                    if (!this.MyPed.IsAiming)
                        this.MyPed.OnAimButtonPressed();
                    this.MyPed.IsFireOn = true;
                }
                else
                {
                    if (this.MyPed.IsAiming)
                        this.MyPed.OnAimButtonPressed();
                }
            }
            else
            {
                if (diff.magnitude < 10f)
                {
                    this.MyPed.AimDirection = dir;
                    this.MyPed.IsAimOn = true;
                    this.MyPed.IsFireOn = true;
                }
                else if (Vector2.Distance(ped.transform.position.ToVec2WithXAndZ(), this.MyPed.transform.position.ToVec2WithXAndZ()) > 3f)
                {
                    this.MyPed.IsRunOn = true;
                    this.MyPed.Movement = dir;
                }
            }
        }

        public void StartIdling()
        {
            this.Action = PedAIAction.Idle;
            this.HasCurrentNode = false;
            this.HasTargetNode = false;
        }

        public void StartWalkingAround(PathNode pathNode)
        {
            this.CurrentNode = pathNode;
            this.HasCurrentNode = true;
            this.TargetNode = pathNode;
            this.HasTargetNode = true;
            this.Action = PedAIAction.WalkingAround;
            this.TargetPed = null;
            this.AssignMoveDestinationBasedOnTargetNode();
        }

        public void StartWalkingAround()
        {
            this.HasCurrentNode = false;
            this.HasTargetNode = false;
            this.Action = PedAIAction.WalkingAround;
            this.TargetPed = null;
        }

        public void StartFollowing(Ped ped)
        {
            this.Action = PedAIAction.Following;
            this.TargetPed = ped;
        }

        public void StartChasing(Ped ped)
        {
            this.Action = PedAIAction.Chasing;
            this.TargetPed = ped;
            _enemyPeds.AddIfNotPresent(ped);
        }

        public void Recruit(Ped recruiterPed)
        {
            if (this.Action == PedAIAction.Following)
            {
                if (this.TargetPed == recruiterPed)
                {
                    // unfollow
                    this.TargetPed = null;
                    return;
                }
            }
            else if (this.Action == PedAIAction.Idle || this.Action == PedAIAction.WalkingAround)
            {
                if (!this.PedestrianType.IsGangMember() && !this.PedestrianType.IsCriminal())
                    return;

                this.StartFollowing(recruiterPed);
            }
        }

        private static Transform GetHeadOrTransform(Ped ped)
        {
            return ped.PlayerModel.Head != null ? ped.PlayerModel.Head : ped.transform;
        }

        private static PathNode GetNextPathNode(PathNode previousNode, PathNode currentNode)
        {
            var possibilities = new List<PathNode>(
                NodeReader.GetAllLinkedNodes(currentNode)
                    .Where(_ => !_.Equals(previousNode)));

            if (possibilities.Count > 0)
            {
                return possibilities.RandomElement();
            }
            else
            {
                //No possibilities found, returning to previous node
                return previousNode;
            }
        }

        PathNode? GetClosestPathNodeToWalk()
        {
            Vector3 pos = this.MyPed.transform.position;
            float radius = 200f;

            var pathNodeInfo = NodeReader.GetAreasInRadius(pos, radius)
                .SelectMany(area => area.PedNodes)
                .Where(node => node.CanPedWalkHere)
                .Select(node => (node, distance: Vector3.Distance(node.Position, pos)))
                .Where(_ => _.distance < radius)
                .MinBy(_ => _.distance, default);

            if (EqualityComparer<PathNode>.Default.Equals(pathNodeInfo.node, default))
                return null;

            return pathNodeInfo.node;
        }

        private void FindNextNodeDelayed()
        {
            if (_isFindingPathNodeDelayed)
                return;

            _isFindingPathNodeDelayed = true;

            this.CancelInvoke(nameof(this.FindNextNodeDelayedCallback));
            this.Invoke(nameof(this.FindNextNodeDelayedCallback), 2f);
        }

        private void FindNextNodeDelayedCallback()
        {
            _isFindingPathNodeDelayed = false;

            if (this.HasTargetNode) // already assigned ?
                return;

            var closestPathNodeToWalk = this.GetClosestPathNodeToWalk();
            if (null == closestPathNodeToWalk)
                return;

            this.TargetNode = closestPathNodeToWalk.Value;
            this.HasTargetNode = true;
            this.AssignMoveDestinationBasedOnTargetNode();
        }

        private Ped GetNextPedToAttack()
        {
            _enemyPeds.RemoveDeadObjectsIfNotEmpty();
            if (_enemyPeds.Count == 0)
                return null;

            Vector3 myPosition = this.transform.position;

            var closestPed = _enemyPeds.Aggregate((p1, p2) =>
                Vector3.Distance(p1.transform.position, myPosition)
                < Vector3.Distance(p2.transform.position, myPosition)
                    ? p1
                    : p2);

            return closestPed;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(CurrentNode.Position, TargetNode.Position);
            Gizmos.DrawWireSphere(CurrentNode.Position, CurrentNode.PathWidth / 2f);
            Gizmos.DrawWireSphere(TargetNode.Position, TargetNode.PathWidth / 2f);

            Gizmos.color = Color.yellow;

            NodeReader.GetAllLinkedNodes(TargetNode)
                .Except(new[] {CurrentNode})
                .ForEach(node =>
                {
                    Gizmos.DrawLine(TargetNode.Position, node.Position);
                    Gizmos.DrawWireSphere(node.Position, node.PathWidth / 2f);
                });
            NodeReader.GetAllLinkedNodes(CurrentNode)
                .Except(new[] {TargetNode})
                .ForEach(node =>
                {
                    Gizmos.DrawLine(CurrentNode.Position, node.Position);
                    Gizmos.DrawWireSphere(node.Position, node.PathWidth / 2f);
                });
        }
    }

}