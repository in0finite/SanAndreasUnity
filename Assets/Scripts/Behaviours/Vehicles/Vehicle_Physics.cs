using SanAndreasUnity.Importing.Vehicles;
using UnityEngine;
using VConsts = SanAndreasUnity.Behaviours.Vehicles.VehiclePhysicsConstants;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
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

            VConsts.Changed += UpdateValues;

            _rigidBody.mass = HandlingData.Mass;
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

        private void UpdateValues(VConsts vals)
        {
            _rigidBody.drag = HandlingData.Drag * vals.DragScale;
        }

        private void FixedUpdate()
        {
            foreach (var wheel in _wheels)
            {
                if (wheel.Alignment == WheelAlignment.RightFront ||
                    wheel.Alignment == WheelAlignment.LeftFront)
                {
                    wheel.Collider.steerAngle = HandlingData.SteeringLock * Steering;
                }

               
                wheel.Collider.motorTorque = Accelerator
                    * HandlingData.TransmissionEngineAccel
                    * VConsts.Instance.AccelerationScale;
            }
        }
    }
}
