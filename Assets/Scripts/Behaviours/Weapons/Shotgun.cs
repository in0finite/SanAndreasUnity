using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class Shotgun : Weapon
	{


		public override AnimId AimAnim {
			get {
				return new AnimId (AnimGroup.Shotgun, AnimIndex.shotgun_fire);
			}
		}

	}

}
