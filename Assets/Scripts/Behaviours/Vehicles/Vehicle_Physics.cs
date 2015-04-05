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

            foreach (var wheel in _wheels)
            {
                wheel.Collider = wheel.Parent.gameObject.AddComponent<WheelCollider>();
                wheel.Collider.radius = 0.35f;
                wheel.Collider.suspensionDistance = 0.2f;

                var spring = wheel.Collider.suspensionSpring;
                spring.targetPosition = 1.0f;
                wheel.Collider.suspensionSpring = spring;
            }
        }

        private void FixedUpdate()
        {
            foreach (var wheel in _wheels)
            {
                wheel.Collider.motorTorque = MotorTorque;
            }
        }
    }
}
