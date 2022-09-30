using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Vehicles;
using UGameCore.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.VehicleDef;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public enum VehicleLight
    {
        FrontLeft = 1,
        FrontRight = 2,

        RearLeft = 4,
        RearRight = 8,

        Front = FrontLeft | FrontRight,
        Rear = RearLeft | RearRight,

        All = Front | Rear
    }

    public enum VehicleBlinkerMode
    {
        None, Left, Right, Emergency
    }

#if CLIENT
    public partial class Vehicle : Networking.Networkable
#else

    public partial class Vehicle : MonoBehaviour
#endif
    {
        static List<Vehicle> s_vehicles = new List<Vehicle>();
        public static IEnumerable<Vehicle> AllVehicles => s_vehicles;
        public static int NumVehicles => s_vehicles.Count;
        public static IEnumerable<Rigidbody> AllVehicleRigidBodies => AllVehicles.Select(v => v.RigidBody).Where(r => r != null);

        private static int _sLayer = -1;

        [HideInInspector]
        public Light m_frontLeftLight, m_frontRightLight, m_rearLeftLight, m_rearRightLight;

        private bool frontLeftLightOk = true, frontRightLightOk = true, rearLeftLightOk = true, rearRightLightOk = true;

        private const float blinkerSum = 1.5f;

        private Material directionalLightsMat;

        internal VehicleBlinkerMode blinkerMode;

        public bool m_frontLeftLightOk
        {
            get
            {
                return frontLeftLightOk;
            }
            set
            {
                frontLeftLightOk = value;
                SetLight(VehicleLight.FrontLeft, value ? 1 : 0);
            }
        }

        public bool m_frontRightLightOk
        {
            get
            {
                return frontRightLightOk;
            }
            set
            {
                frontRightLightOk = value;
                SetLight(VehicleLight.FrontRight, value ? 1 : 0);
            }
        }

        public bool m_rearLeftLightOk
        {
            get
            {
                return rearLeftLightOk;
            }
            set
            {
                rearLeftLightOk = value;
                SetLight(VehicleLight.RearLeft, value ? 1 : 0);
            }
        }

        public bool m_rearRightLightOk
        {
            get
            {
                return rearRightLightOk;
            }
            set
            {
                rearRightLightOk = value;
                SetLight(VehicleLight.RearRight, value ? 1 : 0);
            }
        }

        public static int Layer
        {
            get { return _sLayer == -1 ? _sLayer = UnityEngine.LayerMask.NameToLayer("Vehicle") : _sLayer; }
        }

        public static int LayerMask { get { return 1 << Layer; } }

        public static int MeshLayer => UnityEngine.LayerMask.NameToLayer("VehicleMesh");
        public static int MeshLayerMask => 1 << MeshLayer;

        public static readonly Color32 DefaultVehicleColor = default;

        private readonly Color32[] _colors = { DefaultVehicleColor, DefaultVehicleColor, DefaultVehicleColor, DefaultVehicleColor };
        public IReadOnlyList<Color32> Colors => _colors;
        private readonly float[] _lights = { 0, 0, 0, 0 };
        private MaterialPropertyBlock _props;
        private static readonly int CarColorPropertyId = Shader.PropertyToID("_CarColor");
        private static readonly int CarEmissionPropertyId = Shader.PropertyToID("_CarEmission");
        private bool _colorsChanged, _isNightToggled;

        public event System.Action onColorsChanged = delegate {};

        private const float constRearNightIntensity = .7f;

        public bool IsNightToggled
        {
            get
            {
                return _isNightToggled;
            }
            set
            {
                _isNightToggled = value;

                SetLight(VehicleLight.FrontLeft, _isNightToggled ? VehicleAPI.frontLightIntensity : 0);
                SetLight(VehicleLight.FrontRight, _isNightToggled ? VehicleAPI.frontLightIntensity : 0);
                SetLight(VehicleLight.RearLeft, _isNightToggled ? constRearNightIntensity : 0);
                SetLight(VehicleLight.RearRight, _isNightToggled ? constRearNightIntensity : 0);
            }
        }

        private VehicleController _controller;

        bool m_isServer => Net.NetStatus.IsServer;
        public bool IsControlledByLocalPlayer => Ped.Instance != null && Ped.Instance.CurrentVehicle == this && Ped.Instance.CurrentVehicleSeat.IsDriver;

        public Mirror.NetworkIdentity NetIdentity { get; private set; }

        public Net.CustomNetworkTransform NetTransform { get; private set; }

        List<Ped> m_lastPreparedPeds = new List<Ped>();

        public string DescriptionForLogging
        {
            get
            {
                if (this.Definition != null)
                    return $"(modelId={this.Definition.Id}, name={this.Definition.GameName})";
                else
                    return $"(gameObjectName={this.gameObject.name}, instanceId={this.GetInstanceID()})";
            }
        }



        private void Awake()
        {
            this.NetIdentity = this.GetComponentOrThrow<Mirror.NetworkIdentity>();
            this.NetTransform = this.GetComponent<Net.CustomNetworkTransform>();
            _props = new MaterialPropertyBlock();
            this.Awake_Damage();
            this.Awake_Radio();
        }

        void OnEnable()
        {
            s_vehicles.Add(this);
        }

        void OnDisable()
        {
            s_vehicles.Remove(this);

            VehiclePhysicsConstants.Changed -= this.UpdateValues;

            this.OnDisable_Radio();

            if (this.HighDetailMeshesParent != null)
            {
                Destroy(this.HighDetailMeshesParent.gameObject);
            }

        }

        void Start()
        {
            this.ApplySyncRate(VehicleManager.Instance.vehicleSyncRate);

            this.Start_Radio();

            Debug.Log($"Created vehicle {this.DescriptionForLogging}, time: {F.CurrentDateForLogging}");
        }

        public void SetColors(params int[] clrIndices)
        {
            this.SetColors(CarColors.FromIndices(clrIndices));
        }

        public void SetColors(Color32[] colors)
        {
            /*if (colors.Length > 4)
                throw new System.ArgumentException("Vehicle can not have more than 4 colors");*/

            for (int i = 0; i < 4 && i < colors.Length; ++i)
            {
                if (_colors[i].Equals(colors[i]))
                    continue;
                _colors[i] = colors[i];
                _colorsChanged = true;
            }
        }

        private Light GetLight(VehicleLight light)
        {
            //if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");
            switch (light)
            {
                case VehicleLight.FrontLeft:
                    return m_frontLeftLight;

                case VehicleLight.FrontRight:
                    return m_frontRightLight;

                case VehicleLight.RearLeft:
                    return m_rearLeftLight;

                case VehicleLight.RearRight:
                    return m_rearRightLight;
            }

            return null;
        }

        private bool IsLightOk(VehicleLight light)
        {
            switch (light)
            {
                case VehicleLight.FrontLeft:
                    return m_frontLeftLightOk;

                case VehicleLight.FrontRight:
                    return m_frontRightLightOk;

                case VehicleLight.RearLeft:
                    return m_rearLeftLightOk;

                case VehicleLight.RearRight:
                    return m_rearRightLightOk;
            }

            return true;
        }

        private bool IsAnyLightPowered()
        {
            if (_lights != null)
                return _lights.Any(x => x > 0);
            return false;
        }

        public void SetLight(VehicleLight light, float brightness)
        {
            brightness = Mathf.Clamp01(brightness);

            for (var i = 0; i < 4; ++i)
            {
                var bit = 1 << i;
                if (((int)light & bit) == bit)
                {
                    VehicleLight parsedLight = (VehicleLight)bit; //VehicleAPI.ParseFromBit(i);

                    if (IsLightOk(parsedLight))
                    {
                        Light lightObj = GetLight(parsedLight);
                        bool mustRearPower = _isNightToggled && !VehicleAPI.IsFrontLight(light);

                        if (brightness > 0 || mustRearPower)
                        {
                            if (lightObj != null && !lightObj.enabled)
                            {
                                lightObj.enabled = true;
                                lightObj.intensity = mustRearPower ? constRearNightIntensity : brightness;
                            }
                        }
                        else
                        {
                            if (lightObj != null) lightObj.enabled = false;
                        }

                        SetLight(i, mustRearPower ? constRearNightIntensity : brightness);
                    }
                }
            }
        }

        private void SetLight(int index, float brightness)
        {
            if (_lights[index] == brightness) return;
            _lights[index] = brightness;
            _colorsChanged = true;
        }

        public VehicleDef Definition { get; private set; }

        public Transform DriverTransform { get; private set; }

        public bool HasDriverSeat { get { return DriverTransform != null && DriverTransform.childCount > 0; } }

        public VehicleController StartControlling()
        {
            //SetAllCarLights();
            return _controller ?? (_controller = gameObject.GetOrAddComponent<VehicleController>());
        }

        public void SetAllCarLights()
        {
            // Implemented: Add lights

            Transform headlights = this.GetComponentWithName<Transform>("headlights"),
                      taillights = this.GetComponentWithName<Transform>("taillights");

            Vehicle vh = gameObject.GetComponent<Vehicle>();

            if (headlights != null)
            {
                m_frontLeftLight = VehicleAPI.SetCarLight(vh, headlights, VehicleLight.FrontLeft);
                m_frontRightLight = VehicleAPI.SetCarLight(vh, headlights, VehicleLight.FrontRight);
            }

            if (taillights != null)
            {
                m_rearLeftLight = VehicleAPI.SetCarLight(vh, taillights, VehicleLight.RearLeft);
                m_rearRightLight = VehicleAPI.SetCarLight(vh, taillights, VehicleLight.RearRight);
            }

            m_frontLeftLightOk = m_frontLeftLight != null;
            m_frontRightLightOk = m_frontRightLight != null;
            m_rearLeftLightOk = m_rearLeftLight != null;
            m_rearRightLightOk = m_rearRightLight != null;
        }

        public SeatAlignment GetSeatAlignmentOfClosestSeat(Vector3 position)
        {
            var seat = FindClosestSeat(position);
            return seat != null ? seat.Alignment : SeatAlignment.None;
        }

        public Seat FindClosestSeat(Vector3 position)
        {
            if (this.Seats.Count < 1)
                return null;

            return this.Seats.Aggregate((a, b) => 
                Vector3.Distance(position, a.Parent.position) < Vector3.Distance(position, b.Parent.position) ? a : b);
        }

        public Transform FindClosestSeatTransform(Vector3 position)
        {
            var seat = FindClosestSeat(position);
            return seat != null ? seat.Parent : null;
        }

        public Seat GetSeat(SeatAlignment alignment)
        {
            return _seats.FirstOrDefault(x => x.Alignment == alignment);
        }

        public Seat DriverSeat => _seats.FirstOrDefault(s => s.IsDriver);

        public Transform GetSeatTransform(SeatAlignment alignment)
        {
            var seat = GetSeat(alignment);
            return seat != null ? seat.Parent : null;
        }

        public bool IsLocalPedInside()
        {
            var ped = Ped.Instance;
            if (ped != null)
            {
                return this.Seats.Exists(s => s.OccupyingPed == ped);
            }
            return false;
        }

        public void StopControlling()
        {
            //Destroy(_controller);
            //_controller = null;
        }

        internal void OnPedPreparedForVehicle(Ped ped, Seat seat)
        {
            int numPedsToAdd = this.Seats.Count - m_lastPreparedPeds.Count;
            for (int i = 0; i < numPedsToAdd; i++)
            {
                m_lastPreparedPeds.Add(null);
            }

            int index = this.Seats.FindIndex(s => s == seat);

            if (m_lastPreparedPeds[index] == ped)
                return;

            m_lastPreparedPeds[index] = ped;
            seat.TimeWhenPedChanged = Time.timeAsDouble;

            this.OnPedAssignedToVehicle_Radio(ped, seat);

        }

        internal void OnPedRemovedFromVehicle(Ped ped, Seat seat)
        {
            int numPedsToAdd = this.Seats.Count - m_lastPreparedPeds.Count;
            for (int i = 0; i < numPedsToAdd; i++)
            {
                m_lastPreparedPeds.Add(null);
            }

            int index = this.Seats.FindIndex(s => s == seat);
            
            m_lastPreparedPeds[index] = null;
            seat.TimeWhenPedChanged = Time.timeAsDouble;

        }

        private void UpdateMaterials()
        {
            if (!_colorsChanged)
                return;

            _colorsChanged = false;

            UpdateMaterials(_frames, _colors, _lights, _props);

            F.InvokeEventExceptionSafe(this.onColorsChanged);
        }

        public static void UpdateMaterials(
            FrameContainer frames,
            Color32[] paintJobColors,
            float[] lights,
            MaterialPropertyBlock materialPropertyBlock)
        {
            Color32 headLightColor = new Color32(255, 255, 255, 255);
            Color32 tailLightColor = new Color32(255, 255, 255, 255);

            // compute car colors
            Color32[] carColors = new []
            {
                new Color32(255, 255, 255, 255),
                paintJobColors[0],
                paintJobColors[1],
                paintJobColors[2],
                paintJobColors[3],
                headLightColor,
                headLightColor,
                tailLightColor,
                tailLightColor,
            };

            // compute car emissions
            float[] carEmissions = new[]
            {
                0f,
                0f,
                0f,
                0f,
                0f,
                Mathf.Exp(lights[0] * 2) - 1,
                Mathf.Exp(lights[1] * 2) - 1,
                Mathf.Exp(lights[2] * 2) - 1,
                Mathf.Exp(lights[3] * 2) - 1,
            };

            foreach (var frame in frames)
            {
                var mr = frame.GetComponent<MeshRenderer>();
                if (mr == null) continue;

                // get color index from each material, and assign properties accordingly

                var materials = mr.sharedMaterials;

                for (int i = 0; i < materials.Length; i++)
                {
                    int carColorIndex = materials[i].GetInt(Importing.Conversion.Geometry.CarColorIndexId);
                    materialPropertyBlock.SetColor(CarColorPropertyId, carColors[carColorIndex]);
                    materialPropertyBlock.SetFloat(CarEmissionPropertyId, carEmissions[carColorIndex]);
                    mr.SetPropertyBlock(materialPropertyBlock, i);
                }

            }
        }

        private void Update()
        {

            if (Net.NetStatus.IsServer
                || (this.IsControlledByLocalPlayer && VehicleManager.Instance.controlWheelsOnLocalPlayer && ! VehicleManager.Instance.destroyWheelCollidersOnClient))
            {
                foreach (var wheel in _wheels)
                {
                    Vector3 position = Vector3.zero;

                    WheelHit wheelHit;

                    if (wheel.Collider.GetGroundHit(out wheelHit))
                    {
                        position.y = (wheelHit.point.y - wheel.Collider.transform.position.y) + wheel.Collider.radius;
                    }
                    else
                    {
                        position.y -= wheel.Collider.suspensionDistance;
                    }

                    wheel.Child.transform.localPosition = position;

                    UpdateWheelRotation(wheel, wheel.Collider.rpm, wheel.Collider.steerAngle);
                }
            }

            this.UpdateMaterials();

            this.Update_Damage();

            this.Update_Radio();

            this.UpdateHighDetailMeshes();

            if (Net.NetStatus.IsServer && this.transform.position.y < -2000f)
            {
                Object.Destroy(this.gameObject);
            }

        }

        private void FixedUpdate()
        {
            //    NetworkingFixedUpdate();
            PhysicsFixedUpdate();
        }

        public static void UpdateWheelRotation(Wheel wheel, float rpm, float steerAngle)
        {
            // reset the yaw
            wheel.Child.localRotation = wheel.Roll;

            // calculate new roll
            wheel.Child.Rotate(wheel.IsLeftHand ? Vector3.left : Vector3.right, rpm / 60.0f * 360.0f * Time.deltaTime);
            wheel.Roll = wheel.Child.localRotation;

            // apply yaw
            wheel.Child.localRotation = Quaternion.AngleAxis(steerAngle, Vector3.up) * wheel.Roll;
        }

        void UpdateHighDetailMeshes()
        {
            this.HighDetailMeshesParent.SetPositionAndRotation(this.transform.position, this.transform.rotation);

            for (int i = 0; i < m_highDetailMeshObjectsToUpdate.Count; i++)
            {
                var item = m_highDetailMeshObjectsToUpdate[i];
                item.Value.SetPositionAndRotation(item.Key.position, item.Key.rotation);
            }
        }

        private IEnumerator DelayedBlinkersTurnOff()
        {
            yield return new WaitForSeconds(blinkerSum);

            if (blinkerMode != VehicleBlinkerMode.None)
                blinkerMode = VehicleBlinkerMode.None;
        }

        public void ApplySyncRate(float syncRate)
        {
            foreach (var comp in this.GetComponents<Mirror.NetworkBehaviour>())
			    comp.syncInterval = 1.0f / syncRate;
            
            // also assign it to NetworkTransform, because it may be disabled
            if (this.NetTransform != null)
                this.NetTransform.syncInterval = 1.0f / syncRate;
        }

    }
}