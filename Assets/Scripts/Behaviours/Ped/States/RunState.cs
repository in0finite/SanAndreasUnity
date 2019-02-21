using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class RunState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId (AnimGroup.WalkCycle, AnimIndex.Run); } }
		public override AnimId movementWeaponAnim { get { return m_ped.CurrentWeapon.RunAnim; } }


	}

}
