using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CarEnteringState : DefaultCarState
	{
		PedModel PlayerModel { get { return m_ped.PlayerModel; } }



		public override void OnBecameActive() {

			// TODO: get code from Ped_Vehicle class ; someone needs to assign target vehicle and seat - we should 
			// provide public method in this class


		}

		public bool TryEnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate = false)
		{
			if (m_ped.IsInVehicle)
				return false;

			if (m_ped.IsAiming || m_ped.WeaponHolder.IsFiring)
				return false;

			var seat = vehicle.GetSeat (seatAlignment);
			if (null == seat)
				return false;

			// check if specified seat is taken
			if (seat.IsTaken)
				return false;
			
			// everything is ok, we can enter vehicle

			// switch state here
			m_ped.SwitchState<CarEnteringState>();

			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeat = seat;
			seat.OccupyingPed = this;

			m_ped.characterController.enabled = false;


			if (m_ped.IsLocalPlayer)
			{
				if (m_ped.Camera != null) {
					m_ped.Camera.transform.SetParent (seat.Parent, true);
				}

				/*
                SendToServer(_lastPassengerState = new PlayerPassengerState {
                    Vechicle = vehicle,
                    SeatAlignment = (int) seatAlignment
                }, DeliveryMethod.ReliableOrdered, 1);
                */
			}

			m_ped.transform.SetParent(seat.Parent);
			m_ped.transform.localPosition = Vector3.zero;
			m_ped.transform.localRotation = Quaternion.identity;

			if (m_ped.IsLocalPlayer && seat.IsDriver)
			{
				vehicle.StartControlling();
			}

			m_ped.PlayerModel.IsInVehicle = true;


			if (!vehicle.IsNightToggled && WorldController.IsNight)
				vehicle.IsNightToggled = true;
			else if (vehicle.IsNightToggled && !WorldController.IsNight)
				vehicle.IsNightToggled = false;

			Debug.Log ("IsNightToggled? " + vehicle.IsNightToggled);


			StartCoroutine (EnterVehicleAnimation (seat, immediate));


		}

		private System.Collections.IEnumerator EnterVehicleAnimation(Vehicle.Seat seat, bool immediate)
		{
			var animIndex = seat.IsLeftHand ? AnimIndex.GetInLeft : AnimIndex.GetInRight;

			PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootEnd, new Vector3(-1, -1, -1));

			if (!immediate)
			{
				var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
				animState.wrapMode = WrapMode.Once;

				while (animState.enabled)
				{
					yield return new WaitForEndOfFrame();
				}
			}

			// player now completely entered the vehicle

			// call method from CarSittingState - he will switch state
			m_ped.GetStateOrLogError<CarSittingState> ().EnterVehicle(this.CurrentVehicle, seat);

			// this variable is not needed - it can be obtained based on current state
		//	IsInVehicleSeat = true;


		}

	}

}
