using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class EscapeState : BaseState, IPathMovementState
    {
        private readonly PathMovementData _pathMovementData = new PathMovementData();
        public PathMovementData PathMovementData => _pathMovementData;

        public float nodeSearchRadius = 500f;


        public override void OnBecameInactive()
        {
            _ped.MovementAgent.Destination = null;

            base.OnBecameInactive();
        }

        public override void UpdateState()
        {
            // TODO: exit vehicle

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
                this.MyPed.IsPanicButtonOn = true;
                this.MyPed.Movement = moveInput;
                this.MyPed.Heading = moveInput;
            }
        }

        protected internal override void OnDrawGizmosSelected()
        {
            PedAI.OnDrawGizmosSelected(_pathMovementData);
        }
    }
}