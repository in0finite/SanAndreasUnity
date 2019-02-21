using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class WalkAimState : BaseAimMovementState
	{
		public override AnimId aimWithArm_LowerAnim { get { return m_ped.CurrentWeapon.WalkAnim; } }


		public override void OnBecameActive ()
		{
			base.OnBecameActive ();
		//	m_ped.PlayerModel.PlayAnim (AnimGroup.Gun, AnimIndex.WALK_armed);
		}

	}

}
