using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class MicroUzi : Weapon
	{

		protected override void InitWeapon ()
		{
			base.InitWeapon();
			this.CrouchSpineRotationOffset = WeaponsManager.Instance.crouchSpineRotationOffset2;
		}

	}

}
