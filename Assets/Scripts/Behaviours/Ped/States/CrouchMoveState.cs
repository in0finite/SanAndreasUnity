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
			// can only switch to Crouch state
			if( m_ped.Movement.sqrMagnitude < float.Epsilon )
			{
				m_ped.SwitchState<CrouchState>();
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
