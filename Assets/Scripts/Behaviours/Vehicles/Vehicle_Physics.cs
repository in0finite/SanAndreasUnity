using SanAndreasUnity.Importing.Vehicles;
using UGameCore.Utilities;
using System.Linq;
using UnityEngine;
using VConsts = SanAndreasUnity.Behaviours.Vehicles.VehiclePhysicsConstants;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        private Rigidbody _rigidBody;
        public Rigidbody RigidBody => _rigidBody;

        public struct VehicleInput
        {
            public float accelerator;
            public float steering;
            public bool isHandBrakeOn;

            public void Reset()
            {
                this.accelerator = 0f;
                this.steering = 0f;
                this.isHandBrakeOn = false;
            }

            public void Validate()
            {
                if (float.IsNaN(this.accelerator))
                    this.accelerator = 0f;
                if (float.IsNaN(this.steering))
                    this.steering = 0f;
                this.accelerator = Mathf.Clamp(this.accelerator, -1f, 1f);
                this.steering = Mathf.Clamp(this.steering, -1f, 1f);
            }
        }

        private VehicleInput m_input;
        public VehicleInput Input
        {
            get => m_input;
            set
            {
                m_input = value;
                m_input.Validate();
            }
        }

        public Vector3 Velocity { get { return _rigidBody != null ? _rigidBody.velocity : Vector3.zero; } }

        public float AverageWheelHeight { get { return _wheels.Count == 0 ? transform.position.y : _wheels.Average(x => x.Child.position.y); } }

        public Handling.Car HandlingData { get; private set; }

        private void InitializePhysics()
        {
            _geometryParts.AttachCollisionModel(transform, true);

            _rigidBody = gameObject.GetComponentOrThrow<Rigidbody>();

            _rigidBody.interpolation = Net.NetStatus.IsServer ? VehicleManager.Instance.rigidbodyInterpolationOnServer : VehicleManager.Instance.rigidbodyInterpolationOnClient;
            _rigidBody.collisionDetectionMode = VehicleManager.Instance.rigidBodyCollisionDetectionMode;

            if (Net.NetStatus.IsClientOnly)
            {
                _rigidBody.useGravity = false;
                _rigidBody.detectCollisions = false;
            }

            HandlingData = Handling.Get<Handling.Car>(Definition.HandlingName);

            VConsts.Changed += UpdateValues;

            var vals = VConsts.Instance;

            foreach (var wheel in _wheels)
            {
                var front = (wheel.Alignment & WheelAlignment.Front) == WheelAlignment.Front;

                wheel.Parent.position -= Vector3.up * HandlingData.SuspensionLowerLimit;

                var scale = front ? Definition.WheelScaleFront : Definition.WheelScaleRear;

                var mf = wheel.Child.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    var size = mf.sharedMesh.bounds.size.y;
                    wheel.Child.localScale = Vector3.one * scale / size;
                }

                wheel.Collider = wheel.Parent.gameObject.AddComponent<WheelCollider>();
                wheel.Collider.radius = scale * .5f;
                wheel.Collider.suspensionDistance = HandlingData.SuspensionUpperLimit - HandlingData.SuspensionLowerLimit;
            }

            UpdateValues(vals);
        }

        private void UpdateValues(VConsts vals)
        {
            if (_rigidBody != null)
            {
                _rigidBody.drag = HandlingData.Drag * vals.DragScale;
                _rigidBody.mass = HandlingData.Mass * vals.MassScale;
                _rigidBody.centerOfMass = HandlingData.CentreOfMass;
            }

            foreach (var wheel in _wheels)
            {
                if (null == wheel.Collider)
                    continue;

                var spring = wheel.Collider.suspensionSpring;

                spring.damper = HandlingData.SuspensionDampingLevel * vals.SuspensionDampingScale;
                spring.spring = HandlingData.SuspensionForceLevel * vals.SuspensionForceScale;
                spring.targetPosition = 0.5f;

                wheel.Collider.suspensionSpring = spring;

                var friction = wheel.Collider.sidewaysFriction;
                friction.extremumSlip = vals.SideFrictionExtremumSlip;
                friction.extremumValue = vals.SideFrictionExtremumValue;
                friction.asymptoteSlip = vals.SideFrictionAsymptoteSlip;
                friction.asymptoteValue = vals.SideFrictionAsymptoteValue;
                friction.stiffness = 1f;
                wheel.Collider.sidewaysFriction = friction;

                friction = wheel.Collider.forwardFriction;
                friction.extremumSlip = vals.ForwardFrictionExtremumSlip;
                friction.extremumValue = vals.ForwardFrictionExtremumValue;
                friction.asymptoteSlip = vals.ForwardFrictionAsymptoteSlip;
                friction.asymptoteValue = vals.ForwardFrictionAsymptoteValue;
                friction.stiffness = 1f;
                wheel.Collider.forwardFriction = friction;
            }
        }

        private float DriveBias(Wheel wheel)
        {
            switch (HandlingData.TransmissionDriveType)
            {
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
                ? 1f - HandlingData.BrakeBias : wheel.IsRear
                ? HandlingData.BrakeBias : .5f;
        }

        private void PhysicsFixedUpdate()
        {
            if (m_isServer || (this.IsControlledByLocalPlayer && VehicleManager.Instance.controlWheelsOnLocalPlayer))
            {
                this.UpdateWheelsPhysics();
            }

            if (m_isServer)
            {
                this.AddAntiRollForceToRigidBody();
            }

        }

        void UpdateWheelsPhysics()
        {
            var vals = VConsts.Instance;

            Vector3 velocity = this.Velocity;

            float movementSign = 1f;
            if (velocity == Vector3.zero)
                movementSign = 0f;
            else if (Vector3.Angle(velocity, this.transform.forward) > 90f) // going in reverse
                movementSign = -1f;
            
            float acc = m_input.accelerator;
            float brake = m_input.isHandBrakeOn ? 1f : 0f;
            if (acc != 0f && movementSign != 0f && Mathf.Sign(movementSign) != Mathf.Sign(acc))
            {
                acc = 0f;
                brake = 1f;
            }

            foreach (var wheel in _wheels)
            {
                // apply steering
                if (ShouldSteer(wheel))
                {
                    wheel.Collider.steerAngle = HandlingData.SteeringLock * m_input.steering;
                }

                // apply motor torque
                wheel.Collider.motorTorque =
                    acc * HandlingData.TransmissionEngineAccel
                    * vals.AccelerationScale * DriveBias(wheel);

                // apply brake torque
                wheel.Collider.brakeTorque =
                    brake * HandlingData.BrakeDecel
                    * vals.BreakingScale * BrakeBias(wheel);

                // update travel
                if (wheel.Complement != null) wheel.UpdateTravel();
            }
        }

        /// <summary>
		/// Adds upward force to rigid body based on difference of travel between complemented wheels.
		/// </summary>
        void AddAntiRollForceToRigidBody()
        {
            foreach (var wheel in _wheels.Where(x => x.Complement != null))
            {
                if (wheel.Travel == wheel.Complement.Travel) continue;
                if (!wheel.Collider.isGrounded) continue;

                var force = (wheel.Complement.Travel - wheel.Travel) * VConsts.Instance.AntiRollScale;
                _rigidBody.AddForceAtPosition(wheel.Parent.transform.up * force, wheel.Parent.position);
            }
        }

    }
}