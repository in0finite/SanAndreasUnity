using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{
	
	public class RLauncher : Weapon
	{


		public override AnimId IdleAnim {
			get {
				return new AnimId (AnimGroup.Rocket, AnimIndex.idle_rocket);
			}
		}

		public override AnimId WalkAnim {
			get {
				return new AnimId (AnimGroup.Rocket, AnimIndex.walk_rocket);
			}
		}

		public override AnimId RunAnim {
			get {
				return new AnimId (AnimGroup.Rocket, AnimIndex.run_rocket);
			}
		}

		public override AnimId AimAnim {
			get {
				return new AnimId (AnimGroup.Rocket, AnimIndex.RocketFire);
			}
		}

		public override void UpdateAnimWhileHolding (Ped ped)
		{
			if (ped.IsSprinting) {
				// because anim reports incorrect velocity (it gives positive velocity, but it should give 0),
				// we have to make some fixes

				ped.PlayerModel.PlayAnim (this.IdleAnim);
				//state.normalizedTime = 0f;
				//player.AnimComponent.Sample ();
				//state.enabled = false;
				ped.PlayerModel.RootFrame.LocalVelocity = Vector3.zero;
			} else {
				base.UpdateAnimWhileHolding (ped);
			}
		}


	}

}
