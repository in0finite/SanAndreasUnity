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

        private static bool s_subscribedToPedOnDamageEvent = false;

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


        private void Awake()
        {
            this.MyPed = this.GetComponentOrThrow<Ped>();

            if (!s_subscribedToPedOnDamageEvent)
            {
                s_subscribedToPedOnDamageEvent = true;
                Ped.onDamaged += OnPedDamaged;
            }
        }

        private void OnEnable()
        {
            s_allPedAIs.Add(this);
        }

        private void OnDisable()
        {
            s_allPedAIs.Remove(this);
        }

        private static void OnPedDamaged(Ped hitPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();

            if (attackerPed != null)
            {
                foreach (var ai in AllPedAIs)
                {
                    if (ai.Action == PedAIAction.Following && ai.TargetPed == hitPed)
                    {
                        ai._enemyPeds.AddIfNotPresent(attackerPed);
                    }
                }
            }

            var hitPedAi = hitPed.GetComponent<PedAI>();
            if (null == hitPedAi)
                return;

            if (attackerPed != null)
                hitPedAi._enemyPeds.AddIfNotPresent(attackerPed);

            if (hitPed.PedDef != null &&
                (hitPed.PedDef.DefaultType == PedestrianType.Criminal ||
                 hitPed.PedDef.DefaultType == PedestrianType.Cop ||
                 hitPed.PedDef.DefaultType.IsGangMember()))
            {
                hitPedAi.TargetPed = attackerPed;
                hitPedAi.Action = PedAIAction.Chasing;
            }
            else
                hitPedAi.Action = PedAIAction.Escaping;
        }

        void Update()
        {
            this.MyPed.ResetInput();
            if (NetStatus.IsServer)
            {
                switch (this.Action)
                {
                    case PedAIAction.WalkingAround:
                        this.UpdateWalkingAround();
                        break;
                    case PedAIAction.Chasing:
                        if (this.TargetPed != null)
                        {
                            this.UpdateAttackOnPed(this.TargetPed);
                        }
                        else // The target is dead/disconnected
                        {
                            this.Action = PedAIAction.WalkingAround;
                        }
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
            Vector2 offset = Random.insideUnitCircle * TargetNode.PathWidth / 2f * 0.9f;
            _moveDestination = TargetNode.Position + offset.ToVector3XZ();
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

            if (this.ArrivedAtDestinationNode())
                this.OnArrivedToDestinationNode();

            if (!this.HasTargetNode)
                return;

            this.MyPed.IsWalkOn = true;
            this.MyPed.Movement = (_moveDestination - this.MyPed.transform.position).normalized;
            this.MyPed.Heading = this.MyPed.Movement;
        }

        void UpdateEscaping()
        {
            if (this.ArrivedAtDestinationNode())
                this.OnArrivedToDestinationNode();

            if (!this.HasTargetNode)
                return;

            this.MyPed.IsSprintOn = true;
            this.MyPed.Movement = (_moveDestination - this.MyPed.transform.position).normalized;
            this.MyPed.Heading = this.MyPed.Movement;
        }

        void UpdateFollowing()
        {
            if (null == this.TargetPed)
            {
                this.Action = PedAIAction.Idle;
                return;
            }

            // handle vehicle logic
            if (this.MyPed.IsInVehicle || this.TargetPed.IsInVehicle)
                this.UpdateFollowingMovementPart();

            if (this.Action != PedAIAction.Following)
                return;

            if (null == _currentlyEngagedPed)
            {
                // see if we can attack any enemy ped
                _enemyPeds.RemoveDeadObjects();
                if (_enemyPeds.Count > 0)
                {
                    Vector3 myPosition = this.transform.position;
                    var closestPed = _enemyPeds.Aggregate((p1, p2) =>
                        Vector3.Distance(p1.transform.position, myPosition)
                        < Vector3.Distance(p2.transform.position, myPosition) ? p1 : p2);
                    _currentlyEngagedPed = closestPed;
                }
            }

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
                this.Action = PedAIAction.Idle;
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

        public void StartWalkingAround(PathNode pathNode)
        {
            this.CurrentNode = pathNode;
            this.HasCurrentNode = true;
            this.TargetNode = pathNode;
            this.HasTargetNode = true;
            this.Action = PedAIAction.WalkingAround;
            this.TargetPed = null;
        }

        public void StartFollowing(Ped ped)
        {
            this.Action = PedAIAction.Following;
            this.TargetPed = ped;
        }

        public void Recruit(Ped recruiterPed)
        {
            if (!this.PedestrianType.IsGangMember() && this.PedestrianType != PedestrianType.Criminal)
                return;

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
                // start following
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