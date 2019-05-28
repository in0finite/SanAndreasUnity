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
        bool IsControlledByLocalPlayer => m_vehicle.IsControlledByLocalPlayer;

        [SyncVar] int m_net_id;
        [SyncVar] string m_net_carColors;
        [SyncVar] float m_net_acceleration;
        [SyncVar] float m_net_steering;
        [SyncVar] float m_net_braking;
        [SyncVar(hook=nameof(OnNetPositionChanged))] Vector3 m_net_position;
        [SyncVar(hook=nameof(OnNetRotationChanged))] Quaternion m_net_rotation;
        [SyncVar] Vector3 m_net_linearVelocity;
        [SyncVar] Vector3 m_net_angularVelocity;

        struct WheelSyncData
        {
            public float brakeTorque;
            public float motorTorque;
            public float steerAngle;
            //public float travel;
        }

        class WheelSyncList : SyncList<WheelSyncData> { }

        WheelSyncList m_net_wheelsData;
        
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
            
            // if syncvars are used for updating transform, then disable NetworkTransform, and vice versa
            m_vehicle.NetTransform.enabled = ! VehicleManager.Instance.syncVehicleTransformUsingSyncVars;

            // update status of rigid body
            this.EnableOrDisableRigidBody();


            this.ProcessSyncvars();


            var driverSeat = m_vehicle.DriverSeat;

            if (null == driverSeat || null == driverSeat.OccupyingPed)
            {
                if (NetStatus.IsServer)
                    this.ResetInput();
                return;
            }
            
            if (null == Ped.Instance || driverSeat.OccupyingPed != Ped.Instance)
                return;
            
            // local ped is occupying driver seat

            float oldAcc = m_vehicle.Accelerator;
            float oldBrake = m_vehicle.Braking;
            float oldSteer = m_vehicle.Steering;

			if (!GameManager.CanPlayerReadInput())
                this.ResetInput();
            else
                this.ReadInput();

            // why do we send input ?
            // - so that everyone knows if the gas/brake is pressed, and can simulate wheel effects
            // - so that server can predict position and velocity of rigid body
            PedSync.Local.SendVehicleInput(m_vehicle.Accelerator, m_vehicle.Steering, m_vehicle.Braking);

            // TODO: also send velocity of rigid body

            if (!NetStatus.IsServer && !VehicleManager.Instance.controlInputOnLocalPlayer)
            {
                // local player should not control input, so restore old input
                m_vehicle.Accelerator = oldAcc;
                m_vehicle.Braking = oldBrake;
                m_vehicle.Steering = oldSteer;
            }

        }

        void ProcessSyncvars()
        {
            if (NetStatus.IsServer)
            {
                m_net_acceleration = m_vehicle.Accelerator;
                m_net_steering = m_vehicle.Steering;
                m_net_braking = m_vehicle.Braking;
                m_net_position = m_vehicle.transform.position;
                m_net_rotation = m_vehicle.transform.rotation;
                m_net_linearVelocity = m_vehicle.RigidBody.velocity;
                m_net_angularVelocity = m_vehicle.RigidBody.angularVelocity;

                // wheels
                m_net_wheelsData.Clear();
                foreach (var wheel in m_vehicle.Wheels) {
                    m_net_wheelsData.Add(new WheelSyncData() {
                        brakeTorque = wheel.Collider.brakeTorque,
                        motorTorque = wheel.Collider.motorTorque,
                        steerAngle = wheel.Collider.steerAngle,
                        //travel = wheel.Travel,
                    });
                }
            }
            else
            {
                // apply input
                if (!this.IsControlledByLocalPlayer || (this.IsControlledByLocalPlayer && !VehicleManager.Instance.controlInputOnLocalPlayer))
                {
                    m_vehicle.Accelerator = m_net_acceleration;
                    m_vehicle.Steering = m_net_steering;
                    m_vehicle.Braking = m_net_braking;
                }

                // update wheels
                if (!this.IsControlledByLocalPlayer || (this.IsControlledByLocalPlayer && !VehicleManager.Instance.controlWheelsOnLocalPlayer))
                {
                    for (int i=0; i < m_vehicle.Wheels.Count && i < m_net_wheelsData.Count; i++) {
                        var w = m_vehicle.Wheels[i];
                        var data = m_net_wheelsData[i];
                        w.Collider.brakeTorque = data.brakeTorque;
                        w.Collider.motorTorque = data.motorTorque;
                        w.Collider.steerAngle = data.steerAngle;
                        //w.Travel = data.travel;
                    }
                }

                // position and rotation will be applied in syncvar hooks

                // apply velocity on all clients
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

        public void EnableOrDisableRigidBody()
        {
            if (NetStatus.IsServer)
            {
                F.EnableRigidBody(m_vehicle.RigidBody);
                return;
            }
            
            if (VehicleManager.Instance.whenToDisableRigidBody.Matches(this.IsControlledByLocalPlayer, !NetStatus.IsServer))
                F.DisableRigidBody(m_vehicle.RigidBody);
            else
                F.EnableRigidBody(m_vehicle.RigidBody);
            
        }

        void OnNetPositionChanged(Vector3 pos)
        {
            if (NetStatus.IsServer)
                return;

            if (VehicleManager.Instance.syncVehicleTransformUsingSyncVars) {
                if (m_vehicle != null && m_vehicle.RigidBody != null)
                    m_vehicle.RigidBody.MovePosition(pos);
            }
        }

        void OnNetRotationChanged(Quaternion rot)
        {
            if (NetStatus.IsServer)
                return;

            if (VehicleManager.Instance.syncVehicleTransformUsingSyncVars) {
                if (m_vehicle != null && m_vehicle.RigidBody != null)
                    m_vehicle.RigidBody.MoveRotation(rot);
            }
        }

    }
}