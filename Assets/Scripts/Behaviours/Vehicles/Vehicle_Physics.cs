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

        [Range(-45, 45)]
        public float SteerAngle;

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
<<<<<<< HEAD
                wheel.Collider.motorTorque = MotorTorque;

                if (wheel.Alignment == WheelAlignment.RightFront ||
                    wheel.Alignment == WheelAlignment.LeftFront)
                {
                    wheel.Collider.steerAngle = SteerAngle;
                }
=======
                wheel.Collider.motorTorque = Accelerator * HandlingData.TransmissionEngineAccel * AccelScale;
>>>>>>> 1c8c37b8b39669b3b50276c2f273f124bc2aeb93
            }
        }
    }
}
