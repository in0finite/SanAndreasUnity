using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all movement states.
	/// </summary>
	public abstract class BaseMovementState : BaseScriptState
	{
		public abstract AnimId movementAnim { get; }
		public abstract AnimId movementWeaponAnim { get; }



		public override void UpdateState() {

			base.UpdateState ();

			if (!this.IsActiveState)
				return;


			BaseMovementState.SwitchToMovementStateBasedOnInput (m_ped);

			if (!this.IsActiveState)
				return;

			if (m_ped.IsAimOn && m_ped.IsHoldingWeapon)
			{
				BaseAimMovementState.SwitchToAimMovementStateBasedOnInput (m_ped);
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
				if (ped.CurrentWeapon != null && !ped.CurrentWeapon.CanSprintWithIt)
					ped.SwitchState<StandState> ();
				else
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
