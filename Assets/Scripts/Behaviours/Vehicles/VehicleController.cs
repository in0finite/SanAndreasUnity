using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    [RequireComponent(typeof(Vehicle))]
    public class VehicleController : MonoBehaviour
    {
        private Vehicle _vehicle;
        private PlayerController _playerController;

        private void Awake()
        {
            _vehicle = GetComponent<Vehicle>();
            _playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        }

        private void Update()
        {
			if (!GameManager.CanPlayerReadInput()) return;

            var accel = Input.GetAxis("Vertical");
            var brake = Input.GetButton("Walk") ? 1.0f : 0.0f;
            var speed = Vector3.Dot(_vehicle.Velocity, _vehicle.transform.forward);

            if (speed * accel < 0f)
            {
                brake = Mathf.Max(brake, 0.75f);
                accel = 0f;
            }

            _vehicle.Accelerator = accel;
            _vehicle.Steering = Input.GetAxis("Horizontal");
            _vehicle.Braking = brake;
        }
    }
}