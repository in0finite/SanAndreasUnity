using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
	
	public class PedStalker : MonoBehaviour
	{

		public Ped Player { get; private set; }

		public float stoppingDistance = 3;



		void Awake ()
		{
			this.Player = this.GetComponentOrLogError<Ped> ();
		}

		void Update ()
		{

			// reset input
			this.Player.ResetInput ();

			// follow player instance

			if (Ped.Instance != null) {

				Vector3 targetPos = Ped.InstancePos;
				float currentStoppingDistance = this.stoppingDistance;

				if (Ped.Instance.IsInVehicleSeat && !this.Player.IsInVehicle) {
					// find a free vehicle seat to enter vehicle

					var vehicle = Ped.Instance.CurrentVehicle;
					//	var seat = Player.Instance.CurrentVehicleSeatAlignment;

					var closestfreeSeat = Ped.GetFreeSeats (vehicle).Select (sa => new { sa = sa, tr = vehicle.GetSeatTransform (sa) })
						.OrderBy (s => s.tr.Distance (this.transform.position))
						.FirstOrDefault ();

					if (closestfreeSeat != null) {
						// check if it is in range
						if (closestfreeSeat.tr.Distance (this.transform.position) < this.Player.EnterVehicleRadius) {
							// the seat is in range
							this.Player.EnterVehicle (vehicle, closestfreeSeat.sa);
						} else {
							// the seat is not in range
							// move towards this seat
							targetPos = closestfreeSeat.tr.position;
							currentStoppingDistance = 0.1f;
						}
					}

				} else if (!Ped.Instance.IsInVehicle && this.Player.IsInVehicleSeat) {
					// target player is not in vehicle, and ours is
					// exit the vehicle

					this.Player.ExitVehicle ();
				}


				if (this.Player.IsInVehicle)
					return;

				Vector3 diff = targetPos - this.transform.position;
				float distance = diff.magnitude;

				if (distance > currentStoppingDistance)
				{
					Vector3 diffDir = diff.normalized;

					this.Player.IsRunning = true;
					this.Player.Movement = diffDir;
					this.Player.Heading = diffDir;
				}

			}

		}

	}

}
