using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Paths;
using UnityEngine;
using UGameCore.Utilities;
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

        private float _timeSinceUpdatedStateOn2Seconds = 0f;

        private List<Ped.UnderAimInfo> _lastUnderAimInfos = null;


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
            Weapon.onWeaponConductedAttack += OnWeaponConductedAttack;
        }

        private void OnDisable()
        {
            s_allPedAIs.Remove(this);
            Ped.onDamaged -= OnPedDamaged;
            Vehicle.onDamaged -= OnVehicleDamaged;
            Weapon.onWeaponConductedAttack -= OnWeaponConductedAttack;
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

        void OnWeaponConductedAttack(Weapon.AttackConductedEventData data)
        {
            this.CurrentState.OnWeaponConductedAttack(data);
        }

        void Update()
        {
            if (NetStatus.IsServer)
            {
                this.MyPed.ResetInput();

                _timeSinceUpdatedStateOn2Seconds += Time.deltaTime;
                if (_timeSinceUpdatedStateOn2Seconds >= 2f)
                {
                    _timeSinceUpdatedStateOn2Seconds = 0f;
                    this.CurrentState.UpdateState2Seconds();
                }

                this.CurrentState.UpdateState();

                this.CheckIfSomeoneAimedAtOurPed();
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
            return targetNode.Position + offset.ToVec3XZ();
        }

        public static void FindClosestWalkableNode(PathMovementData pathMovementData, Vector3 position, float radius = 200f)
        {
            if (Time.timeAsDouble - pathMovementData.timeWhenAttemptedToFindClosestNode < 2f) // don't attempt to find it every frame
                return;

            pathMovementData.timeWhenAttemptedToFindClosestNode = Time.timeAsDouble;

            var closestPathNodeToWalk = PedAI.GetClosestPathNodeToWalk(position, radius);
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
            // ignore Emergency nodes unless they are the only ones connected

            // first try with non-Emergency nodes
            var possibilities = NodeReader.GetAllLinkedNodes(currentNode)
                .Where(_ => !_.Flags.EmergencyOnly && !_.Equals(previousNode))
                .ToList();

            if (possibilities.Count > 0)
                return possibilities.RandomElement();

            // now try with Emergency nodes
            possibilities = NodeReader.GetAllLinkedNodes(currentNode)
                .Where(_ => _.Flags.EmergencyOnly && !_.Equals(previousNode))
                .ToList();

            if (possibilities.Count > 0)
                return possibilities.RandomElement();

            // no possibilities found, return to previous node
            return previousNode;
        }

        public static PathNode? GetClosestPathNodeToWalk(Vector3 pos, float radius)
        {
            var pathNodeInfo = NodeReader.GetAreasInRadius(pos, radius)
                .SelectMany(area => area.PedNodes)
                .Where(node => !node.Flags.EmergencyOnly)
                .Select(node => (node, distance: Vector3.Distance(node.Position, pos)))
                .Where(_ => _.distance < radius)
                .MinBy(_ => _.distance, default);

            if (EqualityComparer<PathNode>.Default.Equals(pathNodeInfo.node, default))
                return null;

            return pathNodeInfo.node;
        }

        private void CheckIfSomeoneAimedAtOurPed()
        {
            var underAimInfos = this.MyPed.UnderAimInfos;

            if (underAimInfos.Count == 0)
            {
                _lastUnderAimInfos = null;
                return;
            }

            // there are active "aims" on our ped

            // check if some of them are new
            // if yes, notify the current state

            if (null == _lastUnderAimInfos)
                _lastUnderAimInfos = new List<Ped.UnderAimInfo>();

            List<Ped.UnderAimInfo> newInfos = null;

            for (int i = 0; i < underAimInfos.Count; i++)
            {
                if (!_lastUnderAimInfos.Contains(underAimInfos[i]))
                {
                    // new one
                    if (null == newInfos)
                        newInfos = new List<Ped.UnderAimInfo>();
                    newInfos.Add(underAimInfos[i]);
                }
            }

            // remember last state of the list
            _lastUnderAimInfos.Clear();
            _lastUnderAimInfos.AddRange(underAimInfos);

            // notify current state
            if (newInfos != null)
            {
                this.CurrentState.OnUnderAim(newInfos);
            }
        }

        public static bool PedSeesHeadOfAnotherPed(Ped ped, Ped anotherPed)
        {
            // first check angle
            Vector3 dirToTargetPed = (anotherPed.transform.position - ped.transform.position).normalized;
            if (Vector3.Angle(dirToTargetPed, ped.transform.forward) > 80f)
                return false;


            var targetHead = anotherPed.PlayerModel.Head;
            var targetJaw = anotherPed.PlayerModel.Jaw;
            
            var sourceHead = ped.PlayerModel.Head;

            if (null == targetHead || null == targetJaw || null == sourceHead)
                return false;

            LayerMask layerMask = ~ (Ped.LayerMask & Vehicle.LayerMask);

            // this raycast may not work always, because animation state and skeleton updates are done separately
            // so here, we probably raycast against animation state

            foreach (Transform target in new[] { targetHead, targetJaw })
            {
                Vector3 dir = (target.position - sourceHead.position).normalized;
                if (Physics.Raycast(sourceHead.position, dir, out RaycastHit hit, 30f, layerMask))
                {
                    if (hit.transform.GetComponentInParent<Ped>() == anotherPed)
                        return true;
                }
            }

            return false;
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