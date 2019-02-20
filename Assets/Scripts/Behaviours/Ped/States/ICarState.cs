using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface ICarState : IPedState
	{

		Vehicle CurrentVehicle { get; }
		Vehicle.Seat CurrentVehicleSeat { get; }

	}

}
