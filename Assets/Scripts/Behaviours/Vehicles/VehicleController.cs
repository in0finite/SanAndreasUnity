using UnityEngine;
using SanAndreasUnity.Net;
using Mirror;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    
    public class VehicleController : NetworkBehaviour
    {
        private Vehicle m_vehicle;

        [SyncVar] int m_net_id;
        [SyncVar] float m_net_acceleration;
        [SyncVar] float m_net_steering;
        [SyncVar] float m_net_braking;
        [SyncVar] Vector3 m_net_linearVelocity;
        [SyncVar] Vector3 m_net_angularVelocity;
        
        // is it better to place syncvars in Vehicle class ? - that way, there is no need for hooks
        // - or we could assign/read syncvars in Update()



        private void Awake()
        {
            //m_vehicle = GetComponent<Vehicle>();
        }

        internal void OnAfterCreateVehicle()
        {
            m_vehicle = this.GetComponent<Vehicle>();
            m_net_id = m_vehicle.Definition.Id;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!NetStatus.IsServer)
            {
                // load vehicle on clients
                F.RunExceptionSafe( () => {
                    m_vehicle = Vehicle.Create(this.gameObject, m_net_id, null, this.transform.position, this.transform.rotation);
                });
            }
        }

        private void Update()
        {
            
            this.ProcessSyncvars();


            var driverSeat = m_vehicle.DriverSeat;

            if (null == driverSeat || null == driverSeat.OccupyingPed)
            {
                if (NetStatus.IsServer)
                    this.ResetInput();
                return;
            }
            
            if (driverSeat.OccupyingPed != Ped.Instance)
                return;
            
            // local ped is occupying driver seat

			if (!GameManager.CanPlayerReadInput())
                this.ResetInput();
            else
                this.ReadInput();

            PedSync.Local.SendVehicleInput(m_vehicle.Accelerator, m_vehicle.Steering, m_vehicle.Braking);
        }

        void ProcessSyncvars()
        {
            if (NetStatus.IsServer)
            {
                m_net_acceleration = m_vehicle.Accelerator;
                m_net_steering = m_vehicle.Steering;
                m_net_braking = m_vehicle.Braking;
                m_net_linearVelocity = m_vehicle.RigidBody.velocity;
                m_net_angularVelocity = m_vehicle.RigidBody.angularVelocity;
            }
            else
            {
                m_vehicle.Accelerator = m_net_acceleration;
                m_vehicle.Steering = m_net_steering;
                m_vehicle.Braking = m_net_braking;
                if (VehicleManager.Instance.syncLinearVelocity)
                    m_vehicle.RigidBody.velocity = m_net_linearVelocity;
                if (VehicleManager.Instance.syncAngularVelocity)
                    m_vehicle.RigidBody.angularVelocity = m_net_angularVelocity;
            }
        }

        void ResetInput()
        {
            m_vehicle.Accelerator = 0;
            m_vehicle.Steering = 0;
            m_vehicle.Braking = 0;
        }

        void ReadInput()
        {
            var accel = Input.GetAxis("Vertical");
            var brake = Input.GetButton("Brake") ? 1.0f : 0.0f;
            var speed = Vector3.Dot(m_vehicle.Velocity, m_vehicle.transform.forward);

            if (speed * accel < 0f)
            {
                brake = Mathf.Max(brake, 0.75f);
                accel = 0f;
            }

            m_vehicle.Accelerator = accel;
            m_vehicle.Steering = Input.GetAxis("Horizontal");
            m_vehicle.Braking = brake;
        }

    }
}