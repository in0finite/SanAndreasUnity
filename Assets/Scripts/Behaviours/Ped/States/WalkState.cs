using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class WalkState : BaseMovementState
	{

		public override void OnBecameActive() {

			// play anim
			m_ped.PlayerModel.PlayAnim(Importing.Animation.AnimGroup.WalkCycle, Importing.Animation.AnimIndex.Walk);

		}

		public override void UpdateState() {

			base.UpdateState();

			// TODO: check if we should switch to stand state (if walk key is no longer pressed)
			if (!m_ped.IsWalkOn)
				m_ped.SwitchState<StandState> ();
			
		}

	}

}
