using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class PistolSilenced : Weapon
	{

		protected override void InitWeapon ()
		{
			base.InitWeapon();
			this.CrouchAimAnim = new AnimId("SILENCED", "SilenceCrouchfire");
			this.CrouchSpineRotationOffset = WeaponsManager.Instance.crouchSpineRotationOffset2;
		}

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

	}

}
