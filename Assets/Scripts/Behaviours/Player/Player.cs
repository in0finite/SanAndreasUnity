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
        public bool IsInVehicleSeat { get; private set; }
        public bool IsDrivingVehicle { get; private set; }

        private Vehicle.SeatAlignment _currentVehicleSeatAlignment;

        #endregion

        public Player()
        {
            _snapshots = new SnapshotBuffer<PlayerPedestrianState>(.25d);
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

        public void EnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment)
        {
            if (IsInVehicle) return;

            _controller.enabled = false;
            CurrentVehicle = vehicle;

            var timer = new Stopwatch();
            timer.Start();

            var seat = vehicle.GetSeat(seatAlignment);

            Camera.transform.SetParent(seat.Parent, true);
            transform.SetParent(seat.Parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (seat.IsDriver)
            {
                vehicle.StartControlling();
            }

            PlayerModel.IsInVehicle = true;

            _currentVehicleSeatAlignment = seat.Alignment;

            StartCoroutine(EnterVehicleAnimation(seat));
        }

        public void ExitVehicle()
        {
            if (!IsInVehicle || !IsInVehicleSeat) return;

            CurrentVehicle.StopControlling();

            StartCoroutine(ExitVehicleAnimation());
        }

        private IEnumerator EnterVehicleAnimation(Vehicle.Seat seat)
        {
            var animIndex = seat.IsLeftHand ? AnimIndex.GetInLeft : AnimIndex.GetInRight;

            PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootEnd, new Vector3(-1, -1, -1));

            var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
            animState.wrapMode = WrapMode.Once;

            while (animState.enabled)
            {
                yield return new WaitForEndOfFrame();
            }

            if (seat.IsDriver)
            {
                IsDrivingVehicle = true;

                PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.Sit, PlayMode.StopAll);
            }
            else
            {
                PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.SitPassenger, PlayMode.StopAll);
            }

            IsInVehicleSeat = true;
        }

        private IEnumerator ExitVehicleAnimation()
        {
            IsDrivingVehicle = false;
            IsInVehicleSeat = false;

            var seat = CurrentVehicle.GetSeat(_currentVehicleSeatAlignment);

            var animIndex = seat.IsLeftHand ? AnimIndex.GetOutLeft : AnimIndex.GetOutRight;

            PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootStart, new Vector3(-1, -1, -1));

            var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
            animState.wrapMode = WrapMode.Once;

            while (animState.enabled)
            {
                yield return new WaitForEndOfFrame();
            }

            PlayerModel.IsInVehicle = false;

            CurrentVehicle = null;
            _currentVehicleSeatAlignment = Vehicle.SeatAlignment.None;

            transform.localPosition = PlayerModel.VehicleParentOffset;
            transform.localRotation = Quaternion.identity;

            Camera.transform.SetParent(null, true);
            transform.SetParent(null);

            _controller.enabled = true;

            PlayerModel.VehicleParentOffset = Vector3.zero;
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
