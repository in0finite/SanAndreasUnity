using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface IVehicleState : IPedState
	{

		Vehicle CurrentVehicle { get; }
		Vehicle.Seat CurrentVehicleSeat { get; }

	}

}
