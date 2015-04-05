using UnityEngine;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.Vehicle;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle : MonoBehaviour
    {
        public VehicleDef Definition { get; private set; }

        private void Update()
        {
            foreach (var wheel in _wheels) {
                wheel.transform.Rotate(Vector3.left, Time.deltaTime * 500.0f);
            }
        }
    }
}
