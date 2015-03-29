using UnityEngine;
using System.Collections;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
    public class CameraController : MonoBehaviour
    {
        private Vector3 _velocity;

        private float _pitch;
        private float _yaw;

        private bool _lockedCursor;

        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        public Vector2 PitchClamp = new Vector2(-89f, 89f);
        public Vector2 YawClamp = new Vector2(-180f, 180f);

        public float Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = Mathf.Clamp(value, PitchClamp.x, PitchClamp.y);

                var angles = transform.localEulerAngles;
                angles.x = _pitch;
                transform.localEulerAngles = angles;
            }
        }

        public float Yaw
        {
            get { return _yaw; }
            set
            {
                _yaw = Mathf.Clamp(value.NormalizeAngle(), YawClamp.x, YawClamp.y);

                var angles = transform.localEulerAngles;
                angles.y = _yaw;
                transform.localEulerAngles = angles;
            }
        }

        void Update()
        {
            if (!_lockedCursor && Input.GetMouseButtonDown(0)) {
                _lockedCursor = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else if (_lockedCursor && Input.GetKeyDown(KeyCode.Escape)) {
                _lockedCursor = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (_lockedCursor) {
                var cursorDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

                Yaw += cursorDelta.x * CursorSensitivity.x;
                Pitch -= cursorDelta.y * CursorSensitivity.y;
            }
        }

        void FixedUpdate()
        {
            var move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            if (move.sqrMagnitude > 0f) {
                move.Normalize();
                move = transform.forward * move.z + transform.right * move.x;

                if (Input.GetKey(KeyCode.LeftShift)) {
                    move *= 4f;
                }
            }

            _velocity += (move - _velocity) * .5f;
            transform.position += _velocity;
        }
    }
}
