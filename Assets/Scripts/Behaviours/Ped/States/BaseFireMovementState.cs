using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Weapons;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all movement-fire states.
	/// </summary>
	public abstract class BaseFireMovementState : BaseAimMovementState, IFireState
	{


		public override void UpdateState()
		{
			
			base.UpdateState ();

			if (!this.IsActiveState)
				return;

			if (!m_ped.IsFireOn)
			{
				// stop firing ?
				// - no, because fire anim may still be running
				// when anim gets finished, we'll stop firing

			}

		}


		protected override bool SwitchToNonAimMovementState ()
		{
			// we should not switch to non-aim state, but instead, when fire anim finishes, we will switch back to
			// aim state

			// but the current weapon may be destroyed, in which case we will not exit fire state, because aim anim will not be updated

			if (!m_ped.IsHoldingWeapon)
			{
				m_ped.SwitchState<StandState>();
				return true;
			}

			return false;
		}

		protected override bool SwitchToFiringState ()
		{
			// switch to other fire movement state
			SwitchToFireMovementStateBasedOnInput (m_ped);
			return ! this.IsActiveState;
		}

		protected override bool SwitchToOtherAimMovementState ()
		{
			// don't switch to aim states, because fire anim may still be running

			return ! this.IsActiveState;
		}


		public static void SwitchToFireMovementStateBasedOnInput (Ped ped)
		{

			if (ped.IsWalkOn)
			{
				ped.SwitchState<WalkFireState> ();
			}
			else if (ped.IsRunOn || ped.IsSprintOn)
			{
				if(ped.CurrentWeapon != null && ped.CurrentWeapon.HasFlag(GunFlag.AIMWITHARM))
					ped.SwitchState<RunFireState> ();
				else
					ped.SwitchState<WalkFireState> ();
			}
			else
			{
				ped.SwitchState<StandFireState> ();
			}

		}


		public override void StartFiring ()
		{
			// ignore

		}

		public virtual void StopFiring ()
		{
			BaseAimMovementState.SwitchToAimMovementStateBasedOnInput (m_ped);
		}


		public override void OnSubmitPressed()
		{
			// ignore

		}

		public override void OnJumpPressed()
		{
			// ignore

		}

	}

}
