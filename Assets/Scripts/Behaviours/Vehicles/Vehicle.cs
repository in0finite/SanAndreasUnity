using UnityEngine;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.Vehicle;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle : MonoBehaviour
    {
        public VehicleDef Definition { get; private set; }

        private void Update()
        {
            foreach (var wheel in _wheels)
            {
                Vector3 position = wheel.Collider.transform.position;

                WheelHit wheelHit;

                if (wheel.Collider.GetGroundHit(out wheelHit))
                {
                    position.y = wheelHit.point.y + wheel.Collider.radius;
                }
                else
                {
                    position.y -= wheel.Collider.suspensionDistance;
                }

                wheel.Child.transform.position = position;

                wheel.Child.Rotate(Vector3.right, wheel.Collider.rpm / 60.0f * 360.0f * Time.deltaTime);
            }
        }
    }
}
