using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
	
	public class PedStalker : Ped
	{
		public float stoppingDistance = 3;

		void Update ()
		{

			// reset input
			ResetInput ();

			// follow player instance

			if (Instance != null) {

				Vector3 targetPos = PlayerController.Instance.transform.position;
				float currentStoppingDistance = stoppingDistance;

				if (IsInVehicleSeat && !IsInVehicle) {
					// find a free vehicle seat to enter vehicle

					var vehicle = CurrentVehicle;
					//	var seat = Player.Instance.CurrentVehicleSeatAlignment;

					var closestfreeSeat = GetFreeSeats (vehicle).Select (sa => new { sa = sa, tr = vehicle.GetSeatTransform (sa) })
						.OrderBy (s => s.tr.Distance (this.transform.position))
						.FirstOrDefault ();

					if (closestfreeSeat != null) {
						// check if it is in range
						if (closestfreeSeat.tr.Distance (this.transform.position) < EnterVehicleRadius) {
							// the seat is in range
							EnterVehicle (vehicle, closestfreeSeat.sa);
						} else {
							// the seat is not in range
							// move towards this seat
							targetPos = closestfreeSeat.tr.position;
							currentStoppingDistance = 0.1f;
						}
					}

				} else if (!IsInVehicle && IsInVehicleSeat) {
					// target player is not in vehicle, and ours is
					// exit the vehicle

					ExitVehicle ();
				}


				if (IsInVehicle)
					return;

				Vector3 diff = targetPos - this.transform.position;
				float distance = diff.magnitude;

				if (distance > currentStoppingDistance)
				{
					Vector3 diffDir = diff.normalized;

					IsRunning = true;
					Movement = diffDir;
					Heading = diffDir;
				}

			}

		}

	}

}
