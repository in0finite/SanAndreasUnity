using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;
using System.Linq;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleSittingState : BaseVehicleState
	{


		public override void OnBecameActive()
		{
			base.OnBecameActive();
			this.EnterVehicleInternal();
		}

		public override void OnBecameInactive()
		{
			this.Cleanup();

			base.OnBecameInactive();
		}

		public void EnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment)
		{
			this.EnterVehicle(vehicle, vehicle.GetSeat(seatAlignment));
		}

		public void EnterVehicle(Vehicle vehicle, Vehicle.Seat seat)
		{
			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeatAlignment = seat.Alignment;

			m_ped.SwitchState<VehicleSittingState> ();
		}

		void EnterVehicleInternal()
		{
			Vehicle vehicle = this.CurrentVehicle;
			Vehicle.Seat seat = this.CurrentVehicleSeat;

			VehicleEnteringState.PreparePedForVehicle(m_ped, vehicle, seat);

			this.UpdateAnimsWhilePassenger();

		}


		public override void OnSubmitPressed()
		{
			// exit the vehicle

			if (m_isServer)
				m_ped.ExitVehicle();
			else
				base.OnSubmitPressed();

		}

		public override void UpdateState() {

			base.UpdateState();

			if (!this.IsActiveState)
				return;

			var seat = this.CurrentVehicleSeat;
			if (seat != null)
			{
				if (seat.IsDriver)
					this.UpdateWheelTurning ();
				else
					this.UpdateAnimsWhilePassenger();
			}
			
		}

		protected virtual void UpdateWheelTurning()
		{
			
			m_model.VehicleParentOffset = Vector3.zero;

			var driveState = this.CurrentVehicle.Steering > 0 ? AnimIndex.DriveRight : AnimIndex.DriveLeft;

			var state = m_model.PlayAnim(AnimGroup.Car, driveState, PlayMode.StopAll);

			state.speed = 0.0f;
			state.wrapMode = WrapMode.ClampForever;
			state.time = Mathf.Lerp(0.0f, state.length, Mathf.Abs(this.CurrentVehicle.Steering));

		}

		protected virtual void UpdateAnimsWhilePassenger()
		{
			// if (this.CurrentVehicleSeat.IsDriver)
			// {
			// 	m_model.PlayAnim(AnimGroup.Car, AnimIndex.Sit, PlayMode.StopAll);
			// }
			// else
			{
				m_model.PlayAnim(AnimGroup.Car, AnimIndex.SitPassenger, PlayMode.StopAll);
			}
		}


		public override void UpdateCameraZoom()
		{
			m_ped.CameraDistanceVehicle = Mathf.Clamp(m_ped.CameraDistanceVehicle - m_ped.MouseScrollInput.y, 2.0f, 32.0f);
		}

		public override void CheckCameraCollision ()
		{
			BaseScriptState.CheckCameraCollision(m_ped, this.GetCameraFocusPos(), -m_ped.Camera.transform.forward, 
				m_ped.CameraDistanceVehicle);
		}

		public new Vector3 GetCameraFocusPos()
		{
			if (m_ped.CurrentVehicle != null)
				return m_ped.CurrentVehicle.transform.position;
			else
				return base.GetCameraFocusPos();
		}

	}

}
