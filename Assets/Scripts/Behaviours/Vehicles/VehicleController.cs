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
            var accel = Input.GetAxis("Vertical");
            var brake = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
            var speed = Vector3.Dot(_vehicle.Velocity, _vehicle.transform.forward);

            if (speed * accel < 0f) {
                brake = Mathf.Max(brake, 0.75f);
                accel = 0f;
            }

            _vehicle.Accelerator = accel;
            _vehicle.Steering = Input.GetAxis("Horizontal");
            _vehicle.Braking = brake;
        }
    }
}
