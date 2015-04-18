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

    public partial class Vehicle : Networking.Networkable
    {
        private static int _sLayer = -1;
        public static int Layer
        {
            get { return _sLayer == -1 ? _sLayer = LayerMask.NameToLayer("Vehicle") : _sLayer; }
        }

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

        private readonly Color32[] _colors = { Color.white, Color.white, Color.white, Color.white };
        private readonly float[] _lights = { 1f, 1f, 1f, 1f };
        private readonly MaterialPropertyBlock _props = new MaterialPropertyBlock();
        private bool _colorsChanged;

        public void SetColors(params Color32[] clrs)
        {
            for (var i = 0; i < 4 && i < clrs.Length; ++i) {
                if (_colors[i].Equals(clrs[i])) continue;
                _colors[i] = clrs[i];
                _colorsChanged = true;
            }
        }

        public void SetLight(VehicleLight light, float brightness)
        {
            brightness = Mathf.Clamp01(brightness);

            for (var i = 0; i < 4; ++i) {
                var bit = 1 << i;
                if (((int) light & bit) == bit) {
                    SetLight(i, brightness);
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

        private void UpdateColors()
        {
            _colorsChanged = false;

            for (var i = 0; i < 4; ++i) {
                _props.SetColor(CarColorIds[i], _colors[i]);
            }

            _props.SetVector(LightsId, new Vector4(_lights[0], _lights[1], _lights[2], _lights[3]));

            foreach (var frame in _frames) {
                var mr = frame.GetComponent<MeshRenderer>();
                if (mr == null) continue;
                mr.SetPropertyBlock(_props);
            }
        }

        private void Update()
        {
            foreach (var wheel in _wheels) {
                Vector3 position = Vector3.zero;

                WheelHit wheelHit;

                if (wheel.Collider.GetGroundHit(out wheelHit)) {
                    position.y = (wheelHit.point.y - wheel.Collider.transform.position.y) + wheel.Collider.radius;
                } else {
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

            if (Braking > 0f || Vector3.Dot(_rigidBody.velocity, transform.forward) > 0f && Accelerator < 0f) {
                SetLight(VehicleLight.Rear, 1f);
            } else {
                SetLight(VehicleLight.Rear, 0f);
            }

            if (_colorsChanged) {
                UpdateColors();
            }
        }
    }
}
