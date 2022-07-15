using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Weapons
{
	public class Spraycan : Weapon
	{
		public override AnimId AimAnim => new AnimId("SPRAYCAN", "spraycan_fire");
		public override AnimId IdleAnim => new AnimId(AnimGroup.WalkCycle, AnimIndex.Idle);
		public override AnimId WalkAnim => new AnimId(AnimGroup.WalkCycle, AnimIndex.Walk);
		public override AnimId RunAnim => new AnimId(AnimGroup.WalkCycle, AnimIndex.Run);
	}
}
