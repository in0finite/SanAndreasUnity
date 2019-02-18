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

			// TODO: check if we should enter walk, run, or sprint state

			if (m_ped.IsWalkOn)
			{
				m_ped.SwitchState<WalkState> ();
			}
			else if (m_ped.IsRunOn)
			{
				m_ped.SwitchState<RunState> ();
			}
			else if (m_ped.IsSprintOn)
			{
				m_ped.SwitchState<SprintState> ();
			}

		}

	}

}
