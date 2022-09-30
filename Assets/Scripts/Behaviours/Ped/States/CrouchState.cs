using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchState : BaseMovementState, ICrouchState
	{
		public override AnimId movementAnim { get { return new AnimId ("ped", "WEAPON_crouch"); } }
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }



		protected override void SwitchToMovementState ()
		{
			// can only switch to CrouchMove state
			if ( m_ped.Movement.sqrMagnitude > float.Epsilon )
			{
                var crouchMoveState = m_ped.GetState<CrouchMoveState>();
				if (!BaseMovementState.EnoughTimePassedToSwitchBetweenMovementStates(this, crouchMoveState))
					return;
				m_ped.SwitchState(crouchMoveState.GetType());
			}
		}

		protected override void SwitchToAimState ()
		{
			CrouchState.SwitchToAimState(m_ped);
		}

		public static void SwitchToAimState(Ped ped)
		{
			// can only switch to CrouchAim state
			if( ped.IsAimOn && ped.IsHoldingWeapon && BaseMovementState.EnoughTimePassedToSwitchToAimState(ped) )
			{
				ped.SwitchState<CrouchAimState>();
			}
		}

		protected override void UpdateAnims ()
		{
			base.UpdateAnims();

		//	if( !this.IsActiveState )
		//		return;

			CrouchState.AdjustRootFramePosition(m_ped);

		}

		public static void AdjustRootFramePosition(Ped ped)
		{
			// we need to adjust local position of some bones - root frame needs to be 0.5 units below the ped

			var model = ped.PlayerModel;

			if (null == model.RootFrame)
				return;
			if (null == model.UnnamedFrame)
				return;

			// for some reason, y position always remains 0.25
		//	m_model.UnnamedFrame.transform.localPosition = m_model.UnnamedFrame.transform.localPosition.WithXAndZ();

			Vector3 pos = model.RootFrame.transform.localPosition;
			pos.y = -0.5f - model.UnnamedFrame.transform.localPosition.y;
			model.RootFrame.transform.localPosition = pos;

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
