using UnityEngine;
using SanAndreasUnity.Net;
using Mirror;
using SanAndreasUnity.Utilities;
using System.Linq;
using System.Collections.Generic;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    
    public class VehicleController : NetworkBehaviour
    {
        private Vehicle m_vehicle;
        bool IsControlledByLocalPlayer => m_vehicle.IsControlledByLocalPlayer;

        [SyncVar] int m_net_id;
        [SyncVar(hook = nameof(OnNetColorsChanged))] string m_net_carColors;
        [SyncVar] float m_net_acceleration;
        [SyncVar] float m_net_steering;
        [SyncVar] float m_net_braking;
        [SyncVar(hook=nameof(OnNetPositionChanged))] Vector3 m_net_position;
        [SyncVar(hook=nameof(OnNetRotationChanged))] Quaternion m_net_rotation;
        [SyncVar] Vector3 m_net_linearVelocity;
        [SyncVar] Vector3 m_net_angularVelocity;
        [SyncVar] float m_net_health;

        struct WheelSyncData
        {
            public float brakeTorque;
            public float motorTorque;
            public float steerAngle;
            //public float travel;
            public float localPosY;
            public float rpm;
        }

        class WheelSyncList : SyncList<WheelSyncData> { }

        WheelSyncList m_net_wheelsData;

        private readonly SyncedBag.StringSyncDictionary m_syncDictionary = new SyncedBag.StringSyncDictionary();
        public SyncedBag ExtraData { get; }

        // is it better to place syncvars in Vehicle class ? - that way, there is no need for hooks
        // - or we could assign/read syncvars in Update()



        private VehicleController()
        {
            ExtraData = new SyncedBag(m_syncDictionary);
        }

        private void Awake()
        {
            //m_vehicle = GetComponent<Vehicle>();
        }

        private void OnDisable()
        {
            if (m_vehicle != null)
                m_vehicle.onColorsChanged -= this.OnColorsChanged;
        }

        internal void OnAfterCreateVehicle()
        {
            m_vehicle = this.GetComponentOrThrow<Vehicle>();
            m_vehicle.onColorsChanged += this.OnColorsChanged;
            m_net_id = m_vehicle.Definition.Id;
            m_net_carColors = SerializeColors(m_vehicle.Colors);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!NetStatus.IsServer)
            {
                F.RunExceptionSafe( () => {
                    // load vehicle on clients

                    Color32[] colors = DeserializeColors(m_net_carColors);

                    m_vehicle = Vehicle.Create(this.gameObject, m_net_id, null, this.transform.position, this.transform.rotation);

                    m_vehicle.SetColors(colors);
                    
                    // update rigid body status
                    this.EnableOrDisableRigidBody();

                    if (VehicleManager.Instance.destroyWheelCollidersOnClient)
                    {
                        foreach (var wheelCollider in this.GetComponentsInChildren<WheelCollider>())
                        {
                            Destroy(wheelCollider);
                        }
                    }
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

        public static string SerializeColors(IEnumerable<Color32> colors)
        {
            return colors != null
                ? string.Join("|", colors.Select(c => SerializeColor(c)))
                : null;
        }

        public static Color32[] DeserializeColors(string colors)
        {
            return string.IsNullOrEmpty(colors)
                ? null
                : colors.Split('|').Select(s => DeserializeColor(s)).ToArray();
        }

        public static string SerializeColor(Color32 color)
        {
            byte[] values = new byte[] { color.r, color.g, color.b, color.a };
            return string.Join(";", values.Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        public static Color32 DeserializeColor(string colorString)
        {
            string[] splits = colorString.Split(';');

            if (splits.Length != 4)
                throw new System.ArgumentException($"Failed to deserialize color - expected 4 components, found {splits.Length}");

            return new Color32(
                byte.Parse(splits[0], System.Globalization.CultureInfo.InvariantCulture),
                byte.Parse(splits[1], System.Globalization.CultureInfo.InvariantCulture),
                byte.Parse(splits[2], System.Globalization.CultureInfo.InvariantCulture),
                byte.Parse(splits[3], System.Globalization.CultureInfo.InvariantCulture));
        }

        private void OnColorsChanged()
        {
            if (!NetStatus.IsServer)
                return;

            m_net_carColors = SerializeColors(m_vehicle.Colors);
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
                m_net_position = m_vehicle.RigidBody.position;
                m_net_rotation = m_vehicle.RigidBody.rotation;
                m_net_linearVelocity = m_vehicle.RigidBody.velocity;
                m_net_angularVelocity = m_vehicle.RigidBody.angularVelocity;
                m_net_health = m_vehicle.Health;

                // wheels
                m_net_wheelsData.Flush();   // remove current list of changes - this ensures that only the current wheel state is sent, and prevents memory leak bug in Mirror
                m_net_wheelsData.Clear();
                foreach (var wheel in m_vehicle.Wheels) {
                    m_net_wheelsData.Add(new WheelSyncData() {
                        brakeTorque = wheel.Collider.brakeTorque,
                        motorTorque = wheel.Collider.motorTorque,
                        steerAngle = wheel.Collider.steerAngle,
                        //travel = wheel.Travel,
                        localPosY = wheel.Child.localPosition.y,
                        rpm = wheel.Collider.rpm,
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

                        if (w.Collider != null)
                        {
                            w.Collider.brakeTorque = data.brakeTorque;
                            w.Collider.motorTorque = data.motorTorque;
                            w.Collider.steerAngle = data.steerAngle;
                        }
                        //w.Travel = data.travel;
                        w.Child.SetLocalY(data.localPosY);
                        Vehicle.UpdateWheelRotation(w, data.rpm, data.steerAngle);
                    }
                }

                // position and rotation will be applied in syncvar hooks

                // apply velocity on all clients

                if (VehicleManager.Instance.syncLinearVelocity)
                    m_vehicle.RigidBody.velocity = m_net_linearVelocity;
                else
                    m_vehicle.RigidBody.velocity = Vector3.zero;

                if (VehicleManager.Instance.syncAngularVelocity)
                    m_vehicle.RigidBody.angularVelocity = m_net_angularVelocity;
                else
                    m_vehicle.RigidBody.angularVelocity = Vector3.zero;


                m_vehicle.Health = m_net_health;
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
            var customInput = CustomInput.Instance;

            var accel = customInput.GetAxis("Vertical");
            var brake = customInput.GetButton("Brake") ? 1.0f : 0.0f;
            var speed = Vector3.Dot(m_vehicle.Velocity, m_vehicle.transform.forward);

            if (speed * accel < 0f)
            {
                brake = Mathf.Max(brake, 0.75f);
                accel = 0f;
            }

            m_vehicle.Accelerator = accel;
            m_vehicle.Steering = customInput.GetAxis("Horizontal");
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

        void OnNetColorsChanged(string stringColors)
        {
            if (NetStatus.IsServer)
                return;

            if (m_vehicle != null)
                F.RunExceptionSafe(() => m_vehicle.SetColors(DeserializeColors(stringColors)));
        }

    }
}