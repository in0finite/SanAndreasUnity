using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
	
	public class PedStalker : MonoBehaviour
	{

		public Ped MyPed { get; private set; }

		public float stoppingDistance = 3;

		public Ped TargetPed { get; set; }



		void Awake ()
		{
			this.MyPed = this.GetComponentOrLogError<Ped> ();
		}

		void Update ()
		{

			// reset input
			this.MyPed.ResetInput ();

			// follow target ped

			if (this.TargetPed != null) {

				Vector3 targetPos = this.TargetPed.transform.position;
				float currentStoppingDistance = this.stoppingDistance;

				if (this.TargetPed.IsInVehicleSeat && !this.MyPed.IsInVehicle) {
					// find a free vehicle seat to enter vehicle

					var vehicle = this.TargetPed.CurrentVehicle;
					
					var closestfreeSeat = Ped.GetFreeSeats (vehicle).Select (sa => new { sa = sa, tr = vehicle.GetSeatTransform (sa) })
						.OrderBy (s => s.tr.Distance (this.transform.position))
						.FirstOrDefault ();

					if (closestfreeSeat != null) {
						// check if it is in range
						if (closestfreeSeat.tr.Distance (this.transform.position) < this.MyPed.EnterVehicleRadius) {
							// the seat is in range
							this.MyPed.EnterVehicle (vehicle, closestfreeSeat.sa);
						} else {
							// the seat is not in range
							// move towards this seat
							targetPos = closestfreeSeat.tr.position;
							currentStoppingDistance = 0.1f;
						}
					}

				} else if (!this.TargetPed.IsInVehicle && this.MyPed.IsInVehicleSeat) {
					// target player is not in vehicle, and ours is
					// exit the vehicle

					this.MyPed.ExitVehicle ();
				}


				if (this.MyPed.IsInVehicle)
					return;

				Vector3 diff = targetPos - this.transform.position;
				float distance = diff.magnitude;

				if (distance > currentStoppingDistance)
				{
					Vector3 diffDir = diff.normalized;

					this.MyPed.IsRunOn = true;
					this.MyPed.Movement = diffDir;
					this.MyPed.Heading = diffDir;
				}

			}

		}

	}

}
