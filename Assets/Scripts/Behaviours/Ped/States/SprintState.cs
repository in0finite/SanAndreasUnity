using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class SprintState : BaseMovementState
	{

		public override void OnBecameActive() {

			// play anim
			m_ped.PlayerModel.PlayAnim(Importing.Animation.AnimGroup.MyWalkCycle, Importing.Animation.AnimIndex.sprint_civi);

		}

		public override void UpdateState() {

			base.UpdateState();

			// TODO: check if we should switch to stand state (if sprint key is no longer pressed)
			if (!m_ped.IsSprintOn)
				m_ped.SwitchState<StandState> ();
			
		}

	}

}
