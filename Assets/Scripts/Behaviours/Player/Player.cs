using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

        private int jumpTimer;

        #endregion Private Fields

        #region Inspector Fields

        public Camera Camera;
        public Pedestrian PlayerModel;

        public float TurnSpeed = 10f;

        public Weapon[] weapons = new Weapon[(int)WeaponSlot.Count];
        public int currentWeaponSlot = -1;
        public bool autoAddWeapon = false;

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

        public Vector3 Velocity { get; private set; }
        public Vector3 Movement { get; set; }

        public Vector3 Heading { get; private set; }
        public Vehicle CurrentVehicle { get; private set; }

        public bool IsInVehicle { get { return CurrentVehicle != null; } }
        public bool IsInVehicleSeat { get; private set; }
        public bool IsDrivingVehicle { get; private set; }

        private Vehicle.SeatAlignment _currentVehicleSeatAlignment;

        private static bool makeGPUAdjustments;

        #endregion Properties

        public static Player me;

        //     protected override void OnAwake()
        protected void Awake()
        {
            //    base.OnAwake();

            me = this;

            characterController = GetComponent<CharacterController>();

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

            if (!IsGrounded)
                // Find the ground (instead of falling)
                FindGround(false);
        }

        public void FindGround(bool debug = false)
        {
            StartCoroutine(_FindGround(debug));
        }

        private IEnumerator _FindGround(bool debug = false)
        {
            //First, we have to go to an operative range

            if (transform.position.y > 150)
            {
                Vector3 t = transform.position;
                transform.position = new Vector3(t.x, 150, t.z);
            }

            // Review: Maybe this can cause a bug due to CPU perfomance
            yield return new WaitForSeconds(1);

            //Then, when evetrything is loaded, then...

            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position - Vector3.up * characterController.height, Vector3.down), out hit))
            {
                if (!debug)
                    transform.position = hit.point + Vector3.up * characterController.height / 2;
                else
                    Debug.LogFormat("Ground found at {0}!", hit.point);
            }
            else if (debug)
                Debug.Log("Nothing found yet!!");
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

            if (IsInVehicle && IsDrivingVehicle)
                UpdateWheelTurning();

            // switch weapons - does not work
			if (!IsInVehicle && (null == Console.Instance || !Console.Instance.IsOpened) && !MiniMap.toggleMap)
            {
                if (Input.mouseScrollDelta.y != 0)
                {
                    if (currentWeaponSlot < 0)
                        currentWeaponSlot = 0;

                    for (int i = currentWeaponSlot + (int)Mathf.Sign(Input.mouseScrollDelta.y), count = 0;
                        i != currentWeaponSlot && count < (int)WeaponSlot.Count;
                        i += (int)Mathf.Sign(Input.mouseScrollDelta.y), count++)
                    {
                        if (i < 0)
                            i = weapons.Length - 1;
                        if (i >= weapons.Length)
                            i = 0;

                        if (weapons[i] != null)
                        {
                            SwitchWeapon(i);
                            break;
                        }
                    }
                }
            }

            // add weapons to player if he doesn't have any
            if (autoAddWeapon && null == System.Array.Find(weapons, w => w != null))
            {
                // player has no weapons

                weapons[(int)WeaponSlot.Machine] = Weapon.Load(355);
                SwitchWeapon((int)WeaponSlot.Machine);
            }

            //If player falls from the map
            if (IsGrounded && transform.position.y < -50)
            {
                Vector3 t = transform.position;
                transform.position = new Vector3(t.x, 150, t.z);
                FindGround();
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
                    Heading = Vector3.Scale(Movement, new Vector3(1f, 0f, 1f)).normalized;
                }

                var vDiff = Movement * PlayerModel.Speed - new Vector3(Velocity.x, 0f, Velocity.z);

                Velocity += vDiff;
                Velocity = new Vector3(Velocity.x, characterController.isGrounded
                    ? 0f : Velocity.y - 9.81f * 2f * Time.fixedDeltaTime, Velocity.z);

                // Jump! But only if the jump button has been released and player has been grounded for a given number of frames
                if (!Input.GetButton("Jump"))
                    jumpTimer++;
                else if (jumpTimer >= antiBunnyHopFactor)
                {
                    Velocity += Vector3.up * jumpSpeed;
                    jumpTimer = 0;
                }

                characterController.Move(Velocity * Time.fixedDeltaTime);
            }
            else
            {
                Velocity = characterController.velocity;
            }
        }

        private void SwitchWeapon(int slotIndex)
        {
            if (PlayerModel.weapon != null)
            {
                // set parent to weapons container in order to hide it
                //	PlayerModel.weapon.SetParent (Weapon.weaponsContainer.transform);

                PlayerModel.weapon.gameObject.SetActive(false);
            }

            PlayerModel.weapon = weapons[slotIndex].gameObject.transform;
            // change parent to make it visible
            //	PlayerModel.weapon.SetParent(this.transform);
            PlayerModel.weapon.gameObject.SetActive(true);

            currentWeaponSlot = slotIndex;
        }
    }
}