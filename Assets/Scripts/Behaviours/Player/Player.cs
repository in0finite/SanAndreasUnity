using System.Diagnostics;
using System.Collections;
//using Facepunch.Networking;
//using ProtoBuf.Player;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(CharacterController))]
	public partial class Player : MonoBehaviour
    {
        #region Private fields

        private CharacterController _controller;

        #endregion

        #region Inspector Fields

        public Camera Camera;
        public Pedestrian PlayerModel;

        public float TurnSpeed = 10f;

		public	Weapon[] weapons = new Weapon[(int)WeaponSlot.Count] ;
		public	int	currentWeaponSlot = -1;
		public	bool	autoAddWeapon = false ;

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

   //     protected override void OnAwake()
		protected void Awake()
        {
        //    base.OnAwake();

            _controller = GetComponent<CharacterController>();

			IsLocalPlayer = true;
        }

		void	Start() {

		//	MySetupLocalPlayer ();

		}

		private void MySetupLocalPlayer()
		{
			Camera.gameObject.SetActive(true);
			Camera.transform.SetParent(null, true);

			Cell.Focus = transform;
			Cell.PreviewCamera.gameObject.SetActive(false);

			gameObject.AddComponent<PlayerController>();
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

        public void EnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate = false)
        {
            if (IsInVehicle) return;

            CurrentVehicle = vehicle;

            var timer = new Stopwatch();
            timer.Start();

            var seat = vehicle.GetSeat(seatAlignment);

            _controller.enabled = false;

            if (IsLocalPlayer) {
                Camera.transform.SetParent(seat.Parent, true);

				/*
                SendToServer(_lastPassengerState = new PlayerPassengerState {
                    Vechicle = vehicle,
                    SeatAlignment = (int) seatAlignment
                }, DeliveryMethod.ReliableOrdered, 1);
                */
            }

            transform.SetParent(seat.Parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (IsLocalPlayer && seat.IsDriver) {
                vehicle.StartControlling();
            }

            PlayerModel.IsInVehicle = true;

            _currentVehicleSeatAlignment = seat.Alignment;

            StartCoroutine(EnterVehicleAnimation(seat, immediate));
        }

        public void ExitVehicle(bool immediate = false)
        {
            if (!IsInVehicle || !IsInVehicleSeat) return;

            CurrentVehicle.StopControlling();

            if (IsLocalPlayer) {
				/*
                SendToServer(_lastPassengerState = new PlayerPassengerState {
                    Vechicle = null
                }, DeliveryMethod.ReliableOrdered, 1);
                */
            } else {
            //    _snapshots.Reset();
            }

            StartCoroutine(ExitVehicleAnimation(immediate));
        }

        private IEnumerator EnterVehicleAnimation(Vehicle.Seat seat, bool immediate)
        {
            var animIndex = seat.IsLeftHand ? AnimIndex.GetInLeft : AnimIndex.GetInRight;

            PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootEnd, new Vector3(-1, -1, -1));

            if (!immediate) {
                var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
                animState.wrapMode = WrapMode.Once;

                while (animState.enabled) {
                    yield return new WaitForEndOfFrame();
                }
            }

            if (seat.IsDriver) {
                IsDrivingVehicle = true;

                PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.Sit, PlayMode.StopAll);
            } else {
                PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.SitPassenger, PlayMode.StopAll);
            }

            IsInVehicleSeat = true;
        }

        private IEnumerator ExitVehicleAnimation(bool immediate)
        {
            IsDrivingVehicle = false;
            IsInVehicleSeat = false;

            var seat = CurrentVehicle.GetSeat(_currentVehicleSeatAlignment);

            var animIndex = seat.IsLeftHand ? AnimIndex.GetOutLeft : AnimIndex.GetOutRight;

            PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootStart, new Vector3(-1, -1, -1));

            if (!immediate) {
                var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
                animState.wrapMode = WrapMode.Once;

                while (animState.enabled) {
                    yield return new WaitForEndOfFrame();
                }
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
			if (!Loader.HasLoaded)
				return;

			// Reset to a valid (and solid!) start position when falling below the world
			if (transform.position.y < -300) {
				transform.position = GameObject.Find("LV casino 2").GetComponent<Transform>().position;
			}

            if (IsInVehicle && IsDrivingVehicle) {
                UpdateWheelTurning();
            }

			// switch weapons - does not work
			if (!IsInVehicle) {
				if (Input.mouseScrollDelta.y != 0) {
					
					if (currentWeaponSlot < 0)
						currentWeaponSlot = 0;
					
					for( int i=currentWeaponSlot + (int) Mathf.Sign(Input.mouseScrollDelta.y), count=0;
						i != currentWeaponSlot && count < (int) WeaponSlot.Count ;
						i += (int) Mathf.Sign(Input.mouseScrollDelta.y), count++ ) {

						if (i < 0)
							i = weapons.Length - 1;
						if (i >= weapons.Length)
							i = 0;

						if (weapons [i] != null) {
							this.SwitchWeapon (i);
							break;
						}
					}

				}
			}

			// add weapons to player if he doesn't have any
			if (autoAddWeapon && null == System.Array.Find (weapons, w => w != null)) {
				// player has no weapons

				weapons[(int)WeaponSlot.Machine] = Weapon.Load(355);
				this.SwitchWeapon ((int)WeaponSlot.Machine);
			}

        }

        private void FixedUpdate()
        {
			if (!Loader.HasLoaded)
				return;
			
        //    NetworkingFixedUpdate();

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

		private	void	SwitchWeapon( int slotIndex ) {

			if (PlayerModel.weapon != null) {
				// set parent to weapons container in order to hide it
			//	PlayerModel.weapon.SetParent (Weapon.weaponsContainer.transform);

				PlayerModel.weapon.gameObject.SetActive (false);
			}

			PlayerModel.weapon = weapons [slotIndex].gameObject.transform;
			// change parent to make it visible
		//	PlayerModel.weapon.SetParent(this.transform);
			PlayerModel.weapon.gameObject.SetActive(true);

			currentWeaponSlot = slotIndex;

		}

    }
}
