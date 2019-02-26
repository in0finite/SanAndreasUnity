using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class DesertEagle : Weapon
	{
		

		protected override void InitWeapon ()
		{
			base.InitWeapon();
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
				if (this.Data.gunData.AssocGroupId.EndsWith ("bad"))
					return new AnimId (AnimGroup.Python, AnimIndex.python_fire_poor);
				else
					return new AnimId (AnimGroup.Python, AnimIndex.python_fire);
			}
		}


	}

}
