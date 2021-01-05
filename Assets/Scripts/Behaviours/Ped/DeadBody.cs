using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds
{
    public class DeadBody : MonoBehaviour
    {
        private static List<DeadBody> _deadBodies = new List<DeadBody>();
        public static IEnumerable<DeadBody> DeadBodies => _deadBodies;
        public static int NumDeadBodies => _deadBodies.Count;

        private void OnEnable()
        {
            _deadBodies.Add(this);
        }

        private void OnDisable()
        {
            _deadBodies.Remove(this);
        }
    }
}
