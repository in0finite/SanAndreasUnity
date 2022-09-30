using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class StandState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId (AnimGroup.WalkCycle, AnimIndex.Idle); } }
		public override AnimId movementWeaponAnim { get { return m_ped.CurrentWeapon.IdleAnim; } }

        public override float TimeUntilStateCanBeSwitchedToOtherMovementState => 0f;


        public override void UpdateState() {

			base.UpdateState();

			if (!this.IsActiveState)
				return;
			
		}

	}

}
