using UnityEngine;

namespace LlamaSoftware
{
    public class FreeCamera : MonoBehaviour
    {
        [SerializeField]
        private bool EnableInputCapture = true;

        [SerializeField]
        private bool HoldRightMouseCapture = false;

        [SerializeField]
        private float lookSpeed = 3f;

        [SerializeField]
        private float moveSpeed = 3f;

        [SerializeField]
        private float FastMoveSpeed = 10f;

        private bool InputCaptured;
        private float Yaw;
        private float Pitch;
        private float RotationX;
        private float RotationY;
        private float Speed;
        private float Forward;
        private float Right;
        private float Up;

        private void Start()
        {
            enabled = EnableInputCapture;
        }

        private void CaptureInput()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InputCaptured = true;

            Yaw = transform.eulerAngles.y;
            Pitch = transform.eulerAngles.x;
        }

        private void ReleaseInput()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            InputCaptured = false;
        }

        private void Update()
        {
            if (!InputCaptured)
            {
                if (!HoldRightMouseCapture && Input.GetMouseButtonDown(0))
                {
                    CaptureInput();
                }
                else if (HoldRightMouseCapture && Input.GetMouseButtonDown(1))
                {
                    CaptureInput();
                }
            }

            if (!InputCaptured)
            {
                return;
            }

            if (InputCaptured)
            {
                if (!HoldRightMouseCapture && Input.GetKeyDown(KeyCode.Escape))
                {
                    ReleaseInput();
                }
                else if (HoldRightMouseCapture && Input.GetMouseButtonUp(1))
                {
                    ReleaseInput();
                }
            }

            RotationX = Input.GetAxis("Mouse X");
            RotationY = Input.GetAxis("Mouse Y");

            Yaw = (Yaw + lookSpeed * RotationX) % 360f;
            Pitch = (Pitch - lookSpeed * RotationY) % 360f;
            transform.rotation = Quaternion.AngleAxis(Yaw, Vector3.up) * Quaternion.AngleAxis(Pitch, Vector3.right);

            Speed = Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? FastMoveSpeed : moveSpeed);

            Forward = Speed * Input.GetAxis("Vertical");
            Right = Speed * Input.GetAxis("Horizontal");

            Up = Speed * ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));
            transform.position += (transform.forward * Forward) + (transform.right * Right) + (Vector3.up * Up);
        }
    }
}