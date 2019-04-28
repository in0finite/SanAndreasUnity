using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleExitingState : BaseVehicleState
	{
		PedModel PlayerModel { get { return m_ped.PlayerModel; } }


		public override void OnBecameActive() {


		}

		public void ExitVehicle(bool immediate = false)
		{
			if (!m_ped.IsInVehicle || !m_ped.IsInVehicleSeat)
				return;
			
			// obtain current vehicle from Ped
			this.CurrentVehicle = m_ped.CurrentVehicle;
			this.CurrentVehicleSeat = m_ped.CurrentVehicleSeat;

			// after obtaining parameters, switch to this state
			m_ped.SwitchState<VehicleExitingState> ();

			if (this.CurrentVehicleSeat.IsDriver)
				this.CurrentVehicle.StopControlling();

			if (m_ped.IsControlledByLocalPlayer)
			{
				/*
                SendToServer(_lastPassengerState = new PlayerPassengerState {
                    Vechicle = null
                }, DeliveryMethod.ReliableOrdered, 1);
                */
			}
			else
			{
				//    _snapshots.Reset();
			}

			StartCoroutine (ExitVehicleAnimation (immediate));

		}

		private System.Collections.IEnumerator ExitVehicleAnimation(bool immediate)
		{

		//	IsInVehicleSeat = false;

			var seat = this.CurrentVehicleSeat;

			var animIndex = seat.IsLeftHand ? AnimIndex.GetOutLeft : AnimIndex.GetOutRight;

			PlayerModel.VehicleParentOffset = Vector3.Scale(PlayerModel.GetAnim(AnimGroup.Car, animIndex).RootStart, new Vector3(-1, -1, -1));

			if (!immediate)
			{
				var animState = PlayerModel.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
				animState.wrapMode = WrapMode.Once;

				// wait until anim finishes or stops
				while (animState.enabled)
					yield return new WaitForEndOfFrame();
			}

			// player now completely exited the vehicle

			PlayerModel.IsInVehicle = false;

			this.CurrentVehicle = null;
			this.CurrentVehicleSeat = null;
			seat.OccupyingPed = null;

			m_ped.transform.localPosition = PlayerModel.VehicleParentOffset;
			m_ped.transform.localRotation = Quaternion.identity;

			m_ped.transform.SetParent(null);

			m_ped.characterController.enabled = true;

			PlayerModel.VehicleParentOffset = Vector3.zero;

			// change camera parent
			if (m_ped.IsControlledByLocalPlayer) {
				if (m_ped.Camera != null) {
					m_ped.Camera.transform.SetParent (null, true);
				}
			}

			// switch to stand state
			m_ped.SwitchState<StandState> ();

		}

	}

}
