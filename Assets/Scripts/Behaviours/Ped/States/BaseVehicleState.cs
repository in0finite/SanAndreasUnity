using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class BaseVehicleState : BaseScriptState, IVehicleState
	{
		private Vehicle m_currentVehicle;
		public Vehicle CurrentVehicle { get { return m_currentVehicle; } protected set { m_currentVehicle = value; } }

		public Vehicle.Seat CurrentVehicleSeat { get; protected set; }
		public Vehicle.SeatAlignment CurrentVehicleSeatAlignment { get { return this.CurrentVehicleSeat.Alignment; } }


		protected override void UpdateHeading()
		{
			
		}

		protected override void UpdateRotation()
		{
			
		}

		protected override void UpdateMovement()
		{
			
		}

		protected override void ConstrainRotation ()
		{
			
		}

		public bool CanEnterVehicle (Vehicle vehicle, Vehicle.SeatAlignment seatAlignment)
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

			return true;
		}

	}

}
