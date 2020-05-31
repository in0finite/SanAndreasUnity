using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class CountryRifle : Weapon
	{

		protected override void InitWeapon ()
		{
			base.InitWeapon();
			this.CrouchAimAnim = new AnimId("RIFLE", "RIFLE_crouchfire");
		}

	}

}
