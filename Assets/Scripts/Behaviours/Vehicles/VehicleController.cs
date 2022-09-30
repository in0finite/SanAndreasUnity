using UnityEngine;
using SanAndreasUnity.Net;
using Mirror;
using UGameCore.Utilities;
using System.Linq;
using System.Collections.Generic;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class VehicleController : NetworkBehaviour
    {
        private Vehicle m_vehicle;
        bool IsControlledByLocalPlayer => m_vehicle.IsControlledByLocalPlayer;

        [SyncVar] int m_net_id;

        [SyncVar(hook = nameof(OnNetColorsChanged))]
        string m_net_carColors;

        [SyncVar] float m_net_acceleration;
        [SyncVar] float m_net_steering;
        [SyncVar] bool m_net_isHandBrakeOn;

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


        readonly SyncList<WheelSyncData> m_net_wheelsData = new SyncList<WheelSyncData>();

        private readonly SyncDictionary<string, string> m_syncDictionary = new SyncDictionary<string, string>();
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
                F.RunExceptionSafe(() =>
                {
                    // load vehicle on clients

                    Color32[] colors = DeserializeColors(m_net_carColors);

                    m_vehicle = Vehicle.Create(this.gameObject, m_net_id, null, this.transform.position,
                        this.transform.rotation);

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

                    if (VehicleManager.Instance.destroyRigidBodyOnClients)
                    {
                        if (m_vehicle.RigidBody != null)
                            Object.Destroy(m_vehicle.RigidBody);
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
                throw new System.ArgumentException(
                    $"Failed to deserialize color - expected 4 components, found {splits.Length}");

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

            var oldInput = m_vehicle.Input;

            if (!GameManager.CanPlayerReadInput())
                this.ResetInput();
            else
                this.ReadInput();

            // why do we send input ?
            // - so that everyone knows if the gas/brake is pressed, and can simulate wheel effects
            // - so that server can predict position and velocity of rigid body
            PedSync.Local.SendVehicleInput(m_vehicle.Input);

            // TODO: also send velocity of rigid body

            if (!NetStatus.IsServer && !VehicleManager.Instance.controlInputOnLocalPlayer)
            {
                // local player should not control input, so restore old input
                m_vehicle.Input = oldInput;
            }
        }

        void ProcessSyncvars()
        {
            if (NetStatus.IsServer)
            {
                var vehicleInput = m_vehicle.Input;
                m_net_acceleration = vehicleInput.accelerator;
                m_net_steering = vehicleInput.steering;
                m_net_isHandBrakeOn = vehicleInput.isHandBrakeOn;

                m_net_health = m_vehicle.Health;

                // wheels
                m_net_wheelsData
                    .ClearChanges(); // remove current list of changes - this ensures that only the current wheel state is sent, and prevents memory leak bug in Mirror
                m_net_wheelsData.Clear();
                foreach (var wheel in m_vehicle.Wheels)
                {
                    m_net_wheelsData.Add(new WheelSyncData()
                    {
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
                if (!this.IsControlledByLocalPlayer || (this.IsControlledByLocalPlayer &&
                                                        !VehicleManager.Instance.controlInputOnLocalPlayer))
                {
                    m_vehicle.Input = new Vehicle.VehicleInput
                    {
                        accelerator = m_net_acceleration,
                        steering = m_net_steering,
                        isHandBrakeOn = m_net_isHandBrakeOn,
                    };
                }

                // update wheels
                if (!this.IsControlledByLocalPlayer || (this.IsControlledByLocalPlayer &&
                                                        !VehicleManager.Instance.controlWheelsOnLocalPlayer))
                {
                    for (int i = 0; i < m_vehicle.Wheels.Count && i < m_net_wheelsData.Count; i++)
                    {
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

                m_vehicle.Health = m_net_health;
            }
        }

        void ResetInput()
        {
            var input = m_vehicle.Input;
            input.Reset();
            m_vehicle.Input = input;
        }

        void ReadInput()
        {
            var customInput = CustomInput.Instance;

            var vehicleInput = new Vehicle.VehicleInput();
            vehicleInput.accelerator = customInput.GetAxis("Vertical");
            vehicleInput.isHandBrakeOn = customInput.GetButton("Brake");
            vehicleInput.steering = customInput.GetAxis("Horizontal");

            m_vehicle.Input = vehicleInput;
        }

        public void EnableOrDisableRigidBody()
        {
            if (NetStatus.IsServer)
            {
                F.EnableRigidBody(m_vehicle.RigidBody);
            }
        }

        void OnNetColorsChanged(string oldColors, string stringColors)
        {
            if (NetStatus.IsServer)
                return;

            if (m_vehicle != null)
                F.RunExceptionSafe(() => m_vehicle.SetColors(DeserializeColors(stringColors)));
        }
    }
}