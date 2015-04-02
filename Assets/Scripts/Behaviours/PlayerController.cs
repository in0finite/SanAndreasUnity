using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private Vector3 _velocity;

        private float _pitch;
        private float _yaw;

        private bool _lockedCursor;

        private Vector3 _move;

        private CharacterController _controller;

        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        public Vector2 PitchClamp = new Vector2(-89f, 89f);
        public Vector2 YawClamp = new Vector2(-180f, 180f);

        public float VelocitySmoothing = 0.05f;

        public Camera Camera;

        public float Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = Mathf.Clamp(value, PitchClamp.x, PitchClamp.y);

                var angles = Camera.transform.localEulerAngles;
                angles.x = _pitch;
                Camera.transform.localEulerAngles = angles;
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

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
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

                _move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

                if (_move.sqrMagnitude > 0f) {
                    _move.Normalize();
                    _move = transform.forward * _move.z + transform.right * _move.x;

                    if (Input.GetKey(KeyCode.LeftShift)) {
                        _move *= 12f;
                    } else {
                        _move *= 4f;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            var vDiff = _move - new Vector3(_velocity.x, 0f, _velocity.z);

            _velocity += vDiff * (1f - Mathf.Pow(VelocitySmoothing, 4f * Time.fixedDeltaTime));

            if (_controller.isGrounded) {
                _velocity.y = 0f;
            } else {
                _velocity.y -= 9.81f * 2f * Time.fixedDeltaTime;
            }

            _controller.Move(_velocity * Time.fixedDeltaTime);
        }
    }
}
