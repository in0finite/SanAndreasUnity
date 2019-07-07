using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class RollState : BaseMovementState
	{
		public override AnimId movementAnim { get { return m_rollLeft ? new AnimId ("ped", "Crouch_Roll_L") : new AnimId ("ped", "Crouch_Roll_R"); } }
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }

		private bool m_rollLeft = false;
		private AnimationState m_animState;



		public bool CanRoll()
		{
			if( this.IsActiveState )
				return false;
			return true;
		}

		public bool Roll(bool left)
		{
			if( !this.CanRoll() )
				return false;

			m_rollLeft = left;
			m_ped.SwitchState<RollState>();

			return true;
		}

		public override void OnBecameActive ()
		{
			base.OnBecameActive();
			m_animState = m_model.PlayAnim( this.movementAnim );
			m_animState.wrapMode = WrapMode.Once;
			m_model.VelocityAxis = 0;	// movement will be done along x axis
		}


		protected override void SwitchToMovementState ()
		{
			// don't switch to any state from here - we'll switch when anim finishes
		}

		protected override void SwitchToAimState ()
		{
			// don't switch to any state from here - we'll switch when anim finishes
		}


		protected override void UpdateHeading ()
		{
			if (m_isServer || m_ped.IsControlledByLocalPlayer)
				m_ped.Heading = m_ped.transform.forward;
		}

		protected override void UpdateRotation ()
		{
			// don't change rotation
		}

		protected override void UpdateMovement ()
		{
			// adjust movement input before updating movement
			// because only server knows direction of rolling, we'll do this only on server
			if (m_isServer)
				m_ped.Movement = m_rollLeft ? -m_ped.transform.right : m_ped.transform.right;
			
			base.UpdateMovement();
		}

		protected override void UpdateAnims ()
		{
			// correct local x position of root frame
			Vector3 pos = m_model.RootFrame.transform.localPosition;
			pos.x = 0f;
			m_model.RootFrame.transform.localPosition = pos;
		}

		public override void LateUpdateState ()
		{
			base.LateUpdateState();

			if( !this.IsActiveState )
				return;

			if (m_isServer)
			{
				// check if anim is finished

				if( null == m_animState || !m_animState.enabled )
				{
					// anim finished
					// switch to other state

					// try to switch to crouch aim state
					CrouchState.SwitchToAimState(m_ped);
					if( !this.IsActiveState )
						return;

					// switch to crouch state
					m_ped.SwitchState<CrouchState>();
				}

			}

		}


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

		public override void OnFlyButtonPressed ()
		{
			// ignore
		}

		public override void OnFlyThroughButtonPressed ()
		{
			// ignore
		}

	}

}
