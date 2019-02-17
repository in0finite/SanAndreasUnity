using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class StandState : DefaultState
	{

		public override void OnBecameActive() {

			// play anim
			m_ped.PlayerModel.PlayAnim(Importing.Animation.AnimGroup.WalkCycle, Importing.Animation.AnimIndex.Idle);

		}

		override void OnSubmitPressed() {

			//m_ped.SwitchState<CarEnterState>();

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
