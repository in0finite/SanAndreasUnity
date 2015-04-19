using System.Diagnostics;
using System.Collections;
using Facepunch.Networking;
using ProtoBuf.Player;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(CharacterController))]
    public partial class Player : Networking.Networkable
    {
        #region Private fields

        private CharacterController _controller;

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
        public bool IsDrivingVehicle { get; private set; }

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

            Cell.Focus = transform;
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
            transform.SetParent(vehicle.DriverTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            vehicle.StartControlling();

            StartCoroutine(DriveVehicle());
        }

        private IEnumerator DriveVehicle()
        {
            if (IsDrivingVehicle) yield break;

            PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, AnimIndex.GetInLeft).RootEnd, new Vector3(-1, -1, -1));

            var animState = PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.GetInLeft, PlayMode.StopAll);
            animState.wrapMode = WrapMode.Once;

            while (animState.enabled)
            {
                yield return new WaitForEndOfFrame();
            }

            IsDrivingVehicle = true;
        }

        private void UpdateWheelTurning()
        {
            PlayerModel.VehicleParentOffset = Vector3.zero;

            var driveState = CurrentVehicle.Steering > 0 ? AnimIndex.DriveRight : AnimIndex.DriveLeft;

            var state = PlayerModel.PlayAnim(AnimGroup.Car, driveState, PlayMode.StopAll);

            state.speed = 0.0f;
            state.wrapMode = WrapMode.ClampForever;
            state.time = Mathf.Lerp(0.0f, state.length, Mathf.Abs(CurrentVehicle.Steering));
        }

        private void Update()
        {
            PlayerModel.OnFoot = !IsInVehicle;

            if (IsInVehicle && IsDrivingVehicle)
            {
                UpdateWheelTurning();
            }
        }

        private void FixedUpdate()
        {
            NetworkingFixedUpdate();

            if (IsInVehicle) return;

            var forward = Vector3.RotateTowards(transform.forward, Heading, TurnSpeed * Time.deltaTime, 0.0f);
            transform.localRotation = Quaternion.LookRotation(forward);

            if (IsLocalPlayer)
            {
                if (Movement.sqrMagnitude > float.Epsilon)
                {
                    Heading = Vector3.Scale(Movement, new Vector3(1f, 0f, 1f)).normalized;
                }

                var vDiff = Movement * PlayerModel.Speed - new Vector3(Velocity.x, 0f, Velocity.z);

                Velocity += vDiff;
                Velocity = new Vector3(Velocity.x, _controller.isGrounded
                    ? 0f : Velocity.y - 9.81f * 2f * Time.fixedDeltaTime, Velocity.z);

                _controller.Move(Velocity * Time.fixedDeltaTime);
            }
            else
            {
                Velocity = _controller.velocity;
            }
        }
    }
}
