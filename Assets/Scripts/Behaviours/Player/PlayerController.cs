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

        #endregion

        #region Inspector Fields

        public Vector2 CursorSensitivity = new Vector2(2f, 2f);

        public float CarCameraDistance = 6.0f;
        public float PlayerCameraDistance = 2.0f;

        #endregion

        #region Properties

        public Camera Camera { get { return _player.Camera; } }
        public Pedestrian PlayerModel { get { return _player.PlayerModel; } }

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

                _player.Yaw += cursorDelta.x * CursorSensitivity.x;
                _player.Pitch -= cursorDelta.y * CursorSensitivity.y;
            }

            Camera.transform.rotation = Quaternion.AngleAxis(_player.Yaw, Vector3.up)
                * Quaternion.AngleAxis(_player.Pitch, Vector3.right);

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
            float moveSpeed = 0.0f;

            if (inputMove.sqrMagnitude > 0f)
            {
                inputMove.Normalize();

                _player.Heading = Quaternion.AngleAxis(_player.Yaw, Vector3.up) * inputMove;

                if (Input.GetKey(KeyCode.LeftShift)) {
                    moveSpeed = _player.RunSpeed;
                    PlayerModel.Running = true;
                } else {
                    moveSpeed = _player.WalkSpeed;
                    PlayerModel.Walking = true;
                }
            }

            _player.Movement = transform.forward * moveSpeed;

            if (Input.GetButtonDown("Use")) {
                foreach (var vehicle in FindObjectsOfType<Vehicle>()) {
                    var dist = Vector3.Distance(Camera.transform.position, vehicle.transform.position);
                    if (dist > 10f) continue;

                    var ray = new Ray(Camera.transform.position, Camera.transform.forward);
                    if (!vehicle.GetComponentsInChildren<MeshCollider>().Any(
                        x => x.Raycast(ray, out hitInfo, 10f))) continue;

                    _player.EnterVehicle(vehicle);
                    break;
                }
            }
        }
    }
}
