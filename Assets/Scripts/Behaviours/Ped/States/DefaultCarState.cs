using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class DefaultCarState : DefaultState, ICarState
	{
		protected Vehicle m_currentVehicle;
		public Vehicle CurrentVehicle { get { return m_currentVehicle; } }
		public Vehicle.SeatAlignment CurrentVehicleSeatAlignment { get; protected set; }



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
