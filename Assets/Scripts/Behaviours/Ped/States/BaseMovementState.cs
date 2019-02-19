using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all movement states.
	/// </summary>
	public class BaseMovementState : DefaultState
	{
		

		public override void UpdateState() {

			base.UpdateState ();

			if (!this.IsActiveState)
				return;

			// TODO: check:
			// - if we should switch to any of aim states
			// - if we should enter falling state


			BaseMovementState.SwitchToMovementStateBasedOnInput (m_ped);

			if (!this.IsActiveState)
				return;

			if (m_ped.WeaponHolder.IsAimOn)
			{
				BaseAimState.SwitchToAimMovementStateBasedOnInput (m_ped);
			}

		}

		public static void SwitchToMovementStateBasedOnInput (Ped ped)
		{
			
			if (ped.IsWalkOn)
			{
				ped.SwitchState<WalkState> ();
			}
			else if (ped.IsRunOn)
			{
				ped.SwitchState<RunState> ();
			}
			else if (ped.IsSprintOn)
			{
				ped.SwitchState<SprintState> ();
			}
			else
			{
				ped.SwitchState<StandState> ();
			}

		}

		public override void OnSubmitPressed() {

			// try to enter vehicle
			m_ped.TryEnterVehicleInRange ();

		}

		public override void OnJumpPressed() {

			// try to jump

		}

	}

}
