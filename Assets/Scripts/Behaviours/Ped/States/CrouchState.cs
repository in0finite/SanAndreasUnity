using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId (AnimGroup.WalkCycle, AnimIndex.Idle); } }
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }


		public override void UpdateState() {

			base.UpdateState();

			if (!this.IsActiveState)
				return;

		}

		// TODO:
		// - can't switch to jump, sprint, run, walk
		// - can switch to Stand when Crouch or Jump is pressed, or to CrouchMove when run is on
		// - when aim is pressed, switch to CrouchAim


		protected override void SwitchToMovementState ()
		{
			// TODO: can only switch to CrouchMove state

		}

		protected override void SwitchToAimState ()
		{
			// TODO: can only switch to CrouchAim state

		}

		public override void OnJumpPressed ()
		{
			// switch to stand state
			// it's better to do this event-based, because after switching to stand state, we may enter
			// jump state right after it

			m_ped.SwitchState<StandState>();
		}

		public override void OnCrouchButtonPressed ()
		{
			// switch to stand state

			m_ped.SwitchState<StandState>();
		}

	}

}
