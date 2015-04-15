using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    [RequireComponent(typeof(Vehicle))]
    public class VehicleController : MonoBehaviour
    {
        private Vehicle _vehicle;

        private void Awake()
        {
            _vehicle = GetComponent<Vehicle>();
        }

        private void Update()
        {
            _vehicle.Accelerator = Input.GetAxis("Vertical");
            _vehicle.Steering = Input.GetAxis("Horizontal");
            _vehicle.Braking = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
        }
    }
}
