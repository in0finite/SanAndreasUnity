using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class MapObjectActivator : MonoBehaviour
    {
        public MapObject MapObject { get; set; }
        private int _numCurrentCollisions = 0;

        private void OnTriggerEnter(Collider other)
        {
            _numCurrentCollisions++;
            if (_numCurrentCollisions == 1)
                this.MapObject.Show();
        }

        private void OnTriggerExit(Collider other)
        {
            _numCurrentCollisions--;
            if (_numCurrentCollisions == 0)
                this.MapObject.UnShow();
        }
    }
}
