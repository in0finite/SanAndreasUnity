using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(CharacterController))]
#if CLIENT
    public partial class Player : Networking.Networkable
#else
    public partial class Player : MonoBehaviour
#endif
    {
        #region Private Fields

		private WeaponHolder m_weaponHolder;
		public WeaponHolder WeaponHolder { get { return m_weaponHolder; } }

        private int jumpTimer;

        #endregion Private Fields

        #region Inspector Fields

        public Camera Camera;
        public Pedestrian PlayerModel;

		public bool shouldPlayAnims = true;

        public float TurnSpeed = 10f;

        public bool enableFlying = false;
        public bool enableNoclip = false;

        public CharacterController characterController;

        public float jumpSpeed = 8.0f;

        // Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast
        public float antiBumpFactor = .75f;

        // Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
        public int antiBunnyHopFactor = 1;

        #endregion Inspector Fields

        #region Properties

        public Cell Cell { get { return Cell.Instance; } }

		public UnityEngine.Animation AnimComponent { get { return PlayerModel.AnimComponent; } }

        public Vector3 Position
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        public bool IsGrounded
        {
            get
            {
                return characterController.isGrounded;
            }
        }

		public bool IsWalking { get; set; }
		public bool IsRunning { get; set; }
		public bool IsSprinting { get; set; }

        public Vector3 Velocity { get; private set; }
		/// <summary> Current movement input. </summary>
        public Vector3 Movement { get; set; }
		/// <summary> Direction towards which the player turns. </summary>
        public Vector3 Heading { get; set; }
        
		public Vehicle CurrentVehicle { get; private set; }
		public bool IsInVehicle { get { return CurrentVehicle != null; } }
        public bool IsInVehicleSeat { get; private set; }
        public bool IsDrivingVehicle { get; private set; }

        private Vehicle.SeatAlignment _currentVehicleSeatAlignment;

		public bool IsAiming { get { return m_weaponHolder.IsAiming; } }
		public Weapon CurrentWeapon { get { return m_weaponHolder.CurrentWeapon; } }

        private static bool makeGPUAdjustments;

        #endregion Properties


		public	static	Player	Instance { get ; private set ; }

		public	static	Player	FindInstance() {
			return FindObjectOfType<Player> ();
		}

		/// <summary>Position of player instance.</summary>
		public	static	Vector3	InstancePos { get { return Instance.transform.position; } }



        //     protected override void OnAwake()
        protected void Awake()
        {
            //    base.OnAwake();

            Instance = this;

            characterController = GetComponent<CharacterController>();
			m_weaponHolder = GetComponent<WeaponHolder> ();

            IsLocalPlayer = true;

            // Only debug

            //foreach (var go in gameObject.GetComponentsInChildren<Component>())
            //    Debug.LogFormat("Name: {0} => {1}", go.name, go.hideFlags);

            Debug.LogFormat("Shader level: {0}", SystemInfo.graphicsShaderLevel);

            Debug.LogFormat("Max FPS: {0}", Application.targetFrameRate);

            StartCoroutine(GPUAdjust());
        }

        private void Start()
        {
            //	MySetupLocalPlayer ();
        }

        private IEnumerator GPUAdjust()
        {
            // Wait to everything to load
            yield return new WaitForSeconds(1);

			if (SystemInfo.graphicsShaderLevel <= 40) {
				try {
					Debug.Log("Adjusting settings for older GPUs");

					Camera.main.allowMSAA = false;
					Camera.main.allowHDR = false;

					foreach (var mat in transform.root.GetComponentsInChildren<Material>()) {
						mat.EnableKeyword ("_SPECULARHIGHLIGHTS_OFF");
						mat.SetFloat ("_SpecularHighlights", 0f);
					}
				} catch {
				}
			}

        }

        private void MySetupLocalPlayer()
        {
            Camera.gameObject.SetActive(true);
            Camera.transform.SetParent(null, true);

            Cell.Focus = transform;
            Cell.PreviewCamera.gameObject.SetActive(false);

            gameObject.AddComponent<PlayerController>();
        }

        public void OnSpawn()
        {
            // Note: Spawn is performed here.

            jumpTimer = antiBunnyHopFactor;

			if (!IsGrounded) {
				// Find the ground (instead of falling)
				FindGround ();
			}

        }

		public void Teleport(Vector3 position, Quaternion rotation) {

			this.transform.position = position;
			this.transform.rotation = rotation;

			this.FindGround ();

		}

		public void Teleport(Vector3 position) {

			this.Teleport (position, this.transform.rotation);

		}

        public void FindGround ()
		{
			StopCoroutine (FindGroundCoroutine());
			StartCoroutine (FindGroundCoroutine ());
		}

        private IEnumerator FindGroundCoroutine()
        {

			yield return null;

			// set y pos to high value, so that higher grounds can be loaded
			this.transform.SetY (150);

			Vector3 startingPos = this.transform.position;

			// wait for loader to finish, in case he didn't
			while (!Loader.HasLoaded)
				yield return null;

			// yield until you find ground beneath or above the player, or until timeout expires

			float timeStarted = Time.time;
			int numAttempts = 1;

			while (true) {
				
				if (Time.time - timeStarted > 4.0f) {
					// timeout expired
					Debug.LogWarning("Failed to find ground - timeout expired");
					yield break;
				}

				// maintain starting position
				this.transform.position = startingPos;
				this.Velocity = Vector3.zero;

				RaycastHit hit;
				float raycastDistance = 1000f;
				// raycast against all layers, except player
				int raycastLayerMask = ~ LayerMask.GetMask ("Player");

				Vector3[] raycastPositions = new Vector3[]{ this.transform.position, this.transform.position + Vector3.up * raycastDistance };	//transform.position - Vector3.up * characterController.height;
				Vector3[] raycastDirections = new Vector3[]{ Vector3.down, Vector3.down };
				string[] customMessages = new string[]{ "from center", "from above" };

				for (int i = 0; i < raycastPositions.Length; i++) {

					if (Physics.Raycast (raycastPositions[i], raycastDirections[i], out hit, raycastDistance, raycastLayerMask)) {
						// ray hit the ground
						// we can move there

						this.OnFoundGround (hit, numAttempts, customMessages [i]);

						yield break;
					}

				}


				numAttempts++;
				yield return null;
			}

        }

		private void OnFoundGround(RaycastHit hit, int numAttempts, string customMessage) {

			this.transform.position = hit.point + Vector3.up * characterController.height * 1.5f;
			this.Velocity = Vector3.zero;

			Debug.LogFormat ("Found ground at {0}, distance {1}, object name {2}, num attempts {3}, {4}", hit.point, hit.distance, 
				hit.transform.name, numAttempts, customMessage);

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

            if (!vehicle.IsNightToggled && WorldController.IsNight)
                vehicle.IsNightToggled = true;
            else if (vehicle.IsNightToggled && !WorldController.IsNight)
                vehicle.IsNightToggled = false;

            Debug.Log("IsNightToggled? "+vehicle.IsNightToggled);

            var seat = vehicle.GetSeat(seatAlignment);

            characterController.enabled = false;

            if (IsLocalPlayer)
            {
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

            if (IsLocalPlayer && seat.IsDriver)
            {
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

            if (IsLocalPlayer)
            {
                /*
                SendToServer(_lastPassengerState = new PlayerPassengerState {
                    Vechicle = null
                }, DeliveryMethod.ReliableOrdered, 1);
                */
            }
            else
            {
                //    _snapshots.Reset();
            }

            StartCoroutine(ExitVehicleAnimation(immediate));
        }

        private IEnumerator EnterVehicleAnimation(Vehicle.Seat seat, bool immediate)
        {
            var animIndex = seat.IsLeftHand ? AnimIndex.GetInLeft : AnimIndex.GetInRight;

            PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootEnd, new Vector3(-1, -1, -1));

            if (!immediate)
            {
                var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
                animState.wrapMode = WrapMode.Once;

                while (animState.enabled)
                {
                    yield return new WaitForEndOfFrame();
                }
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

        private IEnumerator ExitVehicleAnimation(bool immediate)
        {
            IsDrivingVehicle = false;
            IsInVehicleSeat = false;

            var seat = CurrentVehicle.GetSeat(_currentVehicleSeatAlignment);

            var animIndex = seat.IsLeftHand ? AnimIndex.GetOutLeft : AnimIndex.GetOutRight;

            PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootStart, new Vector3(-1, -1, -1));

            if (!immediate)
            {
                var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
                animState.wrapMode = WrapMode.Once;

                while (animState.enabled)
                    yield return new WaitForEndOfFrame();
            }

            PlayerModel.IsInVehicle = false;

            CurrentVehicle = null;
            _currentVehicleSeatAlignment = Vehicle.SeatAlignment.None;

            transform.localPosition = PlayerModel.VehicleParentOffset;
            transform.localRotation = Quaternion.identity;

            Camera.transform.SetParent(null, true);
            transform.SetParent(null);

            characterController.enabled = true;

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
            if (transform.position.y < -300)
            {
                Velocity = new Vector3(0, 0, 0);
                Transform spawn = GameObject.Find("Player Spawns").GetComponentsInChildren<Transform>()[1];
                transform.position = spawn.position;
                transform.rotation = spawn.rotation;
            }

			ConstrainPosition ();

			UpdateAnims ();

            if (IsInVehicle && IsDrivingVehicle)
                UpdateWheelTurning();
			
            //If player falls from the map
            if (IsGrounded && transform.position.y < -50)
            {
                Vector3 t = transform.position;
                transform.position = new Vector3(t.x, 150, t.z);
                FindGround();
            }

		//	IsWalking = IsRunning = false;

        }

		private void ConstrainPosition() {

			// Constrain to stay inside map

			if (transform.position.x < -3000)
			{
				var t = transform.position;
				t.x = -3000;
				transform.position = t;
			}
			if (transform.position.x > 3000)
			{
				var t = transform.position;
				t.x = 3000;
				transform.position = t;
			}
			if (transform.position.z < -3000)
			{
				var t = transform.position;
				t.z = -3000;
				transform.position = t;
			}
			if (transform.position.z > 3000)
			{
				var t = transform.position;
				t.z = 3000;
				transform.position = t;
			}

		}

		private void UpdateAnims() {

			if (!this.shouldPlayAnims)
				return;

			if (IsInVehicle || m_weaponHolder.IsHoldingWeapon)
				return;

			if (IsRunning) {
				
				PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Run, PlayMode.StopAll);

			} else if (IsWalking) {
				
				PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Walk, PlayMode.StopAll);

			} else if (IsSprinting) {

				PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.sprint_civi);

			} else {
				// player is standing
				PlayerModel.PlayAnim(AnimGroup.WalkCycle, AnimIndex.Idle, PlayMode.StopAll);

			}

		}

        private void FixedUpdate()
        {
            if (!Loader.HasLoaded)
                return;

            //    NetworkingFixedUpdate();

            if (IsInVehicle) return;


			// rotate player towards his heading
			Vector3 forward = Vector3.RotateTowards (this.transform.forward, Heading, TurnSpeed * Time.deltaTime, 0.0f);
			this.transform.rotation = Quaternion.LookRotation(forward);


            if (enableFlying || enableNoclip)
            {
                Heading = Vector3.Scale(Movement, new Vector3(1f, 0f, 1f)).normalized;
                Velocity = Movement * Time.fixedDeltaTime;
                if (enableNoclip)
                {
                    transform.position += Velocity;
                }
                else
                {
                    characterController.Move(Velocity);
                }
            }
            else if (IsLocalPlayer)
            {
				
                if (Movement.sqrMagnitude > float.Epsilon)
                {
					// change heading to match movement input
                    Heading = Vector3.Scale(Movement, new Vector3(1f, 0f, 1f)).normalized;
                }

				// change velocity based on movement input and current speed extracted from anim
                var vDiff = Movement * PlayerModel.Speed - new Vector3(Velocity.x, 0f, Velocity.z);
				Velocity += vDiff;

				// apply gravity
                Velocity = new Vector3(Velocity.x, characterController.isGrounded
                    ? 0f : Velocity.y - 9.81f * 2f * Time.fixedDeltaTime, Velocity.z);

                // Jump! But only if the jump button has been released and player has been grounded for a given number of frames
				if (!Input.GetKey(KeyCode.LeftShift))
                    jumpTimer++;
                else if (jumpTimer >= antiBunnyHopFactor)
                {
                    Velocity += Vector3.up * jumpSpeed;
                    jumpTimer = 0;
                }

				// finally, move the character
                characterController.Move(Velocity * Time.fixedDeltaTime);
            }
            else
            {
                Velocity = characterController.velocity;
            }
        }


		void OnDrawGizmosSelected ()
		{

			// draw heading ray

			Gizmos.color = Color.blue;
			Gizmos.DrawLine (this.transform.position, this.transform.position + this.Heading);

			// draw movement ray

			Gizmos.color = Color.green;
			Gizmos.DrawLine (this.transform.position, this.transform.position + this.Movement);

		}

    }
}