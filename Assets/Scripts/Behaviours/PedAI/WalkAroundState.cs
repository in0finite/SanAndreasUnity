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
    }

    public class WalkAroundState : BaseState
    {
        private readonly PathMovementData _pathMovementData = new PathMovementData();
        private float _timeWhenAttemptedToFindClosestNode = 0f;


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
                if (Time.time - _timeWhenAttemptedToFindClosestNode > 2f) // don't attempt to find it every frame
                {
                    _timeWhenAttemptedToFindClosestNode = Time.time;

                    var closestPathNodeToWalk = PedAI.GetClosestPathNodeToWalk(_ped.transform.position);
                    if (null == closestPathNodeToWalk)
                        return;

                    _pathMovementData.destinationNode = closestPathNodeToWalk;
                    _pathMovementData.moveDestination = PedAI.GetMoveDestinationBasedOnTargetNode(closestPathNodeToWalk.Value);
                }

                return;
            }

            this.MyPed.IsWalkOn = true;
            this.MyPed.Movement = (_pathMovementData.moveDestination - this.MyPed.transform.position).normalized;
            this.MyPed.Heading = this.MyPed.Movement;
        }
    }
}