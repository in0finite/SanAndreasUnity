using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all movement-aim states.
	/// </summary>
	public class BaseAimState : DefaultState
	{


		public override void UpdateState()
		{

			base.UpdateState ();

			if (!this.IsActiveState)
				return;

			// TODO: check:
			// - if we should switch to any of non-aim movement states
			// - if we should enter any of firing states
			// - if we should enter any of other aim movement states
			// - if we should enter falling state

			if (this.SwitchToNonAimMovementState ())
				return;
			if (this.SwitchToFiringState ())
				return;
			if (this.SwitchToOtherAimMovementState ())
				return;
			if (this.SwitchToFallingState ())
				return;

		}

		protected virtual bool SwitchToNonAimMovementState ()
		{
			// check if we should exit aiming state
			if (!m_ped.WeaponHolder.IsHoldingWeapon || !m_ped.WeaponHolder.IsAimOn)
			{
				Debug.LogFormat ("Exiting aim state, IsHoldingWeapon {0}, IsAimOn {1}", m_ped.WeaponHolder.IsHoldingWeapon, m_ped.WeaponHolder.IsAimOn);
				BaseMovementState.SwitchToMovementStateBasedOnInput (m_ped);
				return true;
			}
			return false;
		}

		protected virtual bool SwitchToFiringState ()
		{
			return false;
		}

		protected virtual bool SwitchToOtherAimMovementState ()
		{
			BaseAimState.SwitchToAimMovementStateBasedOnInput (m_ped);
			return ! this.IsActiveState;
		}

		protected virtual bool SwitchToFallingState ()
		{
			return false;
		}


		public static void SwitchToAimMovementStateBasedOnInput (Ped ped)
		{

			if (ped.IsWalkOn)
			{
				ped.SwitchState<WalkAimState> ();
			}
			else if (ped.IsRunOn)
			{
				ped.SwitchState<RunAimState> ();
			}
			else if (ped.IsSprintOn)
			{
				ped.SwitchState<StandAimState> ();
			}
			else
			{
				ped.SwitchState<StandAimState> ();
			}

		}


		public override void OnSubmitPressed()
		{

			// try to enter vehicle
			m_ped.TryEnterVehicleInRange ();

		}

		public override void OnJumpPressed()
		{
			// ignore

		}

	}

}
