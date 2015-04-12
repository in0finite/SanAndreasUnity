using System.Diagnostics;
using Facepunch.Networking;
using ProtoBuf.Player;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(CharacterController))]
    public partial class Player : Networking.Networkable
    {
        #region Private fields
        
        private CharacterController _controller;

        private float _pitch;

        #endregion

        #region Inspector Fields

        public Camera Camera;
        public Pedestrian PlayerModel;

        public float WalkSpeed = 1.5f;
        public float RunSpeed = 5f;
        public float TurnSpeed = 10f;

        public float VelocitySmoothing = 0.05f;

        #endregion
        
        #region Properties

        public Cell Cell { get { return Cell.Instance; } }

        public Vector3 Position
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        public Vector3 Velocity { get; private set; }
        public Vector3 Movement { get; set; }

        public Vector3 Heading { get; private set; }
        public Vehicle CurrentVehicle { get; private set; }

        public bool IsInVehicle { get { return CurrentVehicle != null; } }

        #endregion

        public Player()
        {
            _snapshots = new SnapshotBuffer<PlayerState>(.25d);
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            _controller = GetComponent<CharacterController>();
        }

#if CLIENT

        private void SetupLocalPlayer()
        {
            Camera.gameObject.SetActive(true);
            Camera.transform.SetParent(null, true);

            Cell.Focus = Camera.transform;
            Cell.PreviewCamera.gameObject.SetActive(false);

            gameObject.AddComponent<PlayerController>();
        }

#endif

        public void EnterVehicle(Vehicle vehicle)
        {
            if (IsInVehicle) return;

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

        private void FixedUpdate()
        {
            NetworkingFixedUpdate();

            var forward = Vector3.RotateTowards(transform.forward, Heading, TurnSpeed * Time.deltaTime, 0.0f);
            transform.localRotation = Quaternion.LookRotation(forward);

            if (IsInVehicle) return;

            if (IsLocalPlayer) {
                if (Movement.sqrMagnitude > float.Epsilon) {
                    Heading = Vector3.Scale(Movement, new Vector3(1f, 0f, 1f)).normalized;
                }

                var vDiff = Movement - new Vector3(Velocity.x, 0f, Velocity.z);

                Velocity += vDiff * (1f - Mathf.Pow(VelocitySmoothing, 4f * Time.fixedDeltaTime));
                Velocity = new Vector3(Velocity.x, _controller.isGrounded
                    ? 0f : Velocity.y - 9.81f * 2f * Time.fixedDeltaTime, Velocity.z);

                _controller.Move(Velocity * Time.fixedDeltaTime);
            } else {
                Velocity = _controller.velocity;
            }
        }
    }
}
