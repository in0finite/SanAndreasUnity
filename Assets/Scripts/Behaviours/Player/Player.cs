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

        public float TurnSpeed = 10f;

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
            transform.SetParent(vehicle.DriverTransform.parent);
            transform.localPosition = Vector3.Scale(vehicle.DriverTransform.localPosition, new Vector3(-1.0f, 1.0f, 1.0f));
            transform.localRotation = Quaternion.identity;

            vehicle.gameObject.AddComponent<VehicleController>();

            PlayerModel.LoadAnim("Car_sit");
            PlayerModel.LoadAnim("Drive_L");
            PlayerModel.LoadAnim("Drive_R");
        }

        private void Update()
        {
            if (IsInVehicle)
            {
                var driveState = CurrentVehicle.Steering > 0 ? PlayerModel.Anim["Drive_R"] : PlayerModel.Anim["Drive_L"];

                PlayerModel.Anim.Play(driveState.name, PlayMode.StopAll);

                driveState.speed = 0.0f;
                driveState.wrapMode = WrapMode.ClampForever;
                driveState.time = Mathf.Lerp(0.0f, driveState.length, Mathf.Abs(CurrentVehicle.Steering));
            }

        }

        private void FixedUpdate()
        {
            NetworkingFixedUpdate();

            if (IsInVehicle) return;

            var forward = Vector3.RotateTowards(transform.forward, Heading, TurnSpeed * Time.deltaTime, 0.0f);
            transform.localRotation = Quaternion.LookRotation(forward);

            if (IsLocalPlayer) {
                if (Movement.sqrMagnitude > float.Epsilon) {
                    Heading = Vector3.Scale(Movement, new Vector3(1f, 0f, 1f)).normalized;
                }

                var vDiff = Movement * PlayerModel.Speed - new Vector3(Velocity.x, 0f, Velocity.z);

                Velocity += vDiff;
                Velocity = new Vector3(Velocity.x, _controller.isGrounded
                    ? 0f : Velocity.y - 9.81f * 2f * Time.fixedDeltaTime, Velocity.z);

                _controller.Move(Velocity * Time.fixedDeltaTime);
            } else {
                Velocity = _controller.velocity;
            }
        }
    }
}
