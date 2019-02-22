using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class StandFireState : BaseFireMovementState
	{
		public override AnimId aimWithArm_LowerAnim { get { return m_ped.CurrentWeapon.IdleAnim; } }


		public override void OnBecameActive ()
		{
			base.OnBecameActive ();
		//	m_ped.PlayerModel.Play2Anims (m_ped.CurrentWeapon.AimAnim, m_ped.CurrentWeapon.AimAnimLowerPart);
		}

		protected override AnimationState UpdateAnimsNonAWA ()
		{
			return StandAimState.UpdateAnimsNonAWA (m_ped);
		}

	}

}
