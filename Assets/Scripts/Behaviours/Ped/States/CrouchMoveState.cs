using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchMoveState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId ("ped", "GunCrouchFwd"); } }
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }



		protected override void SwitchToMovementState ()
		{
			if (this.TimeSinceActivated <= this.TimeUntilStateCanBeSwitchedToOtherMovementState)
				return;

			// can only switch to Crouch state
			if ( m_ped.Movement.sqrMagnitude < float.Epsilon )
			{
				var crouchState = m_ped.GetState<CrouchState>();
				if (crouchState.TimeSinceDeactivated <= crouchState.TimeUntilStateCanBeEnteredFromOtherMovementState)
					return;
				m_ped.SwitchState(crouchState.GetType());
			}
		}

		protected override void SwitchToAimState ()
		{
			// can only switch to CrouchAim state
			CrouchState.SwitchToAimState(m_ped);
		}

		public override void OnJumpPressed ()
		{
			// ignore
		}

		public override void OnCrouchButtonPressed ()
		{
			if (m_isServer)
				m_ped.SwitchState<StandState>();
			else
				base.OnCrouchButtonPressed();
		}

	}

}
