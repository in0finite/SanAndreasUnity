using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class RunState : BaseMovementState
	{

		public override void OnBecameActive() {

			// play anim
			m_ped.PlayerModel.PlayAnim(Importing.Animation.AnimGroup.WalkCycle, Importing.Animation.AnimIndex.Run);

		}

		public override void UpdateState() {

			base.UpdateState();

			// TODO: check if we should switch to stand state (if run key is no longer pressed)

		}

	}

}
