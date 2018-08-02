using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class PistolSilenced : Weapon
	{


		public override AnimId IdleAnim {
			get {
				return new AnimId (AnimGroup.WalkCycle, AnimIndex.Idle);
			}
		}

		public override AnimId WalkAnim {
			get {
				return new AnimId (AnimGroup.WalkCycle, AnimIndex.Walk);
			}
		}

		public override AnimId RunAnim {
			get {
				return new AnimId (AnimGroup.WalkCycle, AnimIndex.Run);
			}
		}

		public override AnimId AimAnim {
			get {
				return new AnimId (AnimGroup.Silenced, AnimIndex.Silence_fire);
			}
		}

//		public override void UpdateAnimWhileAiming (Player player)
//		{
//			var state = player.PlayerModel.PlayAnim (this.AimAnim);
//
//		}


	}

}
