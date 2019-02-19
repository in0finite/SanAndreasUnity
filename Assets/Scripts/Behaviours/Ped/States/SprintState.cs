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

			if (!this.IsActiveState)
				return;
			
		}

	}

}
