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
    public class PedAI : MonoBehaviour
    {
        private static readonly List<PedAI> s_allPedAIs = new List<PedAI>();
        public static IReadOnlyList<PedAI> AllPedAIs => s_allPedAIs;

        public Ped MyPed { get; private set; }

        public PedestrianType PedestrianType => this.MyPed.PedDef.DefaultType;

        public List<Ped> EnemyPeds { get; } = new List<Ped>();

        private readonly StateMachine _stateMachine = new StateMachine();
        public BaseState CurrentState => (BaseState) _stateMachine.CurrentState;

        public StateContainer<BaseState> StateContainer { get; } = new StateContainer<BaseState>();


        private void Awake()
        {
            this.MyPed = this.GetComponentOrThrow<Ped>();

            BaseState[] states =
            {
                new IdleState(),
                new WalkAroundState(),
                new EscapeState(),
                new ChaseState(),
                new FollowState(),
            };

            this.StateContainer.AddStates(states);

            foreach (var state in this.StateContainer.States)
                F.RunExceptionSafe(() => state.OnAwake(this));

            _stateMachine.SwitchState(this.StateContainer.GetState<IdleState>());
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
                this.CurrentState.OnMyPedDamaged(dmgInfo, dmgResult);
                return;
            }

            this.CurrentState.OnOtherPedDamaged(hitPed, dmgInfo, dmgResult);
        }

        void OnVehicleDamaged(Vehicle vehicle, DamageInfo damageInfo)
        {
            this.CurrentState.OnVehicleDamaged(vehicle, damageInfo);
        }

        void Update()
        {
            if (NetStatus.IsServer)
            {
                this.MyPed.ResetInput();
                this.CurrentState.UpdateState();
            }
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

        public void SwitchState<T>()
            where T : BaseState
        {
            _stateMachine.SwitchState(this.StateContainer.GetStateOrThrow<T>());
        }

        public void SwitchStateWithParameter<T>(object parameter)
            where T : BaseState
        {
            _stateMachine.SwitchStateWithParameter(this.StateContainer.GetStateOrThrow<T>(), parameter);
        }

        public void StartIdling()
        {
            this.SwitchState<IdleState>();
        }

        public void StartWalkingAround(PathNode pathNode)
        {
            this.SwitchStateWithParameter<WalkAroundState>(pathNode);
        }

        public void StartWalkingAround()
        {
            this.SwitchState<WalkAroundState>();
        }

        public void StartFollowing(Ped ped)
        {
            this.SwitchStateWithParameter<FollowState>(ped);
        }

        public void StartChasing(Ped ped)
        {
            this.SwitchStateWithParameter<ChaseState>(ped);
        }

        public void StartChasing()
        {
            this.SwitchState<ChaseState>();
        }

        public void StartEscaping()
        {
            this.SwitchState<EscapeState>();
        }

        public void Recruit(Ped recruiterPed)
        {
            this.CurrentState.OnRecruit(recruiterPed);
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
            this.CurrentState.OnDrawGizmosSelected();
        }

        public static void OnDrawGizmosSelected(PathMovementData pathMovementData)
        {
            Gizmos.color = Color.green;

            if (pathMovementData.currentNode.HasValue && pathMovementData.destinationNode.HasValue)
                Gizmos.DrawLine(pathMovementData.currentNode.Value.Position, pathMovementData.destinationNode.Value.Position);
            if (pathMovementData.currentNode.HasValue)
                Gizmos.DrawWireSphere(pathMovementData.currentNode.Value.Position, pathMovementData.currentNode.Value.PathWidth / 2f);
            if (pathMovementData.destinationNode.HasValue)
                Gizmos.DrawWireSphere(pathMovementData.destinationNode.Value.Position, pathMovementData.destinationNode.Value.PathWidth / 2f);

            Gizmos.color = Color.yellow;

            if (pathMovementData.destinationNode.HasValue)
            {
                NodeReader.GetAllLinkedNodes(pathMovementData.destinationNode.Value)
                    .Except(new[] {pathMovementData.currentNode.GetValueOrDefault()})
                    .ForEach(node =>
                    {
                        Gizmos.DrawLine(pathMovementData.destinationNode.Value.Position, node.Position);
                        Gizmos.DrawWireSphere(node.Position, node.PathWidth / 2f);
                    });
            }

            if (pathMovementData.currentNode.HasValue)
            {
                NodeReader.GetAllLinkedNodes(pathMovementData.currentNode.Value)
                    .Except(new[] {pathMovementData.destinationNode.GetValueOrDefault()})
                    .ForEach(node =>
                    {
                        Gizmos.DrawLine(pathMovementData.currentNode.Value.Position, node.Position);
                        Gizmos.DrawWireSphere(node.Position, node.PathWidth / 2f);
                    });
            }
        }
    }

}