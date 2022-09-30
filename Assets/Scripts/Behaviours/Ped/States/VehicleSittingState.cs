using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;
using System.Linq;
using Mirror;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleSittingState : BaseVehicleState
	{
		protected Vector3 m_vehicleParentOffset = Vector3.zero;
		//Vector3 m_rootFramePos = Vector3.zero;



		public override void OnBecameActive()
		{
			base.OnBecameActive();
			if (m_isServer)	// clients will do this when vehicle gets assigned
				this.EnterVehicleInternal();
        }

		public override void OnBecameInactive()
		{
			m_vehicleParentOffset = Vector3.zero;
			this.Cleanup();

			base.OnBecameInactive();
		}

		protected override void GetAdditionalNetworkData(Mirror.NetworkWriter writer)
		{
			base.GetAdditionalNetworkData(writer);
			writer.Write(m_vehicleParentOffset);
		}

		protected override void ReadNetworkData(Mirror.NetworkReader reader)
		{
			base.ReadNetworkData(reader);
			m_vehicleParentOffset = reader.ReadVector3();
		}

		protected override void OnVehicleAssigned()
		{
			this.EnterVehicleInternal();
		}

		public void EnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment)
		{
			this.EnterVehicle(vehicle, vehicle.GetSeat(seatAlignment));
		}

		public void EnterVehicle(Vehicle vehicle, Vehicle.Seat seat)
		{
			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeatAlignment = seat.Alignment;

			m_ped.SwitchState (this.GetType());
		}

		protected virtual void EnterVehicleInternal()
		{
			Vehicle vehicle = this.CurrentVehicle;
			Vehicle.Seat seat = this.CurrentVehicleSeat;

			if (m_isServer)
				m_vehicleParentOffset = m_model.VehicleParentOffset;
			else if (m_isClientOnly)
				m_model.VehicleParentOffset = m_vehicleParentOffset;

			BaseVehicleState.PreparePedForVehicle(m_ped, vehicle, seat);

			// save root frame position

			// this.UpdateDriverAnim();	// play driver anim
			// m_model.AnimComponent.Sample();	// sample it
			// if (m_model.RootFrame != null)
			// 	m_rootFramePos = m_model.RootFrame.transform.localPosition;	// save root frame position
			// this.UpdateAnimsInternal();	// restore the correct anim
			// m_model.AnimComponent.Sample();

			// play anims
			this.UpdateAnimsInternal();

		}

        public override void OnPreviousWeaponButtonPressed()
        {
            if (m_ped.IsControlledByLocalPlayer && m_ped.IsDrivingVehicle)
                this.CurrentVehicle.SwitchRadioStation(false);
            else
                base.OnPreviousWeaponButtonPressed();
        }

        public override void OnNextWeaponButtonPressed()
        {
            if (m_ped.IsControlledByLocalPlayer && m_ped.IsDrivingVehicle)
                this.CurrentVehicle.SwitchRadioStation(true);
            else
                base.OnNextWeaponButtonPressed();
        }

        public override void OnSubmitPressed()
		{
			// exit the vehicle

			if (m_isServer)
				m_ped.ExitVehicle();
			else
				base.OnSubmitPressed();

		}

		public override void OnAimButtonPressed()
		{
			if (m_isServer)
			{
				if (m_ped.GetStateOrLogError<DriveByState>().CanEnterState(this.CurrentVehicle, this.CurrentVehicleSeatAlignment))
					m_ped.GetStateOrLogError<DriveByState>().EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
			}
			else
				base.OnAimButtonPressed();
		}

		protected override void UpdateAnims()
		{
			this.UpdateAnimsInternal();
		}

		protected virtual void UpdateAnimsInternal()
		{
			var seat = this.CurrentVehicleSeat;
			if (seat != null)
			{
				if (seat.IsDriver)
					this.UpdateDriverAnim ();
				else
					this.UpdatePassengerAnim();
			}
		}

		protected virtual void UpdateDriverAnim()
		{
			
			m_model.VehicleParentOffset = Vector3.zero;

			var driveState = this.CurrentVehicle.Input.steering > 0 ? AnimIndex.DriveRight : AnimIndex.DriveLeft;

			var state = m_model.PlayAnim(AnimGroup.Car, driveState, PlayMode.StopAll);

			state.speed = 0.0f;
			state.wrapMode = WrapMode.ClampForever;
			state.time = Mathf.Lerp(0.0f, state.length, Mathf.Abs(this.CurrentVehicle.Input.steering));

		}

		protected virtual void UpdatePassengerAnim()
		{
			// if (this.CurrentVehicleSeat.IsDriver)
			// {
			// 	m_model.PlayAnim(AnimGroup.Car, AnimIndex.Sit, PlayMode.StopAll);
			// }
			// else
			{

				// we have to assign offset every frame, because it can be changed when ped model changes
			//	m_model.VehicleParentOffset = m_vehicleParentOffset;
				// same goes for root frame position
				if (m_model.RootFrame != null && m_model.UnnamedFrame != null)
				{
					//m_model.RootFrame.transform.localPosition = m_rootFramePos;
					m_model.RootFrame.transform.localPosition = - m_model.UnnamedFrame.transform.localPosition;
				}

				m_model.PlayAnim(AnimGroup.Car, AnimIndex.SitPassenger, PlayMode.StopAll);
			}
		}


		public override void UpdateCameraZoom()
		{
			m_ped.CameraDistanceVehicle = Mathf.Clamp(m_ped.CameraDistanceVehicle - m_ped.MouseScrollInput.y, PedManager.Instance.minCameraDistanceFromPed,
				PedManager.Instance.maxCameraDistanceFromPed);
		}

		public override void CheckCameraCollision ()
		{
			BaseScriptState.CheckCameraCollision(m_ped, this.GetCameraFocusPos(), -m_ped.Camera.transform.forward, 
				m_ped.CameraDistanceVehicle);
		}

		public override Vector3 GetCameraFocusPos()
		{
			if (m_ped.CurrentVehicle != null)
				return m_ped.CurrentVehicle.transform.position;
			else
				return base.GetCameraFocusPos();
		}

		public override Ped.DamageResult OnDamaged(DamageInfo damageInfo)
		{
			if (damageInfo.damageType == DamageType.Explosion)
			{
				// ped should not be damaged by explosion while he is in vehicle
				return new Ped.DamageResult();
			}

			return base.OnDamaged(damageInfo);
		}

	}

}
