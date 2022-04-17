using SanAndreasUnity.Importing.Paths;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class PathMovementData // don't change to struct, it would be large
    {
        public PathNode? currentNode;
        public PathNode? destinationNode;
        public Vector3 moveDestination;
        public double timeWhenAttemptedToFindClosestNode = 0;

        public void Cleanup()
        {
            this.currentNode = null;
            this.destinationNode = null;
            this.moveDestination = Vector3.zero;
            this.timeWhenAttemptedToFindClosestNode = 0;
        }
    }
}
