using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all movement states.
	/// </summary>
	public abstract class BaseMovementState : BaseScriptState
	{
		public abstract AnimId movementAnim { get; }
		public abstract AnimId movementWeaponAnim { get; }



		public override void UpdateState() {

			base.UpdateState ();

			if (!this.IsActiveState)
				return;


			if (m_isServer)
				this.SwitchToMovementState ();

			if (!this.IsActiveState)
				return;

			if (m_isServer)
				this.SwitchToAimState ();

		}

		protected virtual void SwitchToMovementState()
		{
			BaseMovementState.SwitchToMovementStateBasedOnInput (m_ped);
		}

		public static void SwitchToMovementStateBasedOnInput (Ped ped)
		{

			if (ped.IsJumpOn && ped.GetStateOrLogError<JumpState>().CanJump())
			{
				ped.GetState<JumpState>().Jump();
			}
			else if (ped.IsWalkOn)
			{
				ped.SwitchState<WalkState> ();
			}
			else if (ped.IsRunOn)
			{
				ped.SwitchState<RunState> ();
			}
			else if (ped.IsSprintOn)
			{
				if (ped.CurrentWeapon != null && !ped.CurrentWeapon.CanSprintWithIt)
					ped.SwitchState<StandState> ();
				else
					ped.SwitchState<SprintState> ();
			}
			else
			{
				ped.SwitchState<StandState> ();
			}

		}

		protected virtual void SwitchToAimState()
		{
			if (m_ped.IsAimOn && m_ped.IsHoldingWeapon)
			{
				BaseAimMovementState.SwitchToAimMovementStateBasedOnInput (m_ped);
			}
		}

		protected override void UpdateAnims ()
		{
			if (m_ped.CurrentWeapon != null)
			{
				m_ped.PlayerModel.PlayAnim (this.movementWeaponAnim);
			}
			else
			{
				m_ped.PlayerModel.PlayAnim (this.movementAnim);
			}
		}

		public override void OnSubmitPressed() {

			// try to enter vehicle
			if (m_isServer)
				m_ped.TryEnterVehicleInRange ();
			else
				base.OnSubmitPressed();

		}

		public override void OnCrouchButtonPressed ()
		{
			if (m_isServer)
				m_ped.SwitchState<CrouchState>();
			else
				base.OnCrouchButtonPressed();
		}

		public override void OnFlyButtonPressed ()
		{
			if (m_isServer)
				m_ped.GetStateOrLogError<FlyState> ().EnterState (false);
		}

		public override void OnFlyThroughButtonPressed ()
		{
			if (m_isServer)
				m_ped.GetStateOrLogError<FlyState> ().EnterState (true);
		}


		public override void OnDrawHUD()
		{
			base.OnDrawHUD();

			if (!UIManager.Instance.UseTouchInput || !GameManager.CanPlayerReadInput())
				return;

			// left side: movement buttons: arrows
			// right side: action buttons: crouch, enter, fly, toggle sprint/walk, jump (repeat button), toggle aim

			this.DrawMovementTouchInput();
			this.DrawActionsTouchInput();

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
			if (GUI.RepeatButton(new Rect(horizontalMargin + buttonWidth, topY, buttonWidth, buttonHeight), "DOWN"))
				movementVertical -= 1f;
			topY -= buttonHeight;
			if (GUI.RepeatButton(new Rect(horizontalMargin, topY, buttonWidth, buttonHeight), "LEFT"))
				movementHorizontal -= 1f;
			if (GUI.RepeatButton(new Rect(horizontalMargin + buttonWidth * 2, topY, buttonWidth, buttonHeight), "RIGHT"))
				movementHorizontal += 1f;
			topY -= buttonHeight;
			if (GUI.RepeatButton(new Rect(horizontalMargin + buttonWidth, topY, buttonWidth, buttonHeight), "UP"))
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

			bool isWalkOn = m_ped.IsWalkOn;	// preserve current value
			bool isSprintOn = m_ped.IsSprintOn;	// preserve current value

			float topY = Screen.height - bottomMargin - buttonHeight;
			GUI.contentColor = m_ped.IsWalkOn ? Color.blue : Color.white;
			if (GUI.Button(new Rect(xPos, topY, buttonWidth, buttonHeight), "Walk"))
			{
				isWalkOn = !isWalkOn;
			}

			//topY -= buttonHeight;
			xPos -= buttonWidth + horizontalSpace;
			GUI.contentColor = m_ped.IsSprintOn ? Color.blue : Color.white;
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
			bool isAimOn = m_ped.IsAimOn;	// preserve current value
			topY -= buttonHeight;
			GUI.contentColor = m_ped.IsAimOn ? Color.blue : Color.white;
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

	}

}
