using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class Sawnoff : Weapon
	{

		protected override void InitWeapon ()
		{
			base.InitWeapon();
			this.CrouchAimAnim = new AnimId("COLT45", "colt45_crouchfire");
			this.CrouchSpineRotationOffset = WeaponsManager.Instance.crouchSpineRotationOffset2;
		}

	}

}
