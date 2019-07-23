using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all states that are scripts.
	/// </summary>
	public abstract class BaseScriptState : MonoBehaviour, IPedState
	{

		protected Ped m_ped;
		protected PedModel m_model { get { return m_ped.PlayerModel; } }
	//	protected StateMachine m_stateMachine;
		protected new Transform transform { get { return m_ped.transform; } }
		public bool IsActiveState { get { return m_ped.CurrentState == this; } }
		protected bool m_isServer { get { return Net.NetStatus.IsServer; } }
		protected bool m_isClientOnly => Net.NetStatus.IsClientOnly;
		protected bool m_shouldSendButtonEvents { get { return !m_isServer && m_ped.IsControlledByLocalPlayer; } }



		protected virtual void Awake ()
		{
			m_ped = this.GetComponentInParent<Ped> ();
		}

		protected virtual void OnEnable ()
		{
			
		}

		protected virtual void OnDisable ()
		{
			
		}

		protected virtual void Start ()
		{

		}

		public virtual void OnBecameActive ()
		{
			
		}

		public virtual void OnBecameInactive ()
		{
			
		}

		public virtual bool RepresentsState (System.Type type)
		{
			var myType = this.GetType ();
			return myType.Equals (type) || myType.IsSubclassOf (type);
		}

		public bool RepresentsState<T> () where T : IState
		{
			return this.RepresentsState (typeof(T));
		}

		public virtual void UpdateState() {

			this.ConstrainPosition();
			this.ConstrainRotation();
			
		}

		public virtual void PostUpdateState()
		{
			
		}

		public virtual void LateUpdateState()
		{

			if (m_ped.Camera)
				this.UpdateCamera ();
			
			if (m_ped.shouldPlayAnims)
				this.UpdateAnims ();
			
		}

		public virtual void FixedUpdateState()
		{

			this.UpdateHeading();
			this.UpdateRotation();
			this.UpdateMovement();

		}

		protected virtual void ConstrainPosition()
		{
			if (m_isServer)
				m_ped.ConstrainPosition();
		}

		protected virtual void ConstrainRotation ()
		{
			if (m_isServer)
				m_ped.ConstrainRotation();
		}

		protected virtual void UpdateHeading()
		{
			if (m_isServer)
				m_ped.UpdateHeading ();
		}

		protected virtual void UpdateRotation()
		{
			if (m_isServer)
				m_ped.UpdateRotation ();
		}

		protected virtual void UpdateMovement()
		{
			if (m_isServer)
				m_ped.UpdateMovement ();
		}

		public virtual void UpdateCamera()
		{
			this.RotateCamera();
			this.UpdateCameraZoom();
			this.CheckCameraCollision ();
		}

		public virtual void RotateCamera()
		{
			BaseScriptState.RotateCamera(m_ped, m_ped.MouseMoveInput, m_ped.CameraClampValue.y);
		}

		public static void RotateCamera(Ped ped, Vector2 mouseDelta, float xAxisClampValue)
		{
			Camera cam = ped.Camera;

			if (mouseDelta.sqrMagnitude < float.Epsilon)
				return;

		//	cam.transform.Rotate( new Vector3(-mouseDelta.y, mouseDelta.x, 0f), Space.World );
			var eulers = cam.transform.eulerAngles;
		//	eulers.z = 0f;
			eulers.x += - mouseDelta.y;
			eulers.y += mouseDelta.x;
			// adjust x
			if (eulers.x > 180f)
				eulers.x -= 360f;
			// clamp
			if (xAxisClampValue > 0)
				eulers.x = Mathf.Clamp(eulers.x, -xAxisClampValue, xAxisClampValue);

			cam.transform.rotation = Quaternion.AngleAxis(eulers.y, Vector3.up)
				* Quaternion.AngleAxis(eulers.x, Vector3.right);
			
		}

		public virtual Vector3 GetCameraFocusPos()
		{
			return m_ped.transform.position + Vector3.up * 0.5f;
		}

		public virtual float GetCameraDistance()
		{
			return m_ped.CameraDistance;
		}

		public virtual void UpdateCameraZoom()
		{
			m_ped.CameraDistance = Mathf.Clamp(m_ped.CameraDistance - m_ped.MouseScrollInput.y, 2.0f, 32.0f);
		}

		public virtual void CheckCameraCollision()
		{
			BaseScriptState.CheckCameraCollision (m_ped, this.GetCameraFocusPos (), -m_ped.Camera.transform.forward, 
				this.GetCameraDistance ());
		}

		public static void CheckCameraCollision(Ped ped, Vector3 castFrom, Vector3 castDir, float cameraDistance)
		{
			
			// cast a ray from ped to camera to see if it hits anything
			// if so, then move the camera to hit point

			Camera cam = ped.Camera;

			float distance = cameraDistance;
			var castRay = new Ray(castFrom, castDir);
			RaycastHit hitInfo;
			int ignoreLayer = (1 << MapObject.BreakableLayer) | (1 << Vehicles.Vehicle.Layer) | Ped.LayerMask;

			if (Physics.SphereCast(castRay, 0.25f, out hitInfo, distance, ~ ignoreLayer))
			{
				distance = hitInfo.distance;
			}

			cam.transform.position = castRay.GetPoint(distance);

		}

		protected virtual void UpdateAnims()
		{
			
		}


		public virtual void OnFireButtonPressed()
		{
			if (m_shouldSendButtonEvents)
				PedSync.Local.OnFireButtonPressed();
		}

		public virtual void OnAimButtonPressed()
		{
			if (m_shouldSendButtonEvents)
				PedSync.Local.OnAimButtonPressed();
		}

		public virtual void OnSubmitPressed()
		{
			if (m_shouldSendButtonEvents)
				PedSync.Local.OnSubmitButtonPressed();
		}

		public virtual void OnJumpPressed()
		{

		}

		public virtual void OnCrouchButtonPressed()
		{
			if (m_shouldSendButtonEvents)
				PedSync.Local.OnCrouchButtonPressed();
		}

		public virtual void OnNextWeaponButtonPressed()
		{
			if (m_isServer)
				m_ped.WeaponHolder.SwitchWeapon (true);
			else if (m_shouldSendButtonEvents)
				PedSync.Local.OnNextWeaponButtonPressed();
		}

		public virtual void OnPreviousWeaponButtonPressed()
		{
			if (m_isServer)
				m_ped.WeaponHolder.SwitchWeapon (false);
			else if (m_shouldSendButtonEvents)
				PedSync.Local.OnPreviousWeaponButtonPressed();
		}

		public virtual void OnFlyButtonPressed()
		{

		}

		public virtual void OnFlyThroughButtonPressed()
		{

		}

		public virtual void OnDamaged(DamageInfo info)
		{

		}


		public virtual void OnDrawHUD()
		{
			if (!UIManager.Instance.UseTouchInput || !GameManager.CanPlayerReadInput())
			{
				// we are not using touch input, or we should not read input right now
				// make sure that custom input is resetted
				this.ResetCustomInput();
				return;
			}

			if (Event.current.type == EventType.Repaint)	// repaint event is sent once per frame, when drawing
			{
				// reset input only during repaint event
				this.ResetCustomInput();

				// ignore mouse buttons when touch is enabled
				CustomInput.Instance.SetButton("LeftClick", false);
				if (!CustomInput.Instance.HasButton("RightClick"))
					CustomInput.Instance.SetButton("RightClick", false);
				CustomInput.Instance.SetButtonDown("LeftClick", false);
				CustomInput.Instance.SetButtonDown("RightClick", false);

			}

			// left side: movement buttons: arrows
			// right side: action buttons: crouch, enter, fly, toggle sprint/walk, jump (repeat button), toggle aim

			this.DrawMovementTouchInput();
			this.DrawActionsTouchInput();

		}

		protected virtual void ResetCustomInput()
		{
			var customInput = CustomInput.Instance;

			if (!UIManager.Instance.UseTouchInput)
			{
				// touch input is not used
				customInput.ResetAllInput();
				return;
			}

			// preserve input for: walk, sprint, aim

			bool isWalkOn = customInput.GetButtonNoDefaultInput("Walk");
			bool isSprintOn = customInput.GetButtonNoDefaultInput("Sprint");
			bool isAimOn = customInput.GetButtonNoDefaultInput("RightClick");

			customInput.ResetAllInput();

			customInput.SetButton("Walk", isWalkOn);
			customInput.SetButton("Sprint", isSprintOn);
			customInput.SetButton("RightClick", isAimOn);
		}

		protected virtual void DrawMovementTouchInput()
		{
			// movement buttons

			float height = Screen.height * 0.4f;
			float bottomMargin = Screen.height * 0.05f;
			float horizontalMargin = bottomMargin;

			// we'll need 3 rows of buttons: up, left & right, down
			float buttonHeight = height / 3;
			float buttonWidth = buttonHeight;

			CustomInput customInput = CustomInput.Instance;

			float movementVertical = 0f, movementHorizontal = 0f;

			float topY = Screen.height - bottomMargin - buttonHeight;
			if (GUI.RepeatButton(new Rect(horizontalMargin + buttonWidth, topY, buttonWidth, buttonHeight), UI.HUD.DownArrowTexture))
				movementVertical -= 1f;
			topY -= buttonHeight;
			if (GUI.RepeatButton(new Rect(horizontalMargin, topY, buttonWidth, buttonHeight), UI.HUD.LeftArrowTexture))
				movementHorizontal -= 1f;
			if (GUI.RepeatButton(new Rect(horizontalMargin + buttonWidth * 2, topY, buttonWidth, buttonHeight), UI.HUD.RightArrowTexture))
				movementHorizontal += 1f;
			topY -= buttonHeight;
			if (GUI.RepeatButton(new Rect(horizontalMargin + buttonWidth, topY, buttonWidth, buttonHeight), UI.HUD.UpArrowTexture))
				movementVertical += 1f;

			// set input for vertical and horizontal axis
			customInput.SetAxis("Vertical", movementVertical);
			customInput.SetAxis("Horizontal", movementHorizontal);

		}

		protected virtual void DrawActionsTouchInput()
		{
			// it's on the right side
			// create buttons from bottom to top

			CustomInput customInput = CustomInput.Instance;

			float buttonHeight = Screen.height / 5f * 0.6f;
			float buttonWidth = buttonHeight;

			float bottomMargin = Screen.height * 0.05f;
			float horizontalMargin = bottomMargin;

			float xPos = Screen.width - horizontalMargin - buttonWidth;
			float originalXPos = xPos;
			float horizontalSpace = 5f;

			// sprint/walk toggle button

			bool isWalkOn = customInput.GetButton("Walk");	// preserve current value
			bool isSprintOn = customInput.GetButton("Sprint");	// preserve current value

			float topY = Screen.height - bottomMargin - buttonHeight;
			GUI.contentColor = isWalkOn ? Color.blue : Color.white;
			if (GUI.Button(new Rect(xPos, topY, buttonWidth, buttonHeight), "Walk"))
			{
				isWalkOn = !isWalkOn;
			}

			//topY -= buttonHeight;
			xPos -= buttonWidth + horizontalSpace;
			GUI.contentColor = isSprintOn ? Color.blue : Color.white;
			if (GUI.Button(new Rect(xPos, topY, buttonWidth, buttonHeight), "Sprint"))
			{
				isSprintOn = !isSprintOn;
			}
			GUI.contentColor = Color.white;
			xPos = originalXPos;

			// assign input
			customInput.SetButton("Walk", isWalkOn);
			customInput.SetButton("Sprint", isSprintOn);

			// jump - repeat button

			bool isJumpOn = false;
			topY -= buttonHeight;
			GUI.contentColor = m_ped.IsJumpOn ? Color.blue : Color.white;
			if (GUI.RepeatButton(new Rect(xPos, topY, buttonWidth, buttonHeight), "Jump"))
			{
				isJumpOn = true;
			}
			GUI.contentColor = Color.white;

			customInput.SetButton("Jump", isJumpOn);

			// crouch
			//topY -= buttonHeight;
			xPos -= buttonWidth + horizontalSpace;
			if (GUI.Button(new Rect(xPos, topY, buttonWidth, buttonHeight), "Crouch"))
			{
				customInput.SetKeyDown(KeyCode.C, true);
			}
			xPos = originalXPos;

			// enter
			topY -= buttonHeight;
			if (GUI.Button(new Rect(xPos, topY, buttonWidth, buttonHeight), "Enter"))
			{
				customInput.SetButtonDown("Use", true);
			}

			// aim
			bool isAimOn = customInput.GetButton("RightClick");	// preserve current value
			topY -= buttonHeight;
			GUI.contentColor = isAimOn ? Color.blue : Color.white;
			if (GUI.Button(new Rect(xPos, topY, buttonWidth, buttonHeight), "Aim"))
			{
				isAimOn = !isAimOn;
			}
			GUI.contentColor = Color.white;

			customInput.SetButton("RightClick", isAimOn);

			// fire - repeat button
			bool isFireOn = false;
			xPos -= buttonWidth + horizontalSpace;
			GUI.contentColor = m_ped.IsFireOn ? Color.blue : Color.white;
			if (GUI.RepeatButton(new Rect(xPos, topY, buttonWidth, buttonHeight), "Fire"))
			{
				isFireOn = true;
			}
			GUI.contentColor = Color.white;
			xPos = originalXPos;

			customInput.SetButton("LeftClick", isFireOn);

			// fly
			topY -= buttonHeight;
			if (GUI.Button(new Rect(xPos, topY, buttonWidth, buttonHeight), "Fly"))
			{
				customInput.SetKeyDown(KeyCode.T, true);
			}


		}


		public virtual void OnSwitchedStateByServer(byte[] data)
		{
			m_ped.SwitchState(this.GetType());
		}

		public virtual byte[] GetAdditionalNetworkData()
		{
			return null;
		}

		public virtual void OnChangedWeaponByServer(int newSlot)
		{
			m_ped.WeaponHolder.SwitchWeapon(newSlot);
		}

		public virtual void OnWeaponFiredFromServer(Weapon weapon, Vector3 firePos)
		{
			// if (m_ped.IsControlledByLocalPlayer)
			// 	return;

			// update gun flash
			if (weapon.GunFlash != null)
				weapon.GunFlash.gameObject.SetActive (true);
			weapon.UpdateGunFlashRotation ();

            weapon.PlayFireSound();
		}

	}

}
