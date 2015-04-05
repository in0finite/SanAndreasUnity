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
