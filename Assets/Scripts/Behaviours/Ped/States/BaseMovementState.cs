using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all movement states.
	/// </summary>
	public abstract class BaseMovementState : DefaultState
	{
		public abstract AnimId movementAnim { get; }
		public abstract AnimId movementWeaponAnim { get; }



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

			if (m_ped.IsAimOn && m_ped.IsHoldingWeapon)
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

		protected override void UpdateAnims ()
		{
			if (m_ped.CurrentWeapon != null)
			{
				m_ped.PlayerModel.PlayAnim (this.movementWeaponAnim);
			}
			else
			{
				m_ped.PlayerModel.PlayAnim (this.movementAnim);
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
