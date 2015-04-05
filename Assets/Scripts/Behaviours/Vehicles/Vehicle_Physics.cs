using SanAndreasUnity.Importing.Vehicles;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        [Range(-500, 500)]
        public float MotorTorque;

        public Handling.Car HandlingData { get; private set; }

        private void InitializePhysics()
        {
            _geometryParts.AttachCollisionModel(transform, true);

            var rb = gameObject.AddComponent<Rigidbody>();

            HandlingData = Handling.Get<Handling.Car>(Definition.HandlingName);

            rb.mass = HandlingData.Mass;

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
