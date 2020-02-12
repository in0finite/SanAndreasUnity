using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

    public class DriveByState : VehicleSittingState, IAimState
    {
        
        public override void EnterVehicle(Vehicle vehicle, Vehicle.Seat seat)
		{
			this.CurrentVehicle = vehicle;
			this.CurrentVehicleSeatAlignment = seat.Alignment;

			m_ped.SwitchState<DriveByState> ();
		}

        protected override void EnterVehicleInternal()
        {
            if (m_isServer)
				m_vehicleParentOffset = m_model.VehicleParentOffset;
			else if (m_isClientOnly)
				m_model.VehicleParentOffset = m_vehicleParentOffset;

			BaseVehicleState.PreparePedForVehicle(m_ped, this.CurrentVehicle, this.CurrentVehicleSeat);

            PlayAnims();

        }

        protected override void UpdateAnims()
        {
            PlayAnims();
        }

        void PlayAnims()
        {
            if (this.CurrentVehicleSeat != null)
                m_model.PlayAnim(new Importing.Animation.AnimId("ped", "DrivebyL_" + (this.CurrentVehicleSeat.IsLeftHand ? "L" : "R")));
        }

        void IAimState.StartFiring()
        {
            // switch to firing state

        }

        // camera

        public override void OnAimButtonPressed()
        {
            // switch to sitting state
            if (m_isServer)
                m_ped.GetStateOrLogError<VehicleSittingState>().EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
            else
                base.OnAimButtonPressed();
        }

    }

}
