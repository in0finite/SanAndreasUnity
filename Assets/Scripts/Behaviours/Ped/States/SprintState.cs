using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class SprintState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId (AnimGroup.MyWalkCycle, AnimIndex.sprint_civi); } }
		public override AnimId movementWeaponAnim { get { return new AnimId (AnimGroup.MyWalkCycle, AnimIndex.sprint_civi); } }


		protected override void SwitchToAimState()
		{
			// don't switch to aim state
			// we will switch to aim state from other movement states
		}

	}

}
