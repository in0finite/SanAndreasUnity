using SanAndreasUnity.Importing.Vehicles;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        public float DragScale = 1 / 100f;
        public float AccelScale = 10f;

        private Rigidbody _rigidBody;

        [Range(-1, 1)]
        public float Accelerator;

        [Range(-1, 1)]
        public float Steering;

        public Handling.Car HandlingData { get; private set; }

        private void InitializePhysics()
        {
            _geometryParts.AttachCollisionModel(transform, true);

            _rigidBody = gameObject.AddComponent<Rigidbody>();

            HandlingData = Handling.Get<Handling.Car>(Definition.HandlingName);

            _rigidBody.mass = HandlingData.Mass;
            _rigidBody.drag = HandlingData.Drag * DragScale;
            _rigidBody.centerOfMass = HandlingData.CentreOfMass;

            foreach (var wheel in _wheels)
            {
                wheel.Collider = wheel.Parent.gameObject.AddComponent<WheelCollider>();
                wheel.Collider.radius = 0.35f;
                wheel.Collider.suspensionDistance = 0.2f;

                var spring = wheel.Collider.suspensionSpring;
                spring.targetPosition = 1.0f;
                spring.damper = HandlingData.SuspensionDampingLevel;
                wheel.Collider.suspensionSpring = spring;
            }
        }

        private void FixedUpdate()
        {
            _rigidBody.drag = HandlingData.Drag * DragScale;

            foreach (var wheel in _wheels)
            {
                if (wheel.Alignment == WheelAlignment.RightFront ||
                    wheel.Alignment == WheelAlignment.LeftFront)
                {
                    wheel.Collider.steerAngle = HandlingData.SteeringLock * Steering;
                }

                wheel.Collider.motorTorque = Accelerator * HandlingData.TransmissionEngineAccel * AccelScale;
            }
        }
    }
}
