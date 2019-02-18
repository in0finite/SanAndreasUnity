using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Animation;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
	
	public partial class Ped : MonoBehaviour {

		[SerializeField] private float m_enterVehicleRadius = 2.0f;
		public float EnterVehicleRadius { get { return m_enterVehicleRadius; } set { m_enterVehicleRadius = value; } }

		public Vehicle CurrentVehicle { get; private set; }
		public bool IsInVehicle { get { return CurrentVehicle != null; } }
		public bool IsInVehicleSeat { get; private set; }
		public bool IsDrivingVehicle { get { return this.IsInVehicleSeat && this.CurrentVehicleSeat.IsDriver && this.IsInVehicle; } }
		public Vehicle.Seat CurrentVehicleSeat { get; private set; }
		public Vehicle.SeatAlignment CurrentVehicleSeatAlignment { get { return CurrentVehicleSeat.Alignment; } }



		public void EnterVehicle(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment, bool immediate = false)
		{
			// find state script, and call it's method
			this.GetStateOrLogError<Peds.States.CarEnteringState>().TryEnterVehicle( vehicle, seatAlignment, immediate );
		}

		public void ExitVehicle(bool immediate = false)
		{
			this.GetStateOrLogError<Peds.States.CarExitingState> ().ExitVehicle (immediate);
		}


		public static List<Vehicle.SeatAlignment> GetFreeSeats( Vehicle vehicle )
		{
			return vehicle.Seats.Where (s => !s.IsTaken).Select (s => s.Alignment).ToList ();

//			var freeSeats = new List<Vehicle.SeatAlignment> (vehicle.Seats.Select (s => s.Alignment));
//
//			var players = FindObjectsOfType<Player> ();
//
//			foreach (var p in players) {
//				if (p.IsInVehicle && p.CurrentVehicle == vehicle) {
//					freeSeats.Remove (p.CurrentVehicleSeatAlignment);
//				}
//			}
//
//			return freeSeats;
		}

		private void UpdateWheelTurning()
		{
			
		}


		public Vehicle FindVehicleInRange ()
		{

			// find any vehicles that have a seat inside the checking radius and sort by closest seat
			return FindObjectsOfType<Vehicle>()
				.Where(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position) < EnterVehicleRadius)
				.OrderBy(x => Vector3.Distance(transform.position, x.FindClosestSeatTransform(transform.position).position))
				.FirstOrDefault();
			
		}

		public Vehicle TryEnterVehicleInRange ()
		{
			var vehicle = this.FindVehicleInRange ();
			if (null == vehicle)
				return null;

			var seat = vehicle.FindClosestSeat(this.transform.position);

			this.EnterVehicle(vehicle, seat);

			return vehicle;
		}

	}

}
