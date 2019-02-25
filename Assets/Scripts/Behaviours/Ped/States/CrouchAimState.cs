using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchAimState : BaseAimMovementState
	{
		// not used
		public override AnimId aimWithArm_LowerAnim { get { return m_ped.CurrentWeapon.IdleAnim; } }



		protected override bool SwitchToNonAimMovementState ()
		{
			// can only switch to Crouch state
			if( !m_ped.IsAimOn || !m_ped.IsHoldingWeapon )
			{
				m_ped.SwitchState<CrouchState>();
				return true;
			}
			return false;
		}

		protected override bool SwitchToFiringState ()
		{
			return false;
		}

		protected override bool SwitchToOtherAimMovementState ()
		{
			// there are no other states to switch to
			return false;
		}

		protected override void RotateSpine ()
		{
			// ignore
		}

		public override void StartFiring ()
		{
			// TODO: switch to CrouchFire state

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

		// TODO: check camera collision - change camera offset





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
