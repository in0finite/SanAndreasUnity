using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class Spas12 : Weapon
	{


		public override AnimId AimAnim {
			get {
				return new AnimId (AnimGroup.Buddy, AnimIndex.buddy_fire);
			}
		}

	}

}
