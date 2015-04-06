using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Collections;
using SanAndreasUnity.Behaviours.Vehicles;
using System.Diagnostics;
using System.Linq;

namespace SanAndreasUnity.Behaviours.Player
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

        public Vehicle CurrentVehicle { get; private set; }

        public bool IsInVehicle { get { return CurrentVehicle != null; } }

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

                var trans = IsInVehicle ? Camera.transform : transform;
                var angles = trans.localEulerAngles;
                angles.y = _yaw;
                trans.localEulerAngles = angles;
            }
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
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

            if (!_lockedCursor) return;

            var cursorDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            Yaw += cursorDelta.x * CursorSensitivity.x;
            Pitch -= cursorDelta.y * CursorSensitivity.y;

            if (IsInVehicle) {
                Camera.transform.rotation = Quaternion.AngleAxis(Yaw, Vector3.up) * Quaternion.AngleAxis(Pitch, Vector3.right);
                Camera.transform.position = CurrentVehicle.transform.position - Camera.transform.forward * 8f;
                return;
            }

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

            if (Input.GetButtonDown("Use")) {
                foreach (var vehicle in FindObjectsOfType<Vehicle>()) {
                    var dist = Vector3.Distance(Camera.transform.position, vehicle.transform.position);
                    if (dist > 10f) continue;

                    RaycastHit hitInfo;
                    var ray = Camera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
                    if (!vehicle.GetComponentsInChildren<MeshCollider>().Any(
                        x => x.Raycast(ray, out hitInfo, 2f))) continue;

                    EnterVehicle(vehicle);
                    break;
                }
            }
        }

        private void FixedUpdate()
        {
            if (IsInVehicle) return;

            var vDiff = _move - new Vector3(_velocity.x, 0f, _velocity.z);

            _velocity += vDiff * (1f - Mathf.Pow(VelocitySmoothing, 4f * Time.fixedDeltaTime));

            if (_controller.isGrounded) {
                _velocity.y = 0f;
            } else {
                _velocity.y -= 9.81f * 2f * Time.fixedDeltaTime;
            }

            _controller.Move(_velocity * Time.fixedDeltaTime);
        }

        public void EnterVehicle(Vehicle vehicle)
        {
            if (CurrentVehicle != null) return;

            _controller.enabled = false;
            CurrentVehicle = vehicle;

            var timer = new Stopwatch();
            timer.Start();

            Camera.transform.SetParent(vehicle.DriverTransform, true);
            transform.SetParent(vehicle.transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            vehicle.gameObject.AddComponent<VehicleController>();
        }
    }
}
