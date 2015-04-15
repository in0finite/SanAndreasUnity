using System.Linq;
using UnityEngine;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.VehicleDef;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle : MonoBehaviour
    {
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
        private readonly MaterialPropertyBlock _props = new MaterialPropertyBlock();
        private bool _colorsChanged;

        public VehicleDef Definition { get; private set; }

        public void SetColors(params Color32[] clrs)
        {
            for (var i = 0; i < 4 && i < clrs.Length; ++i) {
                if (_colors[i].Equals(clrs[i])) continue;
                _colors[i] = clrs[i];
                _colorsChanged = true;
            }
        }

        public Transform DriverTransform
        {
            get { return GetPart("ped_frontseat"); }
        }

        private void UpdateColors()
        {
            for (var i = 0; i < 4; ++i) {
                _props.SetColor(CarColorIds[i], _colors[i]);
            }

            foreach (var frame in _frames) {
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

            if (_colorsChanged) {
                _colorsChanged = false;
                UpdateColors();
            }
        }
    }
}
