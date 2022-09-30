using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchAimState : BaseAimMovementState, ICrouchState
	{
		// not used
		public override AnimId aimWithArm_LowerAnim { get { throw new System.InvalidOperationException(); } }

		// override aim anim timings
		public override float AimAnimMaxTime { get { return m_weapon.CrouchAimAnimMaxTime; } }
		public override float AimAnimFireMaxTime { get { return m_weapon.CrouchAimAnimFireMaxTime; } }

		public float cameraFocusPosOffsetY = 0.25f;



		protected override bool SwitchToNonAimMovementState ()
		{
			if (null == m_weapon)
			{
				m_ped.SwitchState<CrouchState>();
				return true;
			}

			// prevent fast state switching
			if (!EnoughTimePassedToSwitchToNonAimState(m_ped, Mathf.Max(this.AimAnimMaxTime, PedManager.Instance.minTimeToReturnToNonAimStateFromAimState)))
				return false;

			if ( !m_ped.IsAimOn )
			{
				m_ped.SwitchState<CrouchState>();
				return true;
			}

			// switch to Roll state
			if(m_ped.Movement.sqrMagnitude > float.Epsilon && m_ped.GetStateOrLogError<RollState>().CanRoll())
			{
				float angle = Vector3.Angle(m_ped.Movement, m_ped.transform.forward);
				if( angle > 50 && angle < 130 )
				{
					float rightAngle = Vector3.Angle( m_ped.Movement, m_ped.transform.right );
					bool left = rightAngle > 90;
					m_ped.GetState<RollState>().Roll( left );
					return true;
				}
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
			
			if( null == m_model.Spine || null == m_weapon )
				return;

			Vector3 forward = m_ped.transform.forward.WithXAndZ();
			float xzLength = m_ped.AimDirection.WithXAndZ().magnitude;
			forward = forward.normalized * xzLength;
			forward.y = m_ped.AimDirection.y;
			forward.Normalize();

			m_model.Spine.forward = forward;

			// apply rotation offset

			m_model.Spine.Rotate( m_weapon.CrouchSpineRotationOffset );

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
			if (m_model.RootFrame != null)
				m_model.RootFrame.LocalVelocity = Vector3.zero;

		//	if( !this.IsActiveState )
		//		return;

			CrouchState.AdjustRootFramePosition(m_ped);

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

			if (m_isServer || m_ped.IsControlledByLocalPlayer)
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
