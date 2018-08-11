using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.VehicleDef;

namespace SanAndreasUnity.Behaviours.Vehicles
{
#if CLIENT
    public partial class Vehicle : Networking.Networkable
#else

    public partial class Vehicle : MonoBehaviour
#endif
    {
        public float angularVelocity;

        private static int _sLayer = -1;

        public Dictionary<VehicleLight, VehicleLights> m_lightDict = new Dictionary<VehicleLight, VehicleLights>();

        private const float blinkerSum = 1.5f;

        private Material directionalLightsMat;

        internal VehicleBlinkerMode blinkerMode;

        private bool hasInit;

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

        private List<VehicleBehaviour> behaviours;

        // Doors
        private IEnumerable<VehicleDoor> vehicleDoors;

        private bool hasRearBrightness
        {
            get
            {
                if(hasInit)
                    return m_lightDict[VehicleLight.RearLeft].lightComponent.intensity > 0 || m_lightDict[VehicleLight.RearRight].lightComponent.intensity > 0;
                return false;
            }
        }
#if CLIENT
        protected override void OnAwake()
        {
            _props = new MaterialPropertyBlock();

            base.OnAwake();
        }
#else

        // Must review
        private void Awake()
        {
            _props = new MaterialPropertyBlock();
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

        public Light GetLight(VehicleLight light)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");

            return m_lightDict[light].lightComponent;
        }

        public bool IsLightOk(VehicleLight light)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");

            return m_lightDict[light].canPower;
        }

        private bool IsAnyLightPowered()
        {
            if (_lights != null)
                return _lights.Any(x => x > 0);
            return false;
        }

        public void SetLight(int index, float brightness)
        {
            if (_lights[index] == brightness) return; // Avoid flooding at light events

            _lights[index] = brightness;
            _colorsChanged = true;
        }

        public void SetMultipleLights(VehicleLight light, float brightness)
        {
            for (var i = 0; i < 4; ++i)
            {
                var bit = 1 << i;
                if (((int)light & bit) == bit)
                {
                    if (hasInit)
                    {
                        VehicleLight parsedLight = (VehicleLight)bit;
                        VehicleLights lights = m_lightDict[parsedLight];
                        float bright = lights.isOk && lights.isRear && WorldController.IsNight ? VehicleLights.rearLightIntensity : brightness;

                        if (lights.canPower) lights.SetLight(brightness);
                    }
                    else
                        SetLight(i, brightness);
                }
            }
        }

        public VehicleDef Definition { get; private set; }

        public Transform DriverTransform { get; private set; }

        public bool HasDriver { get { return DriverTransform != null && DriverTransform.childCount > 0; } }

        public bool IsControlling { get { return _controller != null; } }

        public VehicleController StartControlling()
        { // Must review: Maybe can cause some leaks
            behaviours = new List<VehicleBehaviour>();

            behaviours.AddRange(SetAllCarLights());

            vehicleDoors = SetAllDoors().Cast<VehicleDoor>();
            behaviours.AddRange(vehicleDoors);
            SetAllCollider(behaviours.ToArray());

            hasInit = true;

            return _controller ?? (_controller = gameObject.AddComponent<VehicleController>());
        }

        public void SetAllCollider(VehicleBehaviour[] vehicleBehaviour)
        {
            //Set collider detector on the same place that Rigidbody
            VehicleCollider.Init(gameObject, this, vehicleBehaviour);
        }

        public IEnumerable<VehicleBehaviour> SetAllDoors()
        {
            Transform FrontLeftDoor = this.GetComponentWithName<Transform>("door_lf_dummy"),
                      FrontRightDoor = this.GetComponentWithName<Transform>("door_rf_dummy"),
                      RearLeftDoor = this.GetComponentWithName<Transform>("door_lr_dummy"),
                      RearRightDoor = this.GetComponentWithName<Transform>("door_rr_dummy");

            bool IsTwoDoorsVehicle = RearLeftDoor == null && RearRightDoor == null;

            List<Transform> doors = new List<Transform>();

            doors.Add(FrontLeftDoor);
            doors.Add(FrontRightDoor);

            if (!IsTwoDoorsVehicle)
            {
                doors.Add(RearLeftDoor);
                doors.Add(RearRightDoor);
            }

            foreach (Transform door in doors)
            {
                // Initializate VehicleDoor script here
                yield return VehicleDoor.Init(door, this, false);
            }
        }

        public IEnumerable<VehicleBehaviour> SetAllCarLights()
        {
            // Implemented: Add lights

            Transform headlights = this.GetComponentWithName<Transform>("headlights"),
                      taillights = this.GetComponentWithName<Transform>("taillights");

            Vehicle vh = gameObject.GetComponent<Vehicle>();

            if (headlights != null)
            {
                yield return VehicleLights.Init(headlights, vh, VehicleLight.FrontLeft);
                yield return VehicleLights.Init(headlights, vh, VehicleLight.FrontRight);
            }

            if (taillights != null)
            {
                yield return VehicleLights.Init(taillights, vh, VehicleLight.RearLeft);
                yield return VehicleLights.Init(taillights, vh, VehicleLight.RearRight);
            }
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
            float horAxis = Input.GetAxis("Horizontal");

            // Set car lights
            if (HasDriver)
            {
                if (horAxis != 0)
                    blinkerMode = horAxis < 0 ? VehicleBlinkerMode.Left : VehicleBlinkerMode.Right;
                else if (horAxis == 0 && Steering == 0 && blinkerMode != VehicleBlinkerMode.None)
                    StartCoroutine(DelayedBlinkersTurnOff());
            }

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
                // Flip car
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (Vector3.Dot(transform.up, Vector3.down) > 0)
                    {
                        transform.position += Vector3.up * 1.5f;
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
                    }
                }

                if (Braking > 0.125f)
                    SetMultipleLights(VehicleLight.Rear, 1f);
                else
                {
                    //if(hasRearBrightness)
                        SetMultipleLights(VehicleLight.Rear, 0f);
                }
            }
            else
            {
                if (IsAnyLightPowered())
                    SetMultipleLights(VehicleLight.All, 0f);

                Braking = 1f;
            }

            if (_colorsChanged)
            {
                UpdateColors();
            }
        }

        private void FixedUpdate()
        {
            //angularVelocity = GetComponent<Rigidbody>().angularVelocity.magnitude;

            //    NetworkingFixedUpdate();
            PhysicsFixedUpdate();
        }

        private void OnGUI()
        {
            if (HasDriver)
            {
                GUI.BeginGroup(new Rect(Screen.width - 205, Screen.height - 205 - 50, 200, 200));
                GUI.Box(new Rect(0, 0, 200, 200), "");
                GUI.Label(new Rect(5, 5, 200, 35), "Vehicle Stats", new GUIStyle("label") { fontSize = 30, fontStyle = FontStyle.Bold });
                bool moreThan2Doors = vehicleDoors.Count() > 2;
                GUI.Label(new Rect(5, 45, 200, moreThan2Doors ? 40 : 20), string.Format("Doors: {0}", 
                    string.Join(" | ", vehicleDoors.Select((x, i) => 
                    string.Format("({0}) {1}{2}", x.transform.name.Substring(5, 2).ToUpper(), x.LockHealth.ToString("F2"), moreThan2Doors && i == 1 ? Environment.NewLine : "")))));
                GUI.EndGroup();
            }
        }

        private IEnumerator DelayedBlinkersTurnOff()
        {
            yield return new WaitForSeconds(blinkerSum);

            if (blinkerMode != VehicleBlinkerMode.None)
                blinkerMode = VehicleBlinkerMode.None;
        }
    }
}