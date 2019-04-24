using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
	[DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController))]
    public partial class Ped : 
		#if MIRROR
		Mirror.NetworkBehaviour
		#else
		MonoBehaviour
		#endif
    {
        #region Private Fields

		private static List<Ped> s_allPeds = new List<Ped> ();
		public static Ped[] AllPeds { get { return s_allPeds.ToArray (); } }

		private WeaponHolder m_weaponHolder;
		public WeaponHolder WeaponHolder { get { return m_weaponHolder; } }

		private StateMachine m_stateMachine = new StateMachine ();

        #endregion Private Fields

        #region Inspector Fields

        public Camera Camera { get { return this == Ped.Instance ? Camera.main : null; } }
        public PedModel PlayerModel;

		public bool shouldPlayAnims = true;

        public float TurnSpeed = 10f;

        public CharacterController characterController;

        public float jumpSpeed = 8.0f;

		[SerializeField] private float m_cameraDistance = 3f;
		public float CameraDistance { get { return m_cameraDistance; } set { m_cameraDistance = value; } }

		[SerializeField] private float m_cameraDistanceVehicle = 6f;
		public float CameraDistanceVehicle { get { return m_cameraDistanceVehicle; } set { m_cameraDistanceVehicle = value; } }

		[SerializeField] private Vector2 m_cameraClampValue = new Vector2(60, 60);
		public Vector2 CameraClampValue { get { return m_cameraClampValue; } set { m_cameraClampValue = value; } }

        #endregion Inspector Fields

        #region Properties

		public Peds.States.BaseScriptState[] States { get; private set; }
		public Peds.States.BaseScriptState CurrentState { get { return (Peds.States.BaseScriptState) m_stateMachine.CurrentState; } }

        public Cell Cell { get { return Cell.Instance; } }

		public UnityEngine.Animation AnimComponent { get { return PlayerModel.AnimComponent; } }

		public SanAndreasUnity.Importing.Items.Definitions.PedestrianDef PedDef { get { return this.PlayerModel.Definition; } }

		public static int RandomPedId {
			get {
				int count = Ped.SpawnablePedDefs.Count ();
				if (count < 1)
					throw new System.Exception ("No ped definitions found");

				int index = Random.Range (0, count);
				return Ped.SpawnablePedDefs.ElementAt (index).Id;
			}
		}

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

		public Vector2 MouseMoveInput { get; set; }
		public Vector2 MouseScrollInput { get; set; }

		public bool IsWalkOn { get; set; }
		public bool IsRunOn { get; set; }
		public bool IsSprintOn { get; set; }
		public bool IsJumpOn { get; set; }

        public Vector3 Velocity { get; private set; }
		/// <summary> Current movement input. </summary>
        public Vector3 Movement { get; set; }
		/// <summary> Direction towards which the player turns. </summary>
        public Vector3 Heading { get; set; }
        
		public bool IsAiming { get { return m_weaponHolder.IsAiming; } }
		public Weapon CurrentWeapon { get { return m_weaponHolder.CurrentWeapon; } }
		public bool IsFiring { get { return m_weaponHolder.IsFiring; } }
		public Vector3 AimDirection { get { return m_weaponHolder.AimDirection; } }
		public bool IsAimOn { get ; set ; }
		public bool IsFireOn { get ; set ; }
		public bool IsHoldingWeapon { get { return m_weaponHolder.IsHoldingWeapon; } }

        private static bool makeGPUAdjustments;

		private Coroutine m_findGroundCoroutine;

        #endregion Properties


		public	static	Ped	Instance { get ; private set ; }

		/// <summary>Position of player instance.</summary>
		public	static	Vector3	InstancePos { get { return Instance.transform.position; } }

		public bool IsLocalPlayer { get; private set; }



        
        void Awake()
        {
            
			if (null == Instance) {
				Instance = this;
				IsLocalPlayer = true;
			}

            characterController = GetComponent<CharacterController>();
			m_weaponHolder = GetComponent<WeaponHolder> ();

			this.States = this.GetComponentsInChildren<Peds.States.BaseScriptState> ();

			this.AwakeForDamage ();

        }

        void Start()
        {
            //MySetupLocalPlayer ();

			this.StartForDamage ();

			if (null == this.CurrentState)
				this.SwitchState<Peds.States.StandState> ();

        }

		void OnEnable ()
		{
			s_allPeds.Add (this);
		}

		void OnDisable ()
		{
			s_allPeds.Remove (this);
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


		public T GetState<T>() where T : Peds.States.BaseScriptState
		{
			var type = typeof(T);
			return (T) this.States.FirstOrDefault (s => s.GetType ().Equals (type));
		}

		public T GetStateOrLogError<T>() where T : Peds.States.BaseScriptState
		{
			var state = this.GetState<T> ();
			if(null == state)
				Debug.LogErrorFormat ("Failed to find state: {0}", typeof(T));
			return state;
		}

		public void SwitchState<T>() where T : Peds.States.BaseScriptState {
			
			var state = this.GetStateOrLogError<T> ();
			if (null == state)
				return;
			
		//	IState oldState = this.CurrentState;

			m_stateMachine.SwitchState (state);

//			if (oldState != state)
//			{
//				Debug.LogFormat ("Switched to state: {0}", state.GetType ().Name);
//			}
			
		}


        public void OnSpawn()
        {
            // Note: Spawn is performed here.

			if (!IsGrounded) {
				// Find the ground (instead of falling)
				FindGround ();
			}

        }

		public void Teleport(Vector3 position, Quaternion rotation) {

			if (this.IsInVehicle)
				return;

			this.transform.position = position;
			this.transform.rotation = rotation;

			this.FindGround ();

		}

		public void Teleport(Vector3 position) {

			this.Teleport (position, this.transform.rotation);

		}

        public void FindGround ()
		{
			if (m_findGroundCoroutine != null) {
				StopCoroutine (m_findGroundCoroutine);
				m_findGroundCoroutine = null;
			}

			m_findGroundCoroutine = StartCoroutine (FindGroundCoroutine ());
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

        
        private void Update()
        {
            if (!Loader.HasLoaded)
                return;

			if (this.CurrentState != null)
			{
				this.CurrentState.UpdateState ();
			}

            // Reset to a valid (and solid!) start position when falling below the world
            if (transform.position.y < -300)
            {
                Velocity = new Vector3(0, 0, 0);
                Transform spawn = GameObject.Find("Player Spawns").GetComponentsInChildren<Transform>()[1];
                transform.position = spawn.position;
                transform.rotation = spawn.rotation;
            }

		//	ConstrainPosition ();

		//	ConstrainRotation ();

		//	UpdateAnims ();

            //if (IsDrivingVehicle)
            //    UpdateWheelTurning();
			
            //If player falls from the map
            if (IsGrounded && transform.position.y < -50)
            {
                Vector3 t = transform.position;
                transform.position = new Vector3(t.x, 150, t.z);
                FindGround();
            }

			this.UpdateDamageStuff ();

			this.Update_Net();

		//	IsWalking = IsRunning = false;

			if (this.CurrentState != null)
				this.CurrentState.PostUpdateState();

        }

		void LateUpdate ()
		{

			if (this.CurrentState != null)
			{
				this.CurrentState.LateUpdateState ();
			}

		}

		public void ConstrainPosition() {

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

		public void ConstrainRotation ()
		{
			if (IsInVehicle)
				return;

			// ped can only rotate around Y axis

			Vector3 eulers = this.transform.eulerAngles;
			if (eulers.x != 0f || eulers.z != 0f) {
				eulers.x = 0f;
				eulers.z = 0f;
				this.transform.eulerAngles = eulers;
			}

		}

		private void UpdateAnims() {

			if (!this.shouldPlayAnims)
				return;

			if (IsInVehicle || m_weaponHolder.IsHoldingWeapon)
				return;

			if (IsRunOn) {
				
				PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Run, PlayMode.StopAll);

			} else if (IsWalkOn) {
				
				PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Walk, PlayMode.StopAll);

			} else if (IsSprintOn) {

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

			if (this.CurrentState != null)
			{
				this.CurrentState.FixedUpdateState ();
			}

        }

		public void UpdateHeading()
		{
			
			if (this.IsAiming && this.CurrentWeapon != null && !this.CurrentWeapon.CanTurnInDirectionOtherThanAiming) {
				// ped heading can only be the same as ped direction
				this.Heading = this.WeaponHolder.AimDirection;
			}

			// player can look only along X and Z axis
			this.Heading = this.Heading.WithXAndZ ().normalized;

		}

		public void UpdateRotation()
		{

			// rotate player towards his heading
			Vector3 forward = Vector3.RotateTowards (this.transform.forward, Heading, TurnSpeed * Time.deltaTime, 0.0f);
			this.transform.rotation = Quaternion.LookRotation(forward);

		}

		public void UpdateMovement()
		{
			

			// movement can only be done on X and Z axis
			this.Movement = this.Movement.WithXAndZ ();

			// change heading to match movement input
			//if (Movement.sqrMagnitude > float.Epsilon)
			//{
			//	Heading = Vector3.Scale(Movement, new Vector3(1f, 0f, 1f)).normalized;
			//}

			// change velocity based on movement input and current speed extracted from anim

			float modelVel = Mathf.Abs( PlayerModel.Velocity [PlayerModel.VelocityAxis] );
			//Vector3 localMovement = this.transform.InverseTransformDirection (this.Movement);
			//Vector3 globalMovement = this.transform.TransformDirection( Vector3.Scale( localMovement, modelVel ) );

			Vector3 vDiff = this.Movement * modelVel - new Vector3(Velocity.x, 0f, Velocity.z);
			Velocity += vDiff;

			// apply gravity
			Velocity = new Vector3(Velocity.x, characterController.isGrounded
				? 0f : Velocity.y - (-Physics.gravity.y) * 2f * Time.fixedDeltaTime, Velocity.z);


			// finally, move the character
			characterController.Move(Velocity * Time.fixedDeltaTime);
		

//			if(!IsLocalPlayer)
//            {
//                Velocity = characterController.velocity;
//            }

		}


		public void StartFiring ()
		{
			if (!this.IsAiming)
				return;

			((Peds.States.IAimState)this.CurrentState).StartFiring();
		}

		public void StopFiring ()
		{
			if (!this.IsFiring)
				return;

			((Peds.States.IFireState)this.CurrentState).StopFiring();
		}


		public void ResetInput ()
		{
			this.ResetMovementInput ();
			this.MouseMoveInput = Vector2.zero;
			this.MouseScrollInput = Vector2.zero;
			this.IsAimOn = this.IsFireOn = false;
		}

		public void ResetMovementInput ()
		{
			this.IsWalkOn = this.IsRunOn = this.IsSprintOn = false;
			this.Movement = Vector3.zero;
			this.IsJumpOn = false;
		}

		public void OnFireButtonPressed ()
		{
			if (this.CurrentState != null)
				this.CurrentState.OnFireButtonPressed ();
		}

		public void OnAimButtonPressed ()
		{
			if (this.CurrentState != null)
				this.CurrentState.OnAimButtonPressed ();
		}

		public void OnSubmitPressed ()
		{
			if (this.CurrentState != null)
			{
				this.CurrentState.OnSubmitPressed ();
			}
		}

		public void OnJumpButtonPressed ()
		{
			if (this.CurrentState != null)
				this.CurrentState.OnJumpPressed ();
		}

		public void OnCrouchButtonPressed ()
		{
			if (this.CurrentState != null)
				this.CurrentState.OnCrouchButtonPressed ();
		}

		public void OnNextWeaponButtonPressed ()
		{
			if (this.CurrentState != null)
				this.CurrentState.OnNextWeaponButtonPressed ();
		}

		public void OnPreviousWeaponButtonPressed ()
		{
			if (this.CurrentState != null)
				this.CurrentState.OnPreviousWeaponButtonPressed ();
		}

		public void OnFlyButtonPressed ()
		{
			if (this.CurrentState != null)
				this.CurrentState.OnFlyButtonPressed ();
		}

		public void OnFlyThroughButtonPressed ()
		{
			if (this.CurrentState != null)
				this.CurrentState.OnFlyThroughButtonPressed ();
		}


		void OnGUI ()
		{
			if (!Loader.HasLoaded)
				return;


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