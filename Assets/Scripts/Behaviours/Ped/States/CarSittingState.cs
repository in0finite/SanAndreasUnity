using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CarSittingState : DefaultCarState
	{

		public override void OnBecameActive() {
			
			// play anim
			m_ped.PlayerModel.PlayAnim(Importing.Animation.AnimGroup.Car, Importing.Animation.AnimIndex.Sit);

		}

		protected override void OnSubmitPressed() {

			// TODO: exit the vehicle
		//	m_ped.ExitVehicle();
		//	m_ped.SwitchState<StandState>();

		}

		public override void UpdateState() {

			base.UpdateState();

			this.UpdateWheelTurning();
		}

		protected virtual void UpdateWheelTurning()
		{
			// TODO:

		}

	}

}
