using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleSittingState : BaseVehicleState
	{
		PedModel PlayerModel { get { return m_ped.PlayerModel; } }



		public override void OnBecameActive() {
			

		}

		public void EnterVehicle(Vehicle vehicle, Vehicle.Seat seat)
		{

			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeat = seat;

			m_ped.SwitchState<VehicleSittingState> ();

			if (seat.IsDriver)
			{
				m_ped.PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.Sit, PlayMode.StopAll);
			}
			else
			{
				m_ped.PlayerModel.PlayAnim(AnimGroup.Car, AnimIndex.SitPassenger, PlayMode.StopAll);
			}

		}


		public override void OnSubmitPressed() {

			// exit the vehicle
			m_ped.ExitVehicle();

		}

		public override void UpdateState() {

			base.UpdateState();

			if (m_ped.IsDrivingVehicle)
				this.UpdateWheelTurning ();
			
		}

		protected virtual void UpdateWheelTurning()
		{
			
			PlayerModel.VehicleParentOffset = Vector3.zero;

			var driveState = CurrentVehicle.Steering > 0 ? AnimIndex.DriveRight : AnimIndex.DriveLeft;

			var state = PlayerModel.PlayAnim(AnimGroup.Car, driveState, PlayMode.StopAll);

			state.speed = 0.0f;
			state.wrapMode = WrapMode.ClampForever;
			state.time = Mathf.Lerp(0.0f, state.length, Mathf.Abs(CurrentVehicle.Steering));

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
			return m_ped.CurrentVehicle.transform.position;
		}

	}

}
