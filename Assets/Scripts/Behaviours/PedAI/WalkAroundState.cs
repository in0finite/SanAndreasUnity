using SanAndreasUnity.Importing.Paths;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class WalkAroundState : BaseState, IPathMovementState
    {
        private readonly PathMovementData _pathMovementData = new PathMovementData();
        public PathMovementData PathMovementData => _pathMovementData;

        private ChaseState _chaseState;


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

            if (PedAI.ArrivedAtDestinationNode(_pathMovementData, _ped.transform))
                PedAI.OnArrivedToDestinationNode(_pathMovementData);

            if (!_pathMovementData.destinationNode.HasValue)
            {
                PedAI.FindClosestWalkableNode(_pathMovementData, _ped.transform.position);
                return;
            }

            this.MyPed.IsWalkOn = true;
            this.MyPed.Movement = (_pathMovementData.moveDestination - this.MyPed.transform.position).normalized;
            this.MyPed.Heading = this.MyPed.Movement;
        }

        protected internal override void OnMyPedDamaged(DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            _pedAI.StateContainer.GetStateOrThrow<IdleState>().HandleOnMyPedDamaged(dmgInfo, dmgResult);
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