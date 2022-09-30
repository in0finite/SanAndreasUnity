using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchMoveState : BaseMovementState, ICrouchState
	{
		public override AnimId movementAnim { get { return new AnimId ("ped", "GunCrouchFwd"); } }
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }



		protected override void SwitchToMovementState ()
		{
			// can only switch to Crouch state
			if ( m_ped.Movement.sqrMagnitude < float.Epsilon )
			{
				var crouchState = m_ped.GetState<CrouchState>();
				if (!BaseMovementState.EnoughTimePassedToSwitchBetweenMovementStates(this, crouchState))
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
			{
				var standState = m_ped.GetState<StandState>();
				if (BaseMovementState.EnoughTimePassedToSwitchBetweenMovementStates(this, standState))
					m_ped.SwitchState(standState.GetType());
			}
			else
				base.OnCrouchButtonPressed();
		}

	}

}
