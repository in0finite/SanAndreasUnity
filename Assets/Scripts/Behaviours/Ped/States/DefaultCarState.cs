using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class DefaultCarState : DefaultState, ICarState
	{
		protected Vehicle m_currentVehicle;
		public Vehicle CurrentVehicle { get { return m_currentVehicle; } }

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

	}

}
