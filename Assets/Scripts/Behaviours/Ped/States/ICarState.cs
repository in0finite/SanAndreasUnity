using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface ICarState : Utilities.IState
	{

		Vehicle CurrentVehicle { get; }
		Vehicle.SeatAlignment CurrentVehicleSeatAlignment { get; }

	}

}
