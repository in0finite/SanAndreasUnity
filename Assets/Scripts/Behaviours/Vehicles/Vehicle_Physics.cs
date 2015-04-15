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

        [Range(-1, 1)]
        public float Braking;

        public Handling.Car HandlingData { get; private set; }

        private void InitializePhysics()
        {
            _geometryParts.AttachCollisionModel(transform, true);

            _rigidBody = gameObject.AddComponent<Rigidbody>();

            HandlingData = Handling.Get<Handling.Car>(Definition.HandlingName);

            VConsts.Changed += UpdateValues;

            var vals = VConsts.Instance;

            foreach (var wheel in _wheels)
            {
                var front = (wheel.Alignment & WheelAlignment.Front) == WheelAlignment.Front;

                wheel.Parent.position += Vector3.up * (HandlingData.SuspensionUpperLimit + HandlingData.SuspensionLowerLimit);

                wheel.Collider = wheel.Parent.gameObject.AddComponent<WheelCollider>();
                wheel.Collider.radius = (front ? Definition.WheelScaleFront : Definition.WheelScaleRear) * .5f;
                wheel.Collider.suspensionDistance = HandlingData.SuspensionUpperLimit - HandlingData.SuspensionLowerLimit;

                var spring = wheel.Collider.suspensionSpring;
                spring.targetPosition = 0.5f;
                wheel.Collider.suspensionSpring = spring;
            }

            UpdateValues(vals);
        }

        private void UpdateValues(VConsts vals)
        {
            _rigidBody.drag = HandlingData.Drag * vals.DragScale;
            _rigidBody.mass = HandlingData.Mass * vals.MassScale;
            _rigidBody.centerOfMass = HandlingData.CentreOfMass;

            foreach (var wheel in _wheels) {
                var spring = wheel.Collider.suspensionSpring;
                spring.damper = HandlingData.SuspensionDampingLevel * vals.SuspensionDampingScale;
                spring.spring = HandlingData.SuspensionForceLevel * vals.SuspensionForceScale;
                wheel.Collider.suspensionSpring = spring;

                var friction = wheel.Collider.forwardFriction;
                friction.extremumSlip = vals.FrictionExtremumSlip;
                friction.extremumValue = vals.FrictionExtremumValue;
                friction.asymptoteSlip = vals.FrictionAsymptoteSlip;
                friction.asymptoteValue = vals.FrictionAsymptoteValue;
                friction.stiffness = vals.FrictionStiffness;

                wheel.Collider.sidewaysFriction = friction;
            }
        }

        private float DriveBias(Wheel wheel)
        {
            switch (HandlingData.TransmissionDriveType) {
                case DriveType.Forward:
                    return wheel.IsFront ? 1f : 0f;
                case DriveType.Rear:
                    return wheel.IsRear ? 1f : 0f;
                default:
                    return 1f;
            }
        }

        private bool ShouldSteer(Wheel wheel)
        {
            // TODO: look at flags
            return wheel.IsFront;
        }

        private float BrakeBias(Wheel wheel)
        {
            return wheel.IsFront
                ? HandlingData.BrakeBias : wheel.IsRear
                ? 1f - HandlingData.BrakeBias : .5f;
        }

        private void FixedUpdate()
        {
            foreach (var wheel in _wheels)
            {
                if (ShouldSteer(wheel)) {
                    wheel.Collider.steerAngle = HandlingData.SteeringLock * Steering;
                }

                wheel.Collider.motorTorque =
                    Accelerator * HandlingData.TransmissionEngineAccel
                    * VConsts.Instance.AccelerationScale * DriveBias(wheel);

                wheel.Collider.brakeTorque = 
                    Braking * HandlingData.BrakeDecel
                    * VConsts.Instance.BreakingScale * BrakeBias(wheel);
            }
        }
    }
}
