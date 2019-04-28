using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleEnteringState : BaseVehicleState
	{
		PedModel PlayerModel { get { return m_ped.PlayerModel; } }



		public override void OnBecameActive() {
			
		}

		public bool TryEnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate = false)
		{
			// this code heavily modifies game state, so it can run only on server
			Net.NetStatus.ThrowIfNotOnServer();

			if (!this.CanEnterVehicle (vehicle, seatAlignment))
				return false;
			

			Vehicle.Seat seat = vehicle.GetSeat (seatAlignment);

			// switch state here
			m_ped.SwitchState<VehicleEnteringState>();

			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeat = seat;
			seat.OccupyingPed = m_ped;

			m_ped.characterController.enabled = false;


			if (m_ped.IsControlledByLocalPlayer)
			{
				if (m_ped.Camera != null) {
				//	m_ped.Camera.transform.SetParent (seat.Parent, true);
				}
			}

			m_ped.transform.SetParent(seat.Parent);
			m_ped.transform.localPosition = Vector3.zero;
			m_ped.transform.localRotation = Quaternion.identity;

			if (m_ped.IsControlledByLocalPlayer && seat.IsDriver)
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


			return true;
		}

		private System.Collections.IEnumerator EnterVehicleAnimation(Vehicle.Seat seat, bool immediate)
		{
			var animIndex = seat.IsLeftHand ? AnimIndex.GetInLeft : AnimIndex.GetInRight;

			PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootEnd, new Vector3(-1, -1, -1));

			if (!immediate)
			{
				var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
				animState.wrapMode = WrapMode.Once;

				// TODO: also check if this state is still active state
				while (animState.enabled)
				{
					yield return new WaitForEndOfFrame();
				}
			}

			// TODO: check if this state is still active, and if vehicle is alive


			// player now completely entered the vehicle

			// call method from CarSittingState - he will switch state
			m_ped.GetStateOrLogError<VehicleSittingState> ().EnterVehicle(this.CurrentVehicle, seat);

			// this variable is not needed - it can be obtained based on current state
		//	IsInVehicleSeat = true;


		}

	}

}
