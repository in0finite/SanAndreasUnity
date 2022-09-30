using UnityEngine;
using UGameCore.Utilities;
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

		public virtual float TimeUntilStateCanBeSwitchedToOtherMovementState => PedManager.Instance.timeUntilMovementStateCanBeSwitchedToOtherMovementState;
		public virtual float TimeUntilStateCanBeEnteredFromOtherMovementState => PedManager.Instance.timeUntilMovementStateCanBeEnteredFromOtherMovementState;



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
            System.Type type = BaseMovementState.GetMovementStateToSwitchToBasedOnInput(m_ped);

			this.SwitchToMovementStateIfEnoughTimePassed(type);
		}

		public static bool EnoughTimePassedToSwitchBetweenMovementStates(
			BaseMovementState currentState,
			BaseMovementState targetState)
        {
			if (currentState.TimeSinceActivated < currentState.TimeUntilStateCanBeSwitchedToOtherMovementState)
				return false;

			if (targetState.TimeSinceDeactivated < targetState.TimeUntilStateCanBeEnteredFromOtherMovementState)
				return false;

			return true;
		}

		public bool SwitchToMovementStateIfEnoughTimePassed(System.Type type)
        {
			var state = (BaseMovementState)m_ped.GetStateOrLogError(type);

			if (!EnoughTimePassedToSwitchBetweenMovementStates(this, state))
				return false;

			m_ped.SwitchState(type);

			return true;
		}

		public static void SwitchToMovementStateBasedOnInput (Ped ped)
		{
            System.Type type = GetMovementStateToSwitchToBasedOnInput(ped);
			ped.SwitchState(type);
		}

		public static System.Type GetMovementStateToSwitchToBasedOnInput(Ped ped)
        {
			if (ped.IsJumpOn && ped.GetStateOrLogError<JumpState>().CanJump())
			{
				return typeof(JumpState);
			}
			else if (ped.IsWalkOn)
			{
				return typeof(WalkState);
			}
			else if (ped.IsRunOn)
			{
				return typeof(RunState);
			}
			else if (ped.IsSprintOn)
			{
				if (ped.CurrentWeapon != null && !ped.CurrentWeapon.CanSprintWithIt)
					return typeof(RunState);
				else
					return typeof(SprintState);
			}
			else if (ped.IsPanicButtonOn)
            {
				return typeof(PanicState);
			}
			else
			{
				return typeof(StandState);
			}
		}

		protected virtual void SwitchToAimState()
		{
			if (m_ped.IsAimOn && m_ped.IsHoldingWeapon && EnoughTimePassedToSwitchToAimState(m_ped))
			{
				BaseAimMovementState.SwitchToAimMovementStateBasedOnInput (m_ped);
			}
		}

		public static bool EnoughTimePassedToSwitchToAimState(Ped ped)
        {
			var aimStates = ped.CachedAimStates;

			for (int i = 0; i < aimStates.Count; i++)
            {
				if (Time.timeAsDouble - aimStates[i].LastTimeWhenDeactivated < PedManager.Instance.minTimeToReturnToAimState)
					return false;
            }

			return true;
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
			{
                var crouchState = m_ped.GetState<CrouchState>();
				if (BaseMovementState.EnoughTimePassedToSwitchBetweenMovementStates(this, crouchState))
					m_ped.SwitchState(crouchState.GetType());
			}
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

		public override void OnSurrenderButtonPressed()
		{
			if (m_isServer)
				this.SwitchToMovementStateIfEnoughTimePassed(typeof(SurrenderState));
			else
				base.OnSurrenderButtonPressed();
		}

	}

}
