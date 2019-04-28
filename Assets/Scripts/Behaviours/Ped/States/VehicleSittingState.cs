using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleSittingState : BaseVehicleState
	{


		public override void OnBecameInactive()
		{
			this.Cleanup();

			base.OnBecameInactive();
		}

		public void EnterVehicle(Vehicle vehicle, Vehicle.Seat seat)
		{

			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeat = seat;

			m_ped.SwitchState<VehicleSittingState> ();

			if (seat.IsDriver)
			{
				m_model.PlayAnim(AnimGroup.Car, AnimIndex.Sit, PlayMode.StopAll);
			}
			else
			{
				m_model.PlayAnim(AnimGroup.Car, AnimIndex.SitPassenger, PlayMode.StopAll);
			}

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

			// check if this is still active state ?

			if (m_ped.IsDrivingVehicle)
				this.UpdateWheelTurning ();
			
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
