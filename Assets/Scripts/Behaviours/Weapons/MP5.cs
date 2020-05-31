using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{

	public class MP5 : Weapon
	{
		
		public override AnimId AimAnim {
			get {
				return new AnimId (AnimGroup.Uzi, AnimIndex.UZI_fire);
			}
		}

	}

}
