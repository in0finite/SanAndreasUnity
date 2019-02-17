using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class DefaultCarState : DefaultState, ICarState
	{
		protected Vehicle m_currentVehicle;
		public override Vehicle CurrentVehicle { get { return m_currentVehicle; } }
		public override Vehicle.SeatAlignment CurrentVehicleSeatAlignment { get; private set; }



		protected override void UpdateHeading()
		{
			
		}

		protected override void UpdateRotation()
		{
			
		}

		protected override void UpdateMovement()
		{
			
		}

	}

}
