using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;
using System.Linq;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleEnteringState : BaseVehicleState
	{
		Coroutine m_coroutine;


		public override void OnBecameInactive()
		{
			// restore everything

			this.Cleanup();

			if (m_coroutine != null)
				StopCoroutine(m_coroutine);
			m_coroutine = null;

			base.OnBecameInactive();
		}

		public static void PreparePedForVehicle(Ped ped, Vehicle vehicle, Vehicle.Seat seat)
		{

			seat.OccupyingPed = ped;

			ped.characterController.enabled = false;


			ped.transform.SetParent(seat.Parent);
			ped.transform.localPosition = Vector3.zero;
			ped.transform.localRotation = Quaternion.identity;

			ped.PlayerModel.IsInVehicle = true;

			if (!VehicleManager.Instance.syncPedTransformWhileInVehicle) {
				if (ped.NetTransform != null)
					ped.NetTransform.syncInterval = 0;
			}

		}

		public bool TryEnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate = false)
		{
			Net.NetStatus.ThrowIfNotOnServer();

			if (!this.CanEnterVehicle (vehicle, seatAlignment))
				return false;

			this.EnterVehicle(vehicle, seatAlignment, immediate);
			
			return true;
		}

		internal void EnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate)
		{
			
			Vehicle.Seat seat = vehicle.GetSeat (seatAlignment);

			// switch state here
			m_ped.SwitchState<VehicleEnteringState>();

			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeat = seat;
			
			PreparePedForVehicle(m_ped, vehicle, seat);

			if (seat.IsDriver)
			{
				// TODO: this should be done when ped enters the car - or, it should be removed, because
				// vehicle should know if it has a driver
				vehicle.StartControlling();

				// if (m_isServer) {
				// 	var p = Net.Player.GetOwningPlayer(m_ped);
				// 	if (p != null)
				// 		Net.NetManager.AssignAuthority(vehicle.gameObject, p);
				// }
			}

			if (!vehicle.IsNightToggled && WorldController.IsNight)
				vehicle.IsNightToggled = true;
			else if (vehicle.IsNightToggled && !WorldController.IsNight)
				vehicle.IsNightToggled = false;


			// send message to clients
			if (m_isServer)
			{
				if (!immediate)
				{
					foreach(var pedSync in Net.Player.AllPlayers.Select(p => p.GetComponent<Net.PedSync>()))
						pedSync.PedStartedEnteringVehicle(m_ped);
				}
			}


			m_coroutine = StartCoroutine (EnterVehicleAnimation (seat, immediate));

		}

		private System.Collections.IEnumerator EnterVehicleAnimation(Vehicle.Seat seat, bool immediate)
		{
			var animIndex = seat.IsLeftHand ? AnimIndex.GetInLeft : AnimIndex.GetInRight;

			m_model.VehicleParentOffset = Vector3.Scale(m_model.GetAnim(AnimGroup.Car, animIndex).RootEnd, new Vector3(-1, -1, -1));

			if (!immediate)
			{
				var animState = m_model.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
				animState.wrapMode = WrapMode.Once;

				// wait until anim is finished or vehicle is destroyed
				while (animState.enabled && this.CurrentVehicle != null)
				{
					yield return new WaitForEndOfFrame();
				}
			}

			// check if vehicle is alive
			if (null == this.CurrentVehicle)
			{
				// vehicle destroyed in the meantime ? hmm... ped is a child of vehicle, so it should be
				// destroyed as well ?
				// anyway, switch to stand state
				if (m_isServer)
					m_ped.SwitchState<StandState>();
				yield break;
			}


			// ped now completely entered the vehicle

			// call method from VehicleSittingState - he will switch state
			if (m_isServer)
				m_ped.GetStateOrLogError<VehicleSittingState> ().EnterVehicle(this.CurrentVehicle, seat);

		}

	}

}
