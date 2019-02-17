using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class WalkState : DefaultState
	{

		public override void OnBecameActive() {

			// play anim
			m_ped.PlayerModel.PlayAnim(Importing.Animation.AnimGroup.WalkCycle, Importing.Animation.AnimIndex.Walk);

		}

		override void OnSubmitPressed() {

			m_ped.TryEnterVehicleInRange ();

		}

		override void OnJumpPressed() {

			m_ped.SwitchState<JumpState>();

		}

		override void UpdateState() {

			base.UpdateState();

			// ...

		}

	}

}
