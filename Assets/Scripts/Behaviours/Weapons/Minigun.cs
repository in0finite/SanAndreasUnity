using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{
	public class Minigun : Weapon
	{
        public override AnimId AimAnim => new AnimId(AnimGroup.Flame, AnimIndex.FLAME_fire);
    }
}
