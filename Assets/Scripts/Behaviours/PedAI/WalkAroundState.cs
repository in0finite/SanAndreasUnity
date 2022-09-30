using SanAndreasUnity.Behaviours.Peds.States;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Paths;
using UGameCore.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class WalkAroundState : BaseState, IPathMovementState
    {
        private readonly PathMovementData _pathMovementData = new PathMovementData();
        public PathMovementData PathMovementData => _pathMovementData;

        private ChaseState _chaseState;

        private double _timeWhenStartedSurrendering = 0;
        public double TimeSinceStartedSurrendering => Time.timeAsDouble - _timeWhenStartedSurrendering;
        public bool IsSurrendering => this.TimeSinceStartedSurrendering < 4.0;

        public float nodeSearchRadius = 500f;


        protected internal override void OnAwake(PedAI pedAI)
        {
            base.OnAwake(pedAI);

            _chaseState = _pedAI.StateContainer.GetStateOrThrow<ChaseState>();
        }

        public override void OnBecameActive()
        {
            base.OnBecameActive();

            if (this.ParameterForEnteringState is PathNode pathNode)
            {
                _pathMovementData.currentNode = _pathMovementData.destinationNode = pathNode;
                _pathMovementData.moveDestination = PedAI.GetMoveDestinationBasedOnTargetNode(pathNode);
            }
            else
                _pathMovementData.Cleanup();
        }

        public override void OnBecameInactive()
        {
            _ped.MovementAgent.Destination = null;

            base.OnBecameInactive();
        }

        public override void UpdateState()
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
            if (_enemyPeds.Count > 0 && _chaseState.CanStartChasing())
            {
                _pedAI.StartChasing();
                return;
            }

            if (this.IsSurrendering)
            {
                // make sure ped is surrendering
                if (!_ped.IsSurrendering())
                    _ped.OnSurrenderButtonPressed();

                return;
            }

            if (PedAI.ArrivedAtDestinationNode(_pathMovementData, _ped.transform))
            {
                PedAI.OnArrivedToDestinationNode(_pathMovementData);
                if (_pathMovementData.destinationNode.HasValue)
                {
                    _ped.MovementAgent.Destination = _pathMovementData.moveDestination;
                    _ped.MovementAgent.RunUpdate();
                }
            }

            if (!_pathMovementData.destinationNode.HasValue)
            {
                PedAI.FindClosestWalkableNode(_pathMovementData, _ped.transform.position, this.nodeSearchRadius);
                return;
            }

            _ped.MovementAgent.Destination = _pathMovementData.moveDestination;
            _ped.MovementAgent.StoppingDistance = 0f;

            Vector3 moveInput = _ped.MovementAgent.DesiredDirectionXZ;

            if (moveInput != Vector3.zero)
            {
                this.MyPed.IsWalkOn = true;
                this.MyPed.Movement = moveInput;
                this.MyPed.Heading = moveInput;
            }
        }

        protected internal override void OnMyPedDamaged(DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            _pedAI.StateContainer.GetStateOrThrow<IdleState>().HandleOnMyPedDamaged(dmgInfo, dmgResult);
        }

        protected internal override void OnWeaponConductedAttack(Weapon.AttackConductedEventData data)
        {
            _pedAI.StateContainer.GetStateOrThrow<IdleState>().HandleOnWeaponConductedAttack(data);
        }

        protected internal override void OnUnderAim(IReadOnlyList<Ped.UnderAimInfo> underAimInfos)
        {
            if (_pedAI.PedestrianType.IsGangMember())
                return;

            // find those peds that are visible by our ped

            List<Ped.UnderAimInfo> visibleList = null;

            for (int i = 0; i < underAimInfos.Count; i++)
            {
                if (PedAI.PedSeesHeadOfAnotherPed(_ped, underAimInfos[i].ped))
                {
                    if (null == visibleList)
                        visibleList = new List<Ped.UnderAimInfo>();
                    visibleList.Add(underAimInfos[i]);
                }
            }

            if (visibleList != null)
            {
                if (_pedAI.PedestrianType.IsCop())
                {
                    visibleList.ForEach(_ => _enemyPeds.AddIfNotPresent(_.ped));
                    return;
                }

                _timeWhenStartedSurrendering = Time.timeAsDouble;
            }

        }

        protected internal override void OnRecruit(Ped recruiterPed)
        {
            _pedAI.StateContainer.GetStateOrThrow<IdleState>().HandleOnRecruit(recruiterPed);
        }

        protected internal override void OnDrawGizmosSelected()
        {
            PedAI.OnDrawGizmosSelected(_pathMovementData);
        }
    }
}