using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class Shotgun : Weapon
	{

		protected override void InitWeapon ()
		{
			base.InitWeapon();
			this.CrouchAimAnim = new AnimId("SHOTGUN", "shotgun_crouchfire");
		}

		public override AnimId AimAnim {
			get {
				if (this.Data.gunData.AssocGroupId.EndsWith ("bad"))
					return new AnimId (AnimGroup.Shotgun, AnimIndex.shotgun_fire_poor);
				else
					return new AnimId (AnimGroup.Shotgun, AnimIndex.shotgun_fire);
			}
		}

	}

}
