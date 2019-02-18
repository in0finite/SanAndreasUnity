using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all movement states.
	/// </summary>
	public class BaseMovementState : DefaultState
	{
		

		public override void UpdateState() {

			base.UpdateState ();

			// TODO: check:
			// - if we should switch to any of aim states
			// - if we should enter falling state

		}

		public override void OnSubmitPressed() {

			// try to enter vehicle
			m_ped.TryEnterVehicleInRange ();

		}

		public override void OnJumpPressed() {

			// try to jump

		}

	}

}
