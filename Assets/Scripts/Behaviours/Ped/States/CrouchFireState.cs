using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchFireState : CrouchAimState, IFireState
	{
		// not used
		public override AnimId aimWithArm_LowerAnim { get { throw new System.InvalidOperationException(); } }

		// override aim anim timings
		public override float AimAnimMaxTime { get { return m_weapon.CrouchAimAnimMaxTime; } }
		public override float AimAnimFireMaxTime { get { return m_weapon.CrouchAimAnimFireMaxTime; } }



		protected override bool SwitchToFiringState ()
		{
			// there are no other fire movement states to switch to
			return false;
		}

		protected override bool SwitchToOtherAimMovementState ()
		{
			// we'll switch to aim state when fire anim finishes
			return false;
		}

		protected override void RotateSpine ()
		{
			// TODO:
		}

		public override void StartFiring ()
		{
			// ignore
		}

		public virtual void StopFiring ()
		{
			// switch to crouch-aim state
			m_ped.SwitchState<CrouchAimState>();
		}
		/*
		protected override void UpdateAnims ()
		{
			base.UpdateAnims();

			if( !this.IsActiveState )
				return;

			// anim does not set correct velocity
			// set it to zero to make the ped stand in place
			m_model.RootFrame.LocalVelocity = Vector3.zero;
		}

		protected override AnimationState UpdateAnimsNonAWA ()
		{
			return this.UpdateAnimsAll();
		}

		protected override AnimationState UpdateAnimsAWA ()
		{
			return this.UpdateAnimsAll();
		}

		protected virtual AnimationState UpdateAnimsAll()
		{
			var state = m_model.PlayAnim(m_weapon.CrouchAimAnim);
			state.wrapMode = WrapMode.ClampForever;
			return state;
		}

		protected override void UpdateArmTransformsForAWA ()
		{
			// ignore
		}

		protected override void RotatePedInDirectionOfAiming ()
		{
			BaseAimMovementState.RotatePedInDirectionOfAiming( m_ped );
		}

		protected override void UpdateHeading ()
		{
			// we need to override default behaviour, because otherwise ped will be turning around while aiming
			// with AWA weapons

			m_ped.Heading = m_ped.AimDirection.WithXAndZ ().normalized;
		}
		*/

		// TODO: check camera collision - change camera offset





		public override void OnSubmitPressed ()
		{
			// ignore
		}

		public override void OnJumpPressed ()
		{
			// ignore
		}

		public override void OnCrouchButtonPressed ()
		{
			// ignore
		}

	}

}
