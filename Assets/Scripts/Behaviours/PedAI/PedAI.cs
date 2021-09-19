using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Paths;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours.Peds.AI
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
        public List<Ped> EnemyPeds => _enemyPeds;

        private Ped _currentlyEngagedPed;


        private void Awake()
        {
            this.MyPed = this.GetComponentOrThrow<Ped>();
        }

        private void OnEnable()
        {
            s_allPedAIs.Add(this);
            Ped.onDamaged += OnPedDamaged;
            Vehicle.onDamaged += OnVehicleDamaged;
        }

        private void OnDisable()
        {
            s_allPedAIs.Remove(this);
            Ped.onDamaged -= OnPedDamaged;
            Vehicle.onDamaged -= OnVehicleDamaged;
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

        void OnOtherPedDamaged(Ped damagedPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();
            if (null == attackerPed)
                return;

            if (this.Action == PedAIAction.Following)
            {
                if (null == this.TargetPed) // we are not in a group
                    return;

                bool isAttackerPedMember = this.IsMemberOfOurGroup(attackerPed);
                bool isDamagedPedMember = this.IsMemberOfOurGroup(damagedPed);

                if (attackerPed == this.TargetPed && !isDamagedPedMember && dmgInfo.damageType != DamageType.Explosion)
                {
                    // our leader attacked someone, not as part of explosion
                    // make that someone our enemy
                    _enemyPeds.AddIfNotPresent(damagedPed);
                    return;
                }

                if (this.TargetPed == damagedPed && !isAttackerPedMember)
                {
                    // our leader was attacked
                    // his enemies are also our enemies
                    _enemyPeds.AddIfNotPresent(attackerPed);
                    return;
                }

                if (isDamagedPedMember && !isAttackerPedMember)
                {
                    // attacked ped is member of our group
                    // his enemy will be also our enemy
                    _enemyPeds.AddIfNotPresent(attackerPed);
                    return;
                }

                return;
            }

        }

        void OnVehicleDamaged(Vehicle vehicle, DamageInfo damageInfo)
        {
            Ped attackerPed = damageInfo.GetAttackerPed();
            if (null == attackerPed)
                return;

            if (this.Action == PedAIAction.Following)
            {
                if (null == this.TargetPed) // not member of group
                    return;

                // ignore explosion damage, it can be "accidental"
                if (damageInfo.damageType == DamageType.Explosion)
                    return;

                if (this.IsMemberOfOurGroup(attackerPed))
                    return;

                if (vehicle.Seats.Exists(s => s.OccupyingPed == this.MyPed || s.OccupyingPed == this.TargetPed))
                {
                    // either our leader or we are in the vehicle
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
            return ArrivedAtDestinationNode(this.HasTargetNode ? this.TargetNode : (PathNode?)null, this.transform);
        }

        public static bool ArrivedAtDestinationNode(PathMovementData pathMovementData, Transform tr)
        {
            if (!pathMovementData.destinationNode.HasValue)
                return false;
            if (Vector2.Distance(tr.position.ToVec2WithXAndZ(), pathMovementData.destinationNode.Value.Position.ToVec2WithXAndZ())
                < pathMovementData.destinationNode.Value.PathWidth / 2f)
                return true;
            return false;
        }

        private void OnArrivedToDestinationNode()
        {
            var c = CurrentNode;
            var d = TargetNode;
            OnArrivedToDestinationNode(ref c, ref d, out Vector3 m);
            CurrentNode = c;
            TargetNode = d;
            _moveDestination = m;
        }

        public static void OnArrivedToDestinationNode(PathMovementData pathMovementData)
        {
            if (!pathMovementData.destinationNode.HasValue)
                return;

            PathNode previousNode = pathMovementData.currentNode.GetValueOrDefault();
            pathMovementData.currentNode = pathMovementData.destinationNode;
            pathMovementData.destinationNode = GetNextPathNode(previousNode, pathMovementData.currentNode.Value);
            pathMovementData.moveDestination = GetMoveDestinationBasedOnTargetNode(pathMovementData.destinationNode.Value);
        }

        public static Vector3 GetMoveDestinationBasedOnTargetNode(PathNode targetNode)
        {
            Vector2 offset = Random.insideUnitCircle * targetNode.PathWidth / 2f * 0.9f;
            return targetNode.Position + offset.ToVector3XZ();
        }

        public static void FindClosestWalkableNode(PathMovementData pathMovementData, Vector3 position)
        {
            if (Time.time - pathMovementData.timeWhenAttemptedToFindClosestNode < 2f) // don't attempt to find it every frame
                return;

            pathMovementData.timeWhenAttemptedToFindClosestNode = Time.time;

            var closestPathNodeToWalk = PedAI.GetClosestPathNodeToWalk(position);
            if (null == closestPathNodeToWalk)
                return;

            pathMovementData.destinationNode = closestPathNodeToWalk;
            pathMovementData.moveDestination = PedAI.GetMoveDestinationBasedOnTargetNode(closestPathNodeToWalk.Value);
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
                this.UpdateFollowing_MovementPart();

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

                this.UpdateFollowing_MovementPart();

                if (this.MyPed.IsInVehicle && this.MyPed.IsAiming)
                {
                    // stop aiming
                    this.MyPed.OnAimButtonPressed();
                    return;
                }
            }
        }

        void UpdateFollowing_MovementPart()
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

        public void StartChasing()
        {
            this.Action = PedAIAction.Chasing;
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

        public static PathNode? GetClosestPathNodeToWalk(Vector3 pos)
        {
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