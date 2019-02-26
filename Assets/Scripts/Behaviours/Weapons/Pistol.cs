using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class Pistol : Weapon
	{

		protected override void InitWeapon ()
		{
			base.InitWeapon();
			this.CrouchSpineRotationOffset = WeaponsManager.Instance.crouchSpineRotationOffset2;
		}

	}

}
