using UnityEngine;
using SanAndreasUnity.Net;
using Mirror;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    
    public class VehicleController : NetworkBehaviour
    {
        private Vehicle m_vehicle;

        [SyncVar] int m_net_id;
        [SyncVar] string m_net_carColors;
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
            if (m_vehicle.Colors != null)
                m_net_carColors = string.Join(";", m_vehicle.Colors);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!NetStatus.IsServer)
            {
                F.RunExceptionSafe( () => {
                    // load vehicle on clients
                    int[] colors = string.IsNullOrEmpty(m_net_carColors) ? null : m_net_carColors.Split(';').Select(s => int.Parse(s)).ToArray();
                    m_vehicle = Vehicle.Create(this.gameObject, m_net_id, colors, this.transform.position, this.transform.rotation);
                    
                    // update rigid body status
                    this.EnableOrDisableRigidBody();
                });
            }
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            this.EnableOrDisableRigidBody();
        }

        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
            this.EnableOrDisableRigidBody();
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
            
            if (!this.hasAuthority || null == Ped.Instance || driverSeat.OccupyingPed != Ped.Instance)
                return;
            
            // local ped is occupying driver seat

			if (!GameManager.CanPlayerReadInput())
                this.ResetInput();
            else
                this.ReadInput();

            // why do we send input ?
            // - so that everyone knows if the gas/brake is pressed, and can simulate wheel effects
            // - so that server can predict position and velocity of rigid body
            PedSync.Local.SendVehicleInput(m_vehicle.Accelerator, m_vehicle.Steering, m_vehicle.Braking);

            // TODO: also send velocity of rigid body

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
            else if (!this.hasAuthority)    // don't do it on client who controls vehicle
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

        void EnableOrDisableRigidBody()
        {
            if (NetStatus.IsServer || this.hasAuthority)
            {
                // enable rigid body
                m_vehicle.RigidBody.isKinematic = false;
                m_vehicle.RigidBody.detectCollisions = true;
            }
            else
            {
                // disable rigid body
                if (VehicleManager.Instance.disableRigidBodyOnClients)
                {
                    m_vehicle.RigidBody.isKinematic = true;
                    m_vehicle.RigidBody.detectCollisions = false;
                }
            }
        }

    }
}