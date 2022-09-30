using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class WalkState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId (AnimGroup.WalkCycle, AnimIndex.Walk); } }
		public override AnimId movementWeaponAnim { get { return m_ped.CurrentWeapon.WalkAnim; } }


	}

}
