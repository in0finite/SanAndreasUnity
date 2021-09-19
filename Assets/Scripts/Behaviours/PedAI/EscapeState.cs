using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class EscapeState : BaseState
    {
        private readonly PathMovementData _pathMovementData = new PathMovementData();


        public override void UpdateState()
        {
            // TODO: exit vehicle

            if (PedAI.ArrivedAtDestinationNode(_pathMovementData, _ped.transform))
                PedAI.OnArrivedToDestinationNode(_pathMovementData);

            if (!_pathMovementData.destinationNode.HasValue)
            {
                PedAI.FindClosestWalkableNode(_pathMovementData, _ped.transform.position);
                return;
            }

            this.MyPed.IsSprintOn = true;
            this.MyPed.Movement = (_pathMovementData.moveDestination - this.MyPed.transform.position).normalized;
            this.MyPed.Heading = this.MyPed.Movement;
        }
    }
}