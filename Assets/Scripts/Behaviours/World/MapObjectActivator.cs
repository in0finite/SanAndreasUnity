using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class MapObjectActivator : MonoBehaviour
    {
        private MapObject _mapObject;
        private int _numCurrentCollisions = 0;

        private void Awake()
        {
            _mapObject = this.transform.GetChild(0).GetComponentOrThrow<MapObject>();
            this.GetComponentOrThrow<SphereCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            _numCurrentCollisions++;
            if (_numCurrentCollisions == 1)
                _mapObject.Show();
        }

        private void OnTriggerExit(Collider other)
        {
            _numCurrentCollisions--;
            if (_numCurrentCollisions == 0)
                _mapObject.UnShow();
        }
    }
}
