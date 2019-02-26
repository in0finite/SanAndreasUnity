using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class Tec9 : Weapon
	{

		protected override void InitWeapon ()
		{
			base.InitWeapon();
			this.NeckRotationOffset = WeaponsManager.Instance.crouchSpineRotationOffset2;
		}

	}

}
