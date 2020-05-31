using UnityEngine;
using SanAndreasUnity.Utilities;
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

			// TODO: but, watch out, weapon may become null - if someone else destroys a weapon or changes current weapon
			// to Hand, then weapon will be null - we may never exit fire state, because aim anim will not be updated

			// simply checking if weapon is null and exiting the state should solve the problem

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
			else if (ped.IsRunOn)
			{
				if(ped.CurrentWeapon != null && ped.CurrentWeapon.HasFlag(GunFlag.AIMWITHARM))
					ped.SwitchState<RunFireState> ();
				else
					ped.SwitchState<WalkFireState> ();
			}
			else if (ped.IsSprintOn)
			{
				ped.SwitchState<StandFireState> ();
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
