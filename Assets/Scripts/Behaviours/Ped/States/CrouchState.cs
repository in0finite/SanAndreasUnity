using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId ("ped", "WEAPON_crouch"); } }
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }



		// Description:
		// - can't switch to jump, sprint, run, walk
		// - can switch to Stand when Crouch or Jump is pressed, or to CrouchMove when run is on
		// - when aim is pressed, switch to CrouchAim


		protected override void SwitchToMovementState ()
		{
			// can only switch to CrouchMove state
			if( m_ped.Movement.sqrMagnitude > float.Epsilon )
			{
				m_ped.SwitchState<CrouchMoveState>();
			}
		}

		protected override void SwitchToAimState ()
		{
			CrouchState.SwitchToAimState(m_ped);
		}

		public static void SwitchToAimState(Ped ped)
		{
			// can only switch to CrouchAim state
			if( ped.IsAimOn && ped.IsHoldingWeapon )
			{
				ped.SwitchState<CrouchAimState>();
			}
		}

		public override void OnJumpPressed ()
		{
			// switch to stand state
			// it's better to do this event-based, because after switching to stand state, we may enter
			// jump state right after it

			// we can't switch to stand state, because that will cause ped to jump (jump button will be on)

		//	m_ped.SwitchState<StandState>();
		}

		public override void OnCrouchButtonPressed ()
		{
			// switch to stand state

			m_ped.SwitchState<StandState>();
		}

	}

}
