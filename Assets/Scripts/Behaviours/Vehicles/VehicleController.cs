using UnityEngine;
using SanAndreasUnity.Net;

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
            
            var driverSeat = _vehicle.DriverSeat;

            if (null == driverSeat || null == driverSeat.OccupyingPed)
            {
                if (NetStatus.IsServer)
                    this.ResetInput();
                return;
            }
            
            if (driverSeat.OccupyingPed != Ped.Instance)
                return;
            
            // local ped is occupying driver seat

            this.ResetInput();

			if (!GameManager.CanPlayerReadInput()) return;

            var accel = Input.GetAxis("Vertical");
            var brake = Input.GetButton("Brake") ? 1.0f : 0.0f;
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

        void ResetInput()
        {
            _vehicle.Accelerator = 0;
            _vehicle.Steering = 0;
            _vehicle.Braking = 0;
        }
    }
}