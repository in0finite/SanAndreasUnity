using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Collections;
using SanAndreasUnity.Behaviours.Vehicles;
using System.Diagnostics;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour
    {
        #region Private fields

        private bool _lockedCursor;

        private Player _player;

        private float _pitch;
        private float _yaw;

        #endregion

        #region Inspector Fields

        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        public float CarCameraDistance = 6.0f;
        public float PlayerCameraDistance = 2.0f;

        public Vector2 PitchClamp = new Vector2(-89f, 89f);

        #endregion

        #region Properties

        public Camera Camera { get { return _player.Camera; } }
        public Pedestrian PlayerModel { get { return _player.PlayerModel; } }

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
                _yaw = value.NormalizeAngle();

                var trans = Camera.transform;
                var angles = trans.localEulerAngles;
                angles.y = _yaw;
                trans.localEulerAngles = angles;
            }
        }

        #endregion

        private void Awake()
        {
            _player = GetComponent<Player>();
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

            if (_lockedCursor)
            {
                var cursorDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

                Yaw += cursorDelta.x * CursorSensitivity.x;
                Pitch -= cursorDelta.y * CursorSensitivity.y;
            }

            Camera.transform.rotation = Quaternion.AngleAxis(Yaw, Vector3.up)
                * Quaternion.AngleAxis(Pitch, Vector3.right);

            float distance;
            Transform castFrom;

            if (_player.IsInVehicle) {
                CarCameraDistance = Mathf.Clamp(CarCameraDistance - Input.mouseScrollDelta.y, 2.0f, 32.0f);
                distance = CarCameraDistance;
                castFrom = _player.CurrentVehicle.transform;
            } else {
                PlayerCameraDistance = Mathf.Clamp(PlayerCameraDistance - Input.mouseScrollDelta.y, 2.0f, 32.0f);
                distance = PlayerCameraDistance;
                castFrom = transform;
            }

            var castRay = new Ray(castFrom.position, -Camera.transform.forward);

            RaycastHit hitInfo;

            if (Physics.SphereCast(castRay, 0.25f, out hitInfo, distance)) {
                distance = hitInfo.distance;
            }

            Camera.transform.position = castRay.GetPoint(distance);

            if (_player.IsInVehicle) return;
            if (!_lockedCursor) return;

            var inputMove = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            if (inputMove.sqrMagnitude > 0f)
            {
                inputMove.Normalize();

                if (Input.GetKey(KeyCode.LeftShift)) {
                    PlayerModel.Running = true;
                } else {
                    PlayerModel.Walking = true;
                }
            } else {
                PlayerModel.Walking = false;
            }

            _player.Movement = Vector3.Scale(Camera.transform.TransformVector(inputMove),
                new Vector3(1f, 0f, 1f)).normalized;

            if (!Input.GetButtonDown("Use")) return;

            foreach (var vehicle in FindObjectsOfType<Vehicle>()) {
                var dist = Vector3.Distance(transform.position, vehicle.transform.position);
                if (dist > 10f) continue;

                var ray = new Ray(transform.position, vehicle.transform.position - transform.position);
                if (!vehicle.GetComponentsInChildren<MeshCollider>().Any(
                    x => x.Raycast(ray, out hitInfo, 1.5f))) continue;

                _player.EnterVehicle(vehicle);
                break;
            }
        }
    }
}
