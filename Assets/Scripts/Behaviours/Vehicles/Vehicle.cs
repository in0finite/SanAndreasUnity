using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LightData = SpriteLights.LightData;
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

    public enum VehicleBlinker
    {
        FrontLeft = 1,
        FrontRight = 2,

        RearLeft = 4,
        RearRight = 8,

        Front = FrontLeft | FrontRight,
        Rear = RearLeft | RearRight,

        All = Front | Rear
    }

#if CLIENT
    public partial class Vehicle : Networking.Networkable
#else

    public partial class Vehicle : MonoBehaviour
#endif
    {
        private static int _sLayer = -1;

        [HideInInspector]
        public Light m_frontLeftLight, m_frontRightLight, m_rearLeftLight, m_rearRightLight;

        private bool frontLeftLightOk = true, frontRightLightOk = true, rearLeftLightOk = true, rearRightLightOk = true,
                    m_frontLeftLightPowered = true, m_frontRightLightPowered = true, m_rearLeftLightPowered = true, m_rearRightLightPowered = true;

        private Material directionalLightsMat;

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

        private static int _sLightsId = -1;

        protected static int LightsId
        {
            get
            {
                return _sLightsId == -1 ? _sLightsId = Shader.PropertyToID("_Lights") : _sLightsId;
            }
        }

        private static int[] _sCarColorIds;

        protected static int[] CarColorIds
        {
            get
            {
                return _sCarColorIds ?? (_sCarColorIds = Enumerable.Range(1, 4)
                    .Select(x => Shader.PropertyToID(string.Format("_CarColor{0}", x)))
                    .ToArray());
            }
        }

        private readonly int[] _colors = { 0, 0, 0, 0 };
        private readonly float[] _lights = { 1f, 1f, 1f, 1f };
        private MaterialPropertyBlock _props;
        private bool _colorsChanged;

        private VehicleController _controller;

#if CLIENT
        protected override void OnAwake()
        {
            _props = new MaterialPropertyBlock();

            base.OnAwake();
        }
#endif

        public void SetColors(params int[] clrIndices)
        {
            for (var i = 0; i < 4 && i < clrIndices.Length; ++i)
            {
                if (_colors[i].Equals(clrIndices[i])) continue;
                _colors[i] = clrIndices[i];
                _colorsChanged = true;
            }
        }

        public void SetLight(VehicleLight light, float brightness)
        {
            brightness = Mathf.Clamp01(brightness);

            for (var i = 0; i < 4; ++i)
            {
                var bit = 1 << i;
                if (((int)light & bit) == bit)
                {
                    SetLight(i, brightness);
                }
            }

            switch (light)
            {
                case VehicleLight.All:
                    if (brightness > 0)
                    {
                        if (m_frontLeftLight != null && !m_frontLeftLight.enabled)
                            m_frontLeftLight.enabled = true;
                        if (m_frontRightLight != null && !m_frontRightLight.enabled)
                            m_frontRightLight.enabled = true;
                        if (m_rearLeftLight != null && !m_rearLeftLight.enabled)
                            m_rearLeftLight.enabled = true;
                        if (m_rearRightLight != null && !m_rearRightLight.enabled)
                            m_rearRightLight.enabled = true;

                        if (m_frontLeftLight != null) m_frontLeftLight.intensity = brightness;
                        if (m_frontRightLight != null) m_frontRightLight.intensity = brightness;
                        if (m_rearLeftLight != null) m_rearLeftLight.intensity = brightness;
                        if (m_rearRightLight != null) m_rearRightLight.intensity = brightness;
                    }
                    else
                    {
                        if (m_frontLeftLight != null) m_frontLeftLight.enabled = false;
                        if (m_frontRightLight != null) m_frontRightLight.enabled = false;
                        if (m_rearLeftLight != null) m_rearLeftLight.enabled = false;
                        if (m_rearRightLight != null) m_rearRightLight.enabled = false;
                    }
                    break;

                case VehicleLight.Front:
                    if (brightness > 0)
                    {
                        if (m_frontLeftLight != null && !m_frontLeftLight.enabled)
                            m_frontLeftLight.enabled = true;
                        if (m_frontRightLight != null && !m_frontRightLight.enabled)
                            m_frontRightLight.enabled = true;

                        if (m_frontLeftLight != null) m_frontLeftLight.intensity = brightness;
                        if (m_frontRightLight != null) m_frontRightLight.intensity = brightness;
                    }
                    else
                    {
                        if (m_frontLeftLight != null) m_frontLeftLight.enabled = false;
                        if (m_frontRightLight != null) m_frontRightLight.enabled = false;
                    }
                    break;

                case VehicleLight.FrontLeft:
                    if (brightness > 0)
                    {
                        if (m_frontLeftLight != null && !m_frontLeftLight.enabled)
                            m_frontLeftLight.enabled = true;

                        if (m_frontLeftLight != null) m_frontLeftLight.intensity = brightness;
                    }
                    else
                       if (m_frontLeftLight != null) m_frontLeftLight.enabled = false;
                    break;

                case VehicleLight.FrontRight:
                    if (brightness > 0)
                    {
                        if (m_frontRightLight != null && !m_frontRightLight.enabled)
                            m_frontRightLight.enabled = true;

                        if (m_frontRightLight != null) m_frontRightLight.intensity = brightness;
                    }
                    else
                       if (m_frontRightLight != null) m_frontRightLight.enabled = false;
                    break;

                case VehicleLight.Rear:
                    if (brightness > 0)
                    {
                        if (m_rearLeftLight != null && !m_rearLeftLight.enabled)
                            m_rearLeftLight.enabled = true;
                        if (m_rearRightLight != null && !m_rearRightLight.enabled)
                            m_rearRightLight.enabled = true;

                        if (m_rearLeftLight != null) m_rearLeftLight.intensity = brightness;
                        if (m_rearRightLight != null) m_rearRightLight.intensity = brightness;
                    }
                    else
                    {
                        if (m_rearLeftLight != null) m_rearLeftLight.enabled = false;
                        if (m_rearRightLight != null) m_rearRightLight.enabled = false;
                    }
                    break;

                case VehicleLight.RearLeft:
                    if (brightness > 0)
                    {
                        if (m_rearLeftLight != null && !m_rearLeftLight.enabled)
                            m_rearLeftLight.enabled = true;

                        if (m_rearLeftLight != null) m_rearLeftLight.intensity = brightness;
                    }
                    else
                       if (m_rearLeftLight != null) m_rearLeftLight.enabled = false;
                    break;

                case VehicleLight.RearRight:
                    if (brightness > 0)
                    {
                        if (m_rearRightLight != null && !m_rearRightLight.enabled)
                            m_rearRightLight.enabled = true;

                        if (m_rearRightLight != null) m_rearRightLight.intensity = brightness;
                    }
                    else
                       if (m_rearRightLight != null) m_rearRightLight.enabled = false;
                    break;
            }
        }

        private IEnumerable<GameObject> GetLightObjects()
        {
            return gameObject.GetComponentsInChildren<Light>().Select(x => x.gameObject);
        }

        private void SetLight(int index, float brightness)
        {
            if (_lights[index] == brightness) return;
            _lights[index] = brightness;
            _colorsChanged = true;
        }

        public VehicleDef Definition { get; private set; }

        public Transform DriverTransform { get; private set; }

        public bool HasDriver { get { return DriverTransform != null && DriverTransform.childCount > 0; } }

        public bool IsControlling { get { return _controller != null; } }

        public VehicleController StartControlling()
        {
            return _controller ?? (_controller = gameObject.AddComponent<VehicleController>());
        }

        public SeatAlignment FindClosestSeat(Vector3 position)
        {
            var seat = _seats.Select((s, i) => new { s, i })
                .OrderBy(x => Vector3.Distance(position, x.s.Parent.position))
                .FirstOrDefault();

            return (seat == null ? SeatAlignment.None : seat.s.Alignment);
        }

        public Transform FindClosestSeatTransform(Vector3 position)
        {
            return GetSeatTransform(FindClosestSeat(position));
        }

        public Seat GetSeat(SeatAlignment alignment)
        {
            return _seats.FirstOrDefault(x => x.Alignment == alignment);
        }

        public Transform GetSeatTransform(SeatAlignment alignment)
        {
            return GetSeat(alignment).Parent;
        }

        public void StopControlling()
        {
            Destroy(_controller);
            _controller = null;
        }

        private void UpdateColors()
        {
            _colorsChanged = false;

            var indices = CarColors.FromIndices(_colors);
            for (var i = 0; i < 4; ++i)
            {
                _props.SetColor(CarColorIds[i], indices[i]);
            }

            _props.SetVector(LightsId, new Vector4(_lights[0], _lights[1], _lights[2], _lights[3]));

            foreach (var frame in _frames)
            {
                var mr = frame.GetComponent<MeshRenderer>();
                if (mr == null) continue;
                mr.SetPropertyBlock(_props);
            }
        }

        private void Awake()
        {
            _props = new MaterialPropertyBlock();
        }

        private void Start()
        {
            // Add vehicle damage

            var dam = gameObject.AddComponent<VehicleDamage>();
            dam.damageParts = new Transform[] { transform.GetChild(0).Find("engine") };
            dam.deformMeshes = gameObject.GetComponentsInChildren<MeshFilter>();
            dam.displaceParts = gameObject.GetComponentsInChildren<Transform>().Where(x => x.GetComponent<Frame>() != null || x.GetComponent<FrameContainer>() != null).ToArray();
            dam.damageFactor = 2f;
            dam.collisionIgnoreHeight = -.4f;
            dam.collisionTimeGap = .1f;

            //OptimizeVehicle();

            dam.deformColliders = gameObject.GetComponentsInChildren<MeshCollider>();

            // Implemented: Add lights

            Transform headlights = this.GetComponentWithName<Transform>("headlights"),
                      taillights = this.GetComponentWithName<Transform>("taillights");

            if (headlights != null)
            {
                m_frontLeftLight = SetCarLight(headlights, VehicleLight.FrontLeft);
                m_frontRightLight = SetCarLight(headlights, VehicleLight.FrontRight);
            }

            if (taillights != null)
            {
                m_rearLeftLight = SetCarLight(taillights, VehicleLight.RearLeft);
                m_rearRightLight = SetCarLight(taillights, VehicleLight.RearRight);
            }

            // Apply Light sources

            directionalLightsMat = Resources.Load<Material>("Materials/directionalLight");
            SetLightSources();

            m_frontLeftLightOk = m_frontLeftLight != null;
            m_frontRightLightOk = m_frontRightLight != null;
            m_rearLeftLightOk = m_rearLeftLight != null;
            m_rearRightLightOk = m_rearRightLight != null;
        }

        private void SetLightSources()
        {
            List<LightData> datas = new List<LightData>();
            var objs = GetLightObjects();

            //Map object with an index
            //Debug.LogFormat("Objs: {0}", objs.Count());

            foreach (var go in objs)
            {
                LightData lightData = new LightData();

                lightData.position = go.transform.position;
                lightData.brightness = 1;
                lightData.size = 1;

                datas.Add(lightData);
            }

            var obj = SpriteLights.CreateLights(gameObject.name.ToLower() + "-LD", datas.ToArray(), directionalLightsMat);

            //Debug.LogFormat("Obj Count: {0}", obj.Count());

            gameObject.transform.MakeChild(obj, (p, o) =>
            {
                // Check the index and them set where is has to be generated
                o.transform.position = m_frontLeftLight.transform.position;
            });
        }

        private Light SetCarLight(Transform parent, VehicleLight light, Vector3? pos = null)
        {
            GameObject gameObject = null;
            return SetCarLight(parent, light, pos == null ? (IsLeftLight(light) ? Vector3.zero : new Vector3(-parent.localPosition.x * 2, 0, 0)) : pos.Value, out gameObject);
        }

        private Light SetCarLight(Transform parent, VehicleLight light, Vector3 pos, out GameObject go)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");

            Transform lightObj = new GameObject(GetLightName(light)).transform;
            lightObj.parent = parent;

            Quaternion rot = IsFrontLight(light) ? Quaternion.identity : Quaternion.Euler(Vector3.right * 180);

            lightObj.localPosition = pos;
            lightObj.localRotation = rot;

            // Rear light props
            Light ret = lightObj.gameObject.AddComponent<Light>();
            SetLightProps(GetVehicleLightParent(light), ref ret);

            // Now set its blinker

            go = lightObj.gameObject;
            return ret;
        }

        private bool IsFrontLight(VehicleLight light)
        {
            return light == VehicleLight.Front || light == VehicleLight.FrontLeft || light == VehicleLight.FrontRight;
        }

        private bool IsLeftLight(VehicleLight light)
        {
            return light == VehicleLight.FrontLeft || light == VehicleLight.RearLeft;
        }

        private string GetLightName(VehicleLight light)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");
            string lightName = light.ToString();

            return string.Format("{0}Light", IsFrontLight(light) ? lightName.Substring(5) : lightName.Substring(4));
        }

        private VehicleLight GetVehicleLightParent(VehicleLight light)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");
            string lightName = light.ToString();

            return (VehicleLight)System.Enum.Parse(typeof(VehicleLight), IsFrontLight(light) ? lightName.Substring(0, 5) : lightName.Substring(0, 4));
        }

        private void SetLightProps(VehicleLight vehicleLight, ref Light light)
        {
            if (light == null) return;
            switch (vehicleLight)
            {
                case VehicleLight.Front:
                case VehicleLight.FrontLeft:
                case VehicleLight.FrontRight:
                    light.type = LightType.Spot;
                    light.range = 60;
                    light.spotAngle = 90;
                    light.intensity = 2;
                    break;

                case VehicleLight.Rear:
                case VehicleLight.RearLeft:
                case VehicleLight.RearRight:
                    light.type = LightType.Spot;
                    light.range = 20;
                    light.spotAngle = 50;
                    light.intensity = 1;
                    light.color = Color.red;
                    break;
            }
        }

        private void Update()
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

                // reset the yaw
                wheel.Child.localRotation = wheel.Roll;

                // calculate new roll
                wheel.Child.Rotate(wheel.IsLeftHand ? Vector3.left : Vector3.right, wheel.Collider.rpm / 60.0f * 360.0f * Time.deltaTime);
                wheel.Roll = wheel.Child.localRotation;

                // apply yaw
                wheel.Child.localRotation = Quaternion.AngleAxis(wheel.Collider.steerAngle, Vector3.up) * wheel.Roll;
            }

            if (HasDriver)
            {
                if (m_frontLeftLightPowered && m_frontLeftLightOk)
                    SetLight(VehicleLight.FrontLeft, 1f);

                if (m_frontRightLightPowered && m_frontRightLightOk)
                    SetLight(VehicleLight.FrontRight, 1f);

                if (Input.GetKeyDown(KeyCode.L))
                {
                    m_frontLeftLightPowered = !m_frontLeftLight;
                    m_frontRightLightPowered = !m_frontRightLightPowered;
                }

                if (Braking > 0.125f)
                {
                    if (m_frontLeftLightOk)
                        SetLight(VehicleLight.RearLeft, 1f);

                    if (m_frontRightLightOk)
                        SetLight(VehicleLight.RearRight, 1f);
                }
                else
                {
                    SetLight(VehicleLight.Rear, 0f);
                }
            }
            else
            {
                SetLight(VehicleLight.All, 0f);
                Braking = 1f;
            }

            if (_colorsChanged)
            {
                UpdateColors();
            }
        }

        private void FixedUpdate()
        {
            //    NetworkingFixedUpdate();
            PhysicsFixedUpdate();
        }
    }
}