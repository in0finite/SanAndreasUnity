using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CarSittingState : DefaultCarState
	{

		public override void OnBecameActive() {
			
			// play anim
		//	m_ped.PlayerModel.PlayAnim(Importing.Animation.AnimGroup.Car, Importing.Animation.AnimIndex.Sit);

		}

		public void EnterVehicle(Vehicle vehicle, Vehicle.Seat seat)
		{

			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeat = seat;

			if (seat.IsDriver)
			{
				m_ped.PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.Sit, PlayMode.StopAll);
			}
			else
			{
				m_ped.PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.SitPassenger, PlayMode.StopAll);
			}

			m_ped.SwitchState<CarSittingState> ();

		}

		public override void OnSubmitPressed() {

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
