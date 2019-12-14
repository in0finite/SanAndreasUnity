using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
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

        private static int _sLayer = -1;

        [HideInInspector]
        public Light m_frontLeftLight, m_frontRightLight, m_rearLeftLight, m_rearRightLight;

        private bool frontLeftLightOk = true, frontRightLightOk = true, rearLeftLightOk = true, rearRightLightOk = true,
                    m_frontLeftLightPowered = true, m_frontRightLightPowered = true, m_rearLeftLightPowered = true, m_rearRightLightPowered = true;

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
        public int[] Colors => _colors;
        private readonly float[] _lights = { 1f, 1f, 1f, 1f };
        private MaterialPropertyBlock _props;
        private bool _colorsChanged, _isNightToggled;

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

		public bool IsHornOn
		{
			get; set;
		}
		private AudioSource hornAudioSource;
		AudioClip horn;
		AudioClip _vehicleHornSound;
		private VehicleController _controller;

        bool m_isServer => Net.NetStatus.IsServer;
        public bool IsControlledByLocalPlayer => Ped.Instance != null && Ped.Instance.CurrentVehicle == this && Ped.Instance.CurrentVehicleSeat.IsDriver;

        public Mirror.NetworkTransform NetTransform { get; private set; }



        private void Awake()
        {
            this.NetTransform = this.GetComponent<Mirror.NetworkTransform>();
            _props = new MaterialPropertyBlock();
            m_radioAudioSource = GetComponent<AudioSource>();
		}

        void OnEnable()
        {
            s_vehicles.Add(this);
        }

        void OnDisable()
        {
			      Destroy(horn);
            s_vehicles.Remove(this);
        }

        void Start()
        {
            this.ApplySyncRate(VehicleManager.Instance.vehicleSyncRate);

            currentRadioStationIndex = Random.Range(0, RadioStation.stations.Length);

            Debug.LogFormat("Created vehicle - id {0}, name {1}, time: {2}", this.Definition.Id, 
                      this.Definition.GameName, F.CurrentDateForLogging);
            horn = Audio.AudioManager.CreateAudioClipFromSfx("GENRL", 67, this.Definition.HornId);
            _vehicleHornSound = MakeSubclip(horn, horn.length / 2f, horn.length);
            hornAudioSource = this.gameObject.AddComponent<AudioSource>();
            hornAudioSource.clip = _vehicleHornSound;
        }

        public void SetColors(params int[] clrIndices)
        {
            for (var i = 0; i < 4 && i < clrIndices.Length; ++i)
            {
                if (_colors[i].Equals(clrIndices[i])) continue;
                _colors[i] = clrIndices[i];
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

		public virtual void PlayHornSound()
		{
			if (!IsHornOn) return;
			hornAudioSource.playOnAwake = false;
			hornAudioSource.spatialBlend = 1;
			hornAudioSource.maxDistance = 15;
			if (hornAudioSource && hornAudioSource.clip)
			{
				if (!hornAudioSource.isPlaying)
				{
					hornAudioSource.loop = true;
					hornAudioSource.Play();
				}
			}
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

        public Seat DriverSeat => _seats.FirstOrDefault(s => s.IsDriver);

        public Transform GetSeatTransform(SeatAlignment alignment)
        {
            return GetSeat(alignment).Parent;
        }

        public void StopControlling()
        {
            //Destroy(_controller);
            //_controller = null;
        }

        private void UpdateColors()
        {
            _colorsChanged = false;

            var indices = CarColors.FromIndices(_colors);
            for (var i = 0; i < 4; ++i)
                _props.SetColor(CarColorIds[i], indices[i]);

            _props.SetVector(LightsId, new Vector4(_lights[0], _lights[1], _lights[2], _lights[3]));

            foreach (var frame in _frames)
            {
                var mr = frame.GetComponent<MeshRenderer>();
                if (mr == null) continue;
                mr.SetPropertyBlock(_props);
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

            if (_colorsChanged)
            {
                UpdateColors();
            }

            var localPed = Ped.Instance;
            if (currentRadioStationIndex != -1 && null != localPed && localPed.CurrentVehicle == this && localPed.IsInVehicleSeat)
            {
                if (!m_radioAudioSource.isPlaying)
                {
                    ContinueRadio();
                }
            }
            else
            {
                if (m_radioAudioSource.isPlaying)
                {
                    CurrentRadioStation.currentTime = m_radioAudioSource.time;
                    m_radioAudioSource.Stop();
                }
            }

			if (IsHornOn) { PlayHornSound(); }
			else { hornAudioSource.loop = false; }
			IsHornOn = false;
		}

        private void FixedUpdate()
        {
            //    NetworkingFixedUpdate();
            PhysicsFixedUpdate();
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

		//TODO add vehicle type horn sound, rename the method appropriately
		private AudioClip MakeSubclip(AudioClip clip, float start, float stop)
		{
			int frequency = clip.frequency;
			float timeLength = stop - start;
			int samplesLength = (int)(frequency * timeLength);
			AudioClip newClip = AudioClip.Create(clip.name + "-sub", samplesLength, 1, frequency, false);
			float[] data = new float[samplesLength];
			clip.GetData(data, (int)(frequency * start));
			newClip.SetData(data, 0);
			return newClip;
		}

	}
}