using SanAndreasUnity.Importing.Paths;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class PathMovementData // don't change to struct, it would be large
    {
        public PathNode? currentNode;
        public PathNode? destinationNode;
        public Vector3 moveDestination;
        public float timeWhenAttemptedToFindClosestNode = 0f;

        public void Cleanup()
        {
            this.currentNode = null;
            this.destinationNode = null;
            this.moveDestination = Vector3.zero;
            this.timeWhenAttemptedToFindClosestNode = 0f;
        }
    }

    public class WalkAroundState : BaseState
    {
        private readonly PathMovementData _pathMovementData = new PathMovementData();


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
            if (_enemyPeds.Count > 0)
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
    }
}