using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Paths;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours
{
    public enum PedAction
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

        [SerializeField] private Vector3 currentNodePos;
        [SerializeField] private Vector3 targetNodePos;
        [SerializeField] private Vector2 targetNodeOffset; // Adding random offset to prevent peds to have the exact destination

        public PedAction Action { get; private set; }

        /// <summary>
        /// The node where the Ped starts
        /// </summary>
        public PathNode CurrentNode { get; private set; }

        /// <summary>
        /// The node the Ped is targeting
        /// </summary>
        public PathNode TargetNode { get; private set; }

        /// <summary>
        /// The ped that this ped is chasing
        /// </summary>
        public Ped TargetPed { get; private set; }

        public Ped MyPed { get; private set; }

        public PedestrianType PedestrianType => this.MyPed.PedDef.DefaultType;


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
            var hitPedAi = hitPed.GetComponent<PedAI>();
            if (null == hitPedAi)
                return;

            if (hitPed.PedDef != null &&
                (hitPed.PedDef.DefaultType == PedestrianType.Criminal ||
                hitPed.PedDef.DefaultType == PedestrianType.Cop ||
                hitPed.PedDef.DefaultType.IsGangMember()))
            {
                hitPedAi.TargetPed = dmgInfo.GetAttackerPed();
                hitPedAi.Action = PedAction.Chasing;
            }
            else
                hitPedAi.Action = PedAction.Escaping;
        }

        void Update()
        {
            this.MyPed.ResetInput();
            if (NetStatus.IsServer)
            {
                switch (this.Action)
                {
                    case PedAction.WalkingAround:
                        currentNodePos = CurrentNode.Position;
                        targetNodePos = TargetNode.Position;
                        if (Vector2.Distance(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(targetNodePos.x, targetNodePos.z)) < 3)
                        {
                            // arrived at target node
                            PathNode previousNode = CurrentNode;
                            CurrentNode = TargetNode;
                            TargetNode = GetNextPathNode(previousNode, CurrentNode);
                            targetNodeOffset = new Vector2(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 2));
                        }
                        this.MyPed.IsWalkOn = true;
                        Vector3 dest = targetNodePos + new Vector3(targetNodeOffset.x, 0, targetNodeOffset.y);
                        this.MyPed.Movement = (dest - this.MyPed.transform.position).normalized;
                        this.MyPed.Heading = this.MyPed.Movement;
                        break;
                    case PedAction.Chasing:
                        if (this.TargetPed != null)
                        {
                            Vector3 diff = GetHeadOrTransform(this.TargetPed).position - GetHeadOrTransform(this.MyPed).position;
                            Vector3 dir = diff.normalized;
                            if (diff.magnitude < 10f)
                            {
                                this.MyPed.Heading = dir;
                                this.MyPed.AimDirection = dir;
                                this.MyPed.IsAimOn = true;
                                this.MyPed.IsFireOn = true;
                            }
                            else
                            {
                                this.MyPed.IsRunOn = true;
                                this.MyPed.Movement = dir;
                                this.MyPed.Heading = dir;
                            }
                        }
                        else // The target is dead/disconnected
                        {
                            this.Action = PedAction.WalkingAround;
                        }
                        break;
                    case PedAction.Escaping:
                        currentNodePos = CurrentNode.Position;
                        targetNodePos = TargetNode.Position;
                        if (Vector2.Distance(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(TargetNode.Position.x, TargetNode.Position.z)) < 1f)
                        {
                            // arrived at target node
                            PathNode previousNode = CurrentNode;
                            CurrentNode = TargetNode;
                            TargetNode = GetNextPathNode(previousNode, CurrentNode);
                        }
                        this.MyPed.IsSprintOn = true;
                        this.MyPed.Movement = (TargetNode.Position - this.MyPed.transform.position).normalized;
                        this.MyPed.Heading = this.MyPed.Movement;
                        break;
                    case PedAction.Following:
                        this.UpdateFollowing();
                        break;
                }
            }
        }

        void UpdateFollowing()
        {
            // follow target ped

            if (this.TargetPed != null) {

                Vector3 targetPos = this.TargetPed.transform.position;
                float currentStoppingDistance = 3f;

                if (this.TargetPed.IsInVehicleSeat && !this.MyPed.IsInVehicle) {
                    // find a free vehicle seat to enter vehicle

                    var vehicle = this.TargetPed.CurrentVehicle;

                    var closestfreeSeat = Ped.GetFreeSeats (vehicle).Select (sa => new { sa = sa, tr = vehicle.GetSeatTransform (sa) })
                        .OrderBy (s => s.tr.Distance (this.transform.position))
                        .FirstOrDefault ();

                    if (closestfreeSeat != null) {
                        // check if it is in range
                        if (closestfreeSeat.tr.Distance (this.transform.position) < this.MyPed.EnterVehicleRadius) {
                            // the seat is in range
                            this.MyPed.EnterVehicle (vehicle, closestfreeSeat.sa);
                        } else {
                            // the seat is not in range
                            // move towards this seat
                            targetPos = closestfreeSeat.tr.position;
                            currentStoppingDistance = 0.1f;
                        }
                    }

                } else if (!this.TargetPed.IsInVehicle && this.MyPed.IsInVehicleSeat) {
                    // target player is not in vehicle, and ours is
                    // exit the vehicle

                    this.MyPed.ExitVehicle ();
                }


                if (this.MyPed.IsInVehicle)
                    return;

                Vector3 diff = targetPos - this.transform.position;
                float distance = diff.magnitude;

                if (distance > currentStoppingDistance)
                {
                    Vector3 diffDir = diff.normalized;

                    this.MyPed.IsRunOn = true;
                    this.MyPed.Movement = diffDir;
                    this.MyPed.Heading = diffDir;
                }

            }
            else
            {
                this.Action = PedAction.Idle;
            }
        }

        public void StartWalkingAround(PathNode pathNode)
        {
            this.CurrentNode = pathNode;
            this.TargetNode = pathNode;
            this.Action = PedAction.WalkingAround;
            this.TargetPed = null;
        }

        public void StartFollowing(Ped ped)
        {
            this.Action = PedAction.Following;
            this.TargetPed = ped;
        }

        public void Recruit(Ped recruiterPed)
        {
            if (!this.PedestrianType.IsGangMember() && this.PedestrianType != PedestrianType.Criminal)
                return;

            if (this.Action == PedAction.Following)
            {
                if (this.TargetPed == recruiterPed)
                {
                    // unfollow
                    this.TargetPed = null;
                    return;
                }
            }
            else if (this.Action == PedAction.Idle || this.Action == PedAction.WalkingAround)
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