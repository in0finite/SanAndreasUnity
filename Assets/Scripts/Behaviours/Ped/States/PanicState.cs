using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class PanicState : BaseMovementState
	{
        public override AnimId movementAnim => new AnimId(AnimGroup.WalkCycle, AnimIndex.Panicked);
        public override AnimId movementWeaponAnim => this.movementAnim;


        protected override void SwitchToAimState()
		{
			// don't switch to aim state
			// we will switch to aim state from other movement states
		}

	}

}
