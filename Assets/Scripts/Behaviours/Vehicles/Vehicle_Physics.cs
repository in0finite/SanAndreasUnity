using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        public float MotorTorque;

        private void InitializePhysics()
        {
            _geometryParts.AttachCollisionModel(transform, true);

            var rb = gameObject.AddComponent<Rigidbody>();

            rb.mass = 1000f;

            foreach (var wheel in _wheels) {
                var col = wheel.AddComponent<WheelCollider>();
            }
        }

        private void FixedUpdate()
        {
            foreach (var wheel in _wheels) {
                wheel.GetComponent<WheelCollider>().motorTorque = MotorTorque;
            }
        }
    }
}
