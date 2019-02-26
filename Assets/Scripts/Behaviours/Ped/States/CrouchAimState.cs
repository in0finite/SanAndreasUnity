using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchAimState : BaseAimMovementState
	{
		// not used
		public override AnimId aimWithArm_LowerAnim { get { throw new System.InvalidOperationException(); } }

		// override aim anim timings
		public override float AimAnimMaxTime { get { return m_weapon.CrouchAimAnimMaxTime; } }
		public override float AimAnimFireMaxTime { get { return m_weapon.CrouchAimAnimFireMaxTime; } }

		public float cameraFocusPosOffsetY = 0.25f;



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
			// we'll use this method to rotate neck instead of spine

			if( null == m_model.Neck || null == m_weapon )
				return;

			Vector3 forward = m_ped.transform.forward;
			forward.y = m_ped.AimDirection.y;
			forward.Normalize();

			m_model.Neck.forward = forward;

			// apply rotation offset

			m_model.Neck.Rotate( m_weapon.NeckRotationOffset );

		}

		public override void StartFiring ()
		{
			// switch to CrouchFire state
			m_ped.SwitchState<CrouchFireState>();
		}

		protected override void UpdateAnims ()
		{
			base.UpdateAnims();

		//	if( !this.IsActiveState )
		//		return;

			// anim does not set correct velocity
			// set it to zero to make the ped stand in place
			// this should be done even if parent method changed active state
			m_model.RootFrame.LocalVelocity = Vector3.zero;

		//	if( !this.IsActiveState )
		//		return;

			// we need to adjust local position of some bones - root frame needs to be 0.5 units below the ped

			// for some reason, y position always remains 0.25
		//	m_model.UnnamedFrame.transform.localPosition = m_model.UnnamedFrame.transform.localPosition.WithXAndZ();

			Vector3 pos = m_model.RootFrame.transform.localPosition;
			pos.y = -0.5f - m_model.UnnamedFrame.transform.localPosition.y;
			m_model.RootFrame.transform.localPosition = pos;

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

		public override Vector3 GetCameraFocusPos ()
		{
			return m_ped.transform.position + Vector3.up * this.cameraFocusPosOffsetY;
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
