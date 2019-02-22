using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class WalkFireState : BaseFireMovementState
	{
		public override AnimId aimWithArm_LowerAnim { get { return m_ped.CurrentWeapon.WalkAnim; } }


		public override void OnBecameActive ()
		{
			base.OnBecameActive ();
		//	m_ped.PlayerModel.Play2Anims (m_ped.CurrentWeapon.AimAnim, new AnimId(AnimGroup.Gun, ));
		}

		protected override AnimationState UpdateAnimsNonAWA ()
		{
			return WalkAimState.UpdateAnimsNonAWA (m_ped);
		}

	}

}
