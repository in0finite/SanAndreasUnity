using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class MP5 : Weapon
	{

		protected override void InitWeapon ()
		{
			base.InitWeapon();
		//	this.CrouchAimAnim = new AnimId("UZI", "UZI_crouchfire");
		}

		public override AnimId AimAnim {
			get {
				return new AnimId (AnimGroup.Uzi, AnimIndex.UZI_fire);
			}
		}

	}

}
