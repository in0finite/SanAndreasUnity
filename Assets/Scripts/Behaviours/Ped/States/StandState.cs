using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class StandState : BaseMovementState
	{

		public override void OnBecameActive() {

			// play anim
			m_ped.PlayerModel.PlayAnim(Importing.Animation.AnimGroup.WalkCycle, Importing.Animation.AnimIndex.Idle);

		}

		public override void UpdateState() {

			base.UpdateState();

			// check if we should enter walk, run, or sprint state

			BaseMovementState.SwitchToMovementStateBasedOnInput (m_ped);

		}

	}

}
