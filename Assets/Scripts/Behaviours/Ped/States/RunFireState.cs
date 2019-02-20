using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class RunFireState : BaseFireMovementState
	{
		public override AnimId aimWithArm_LowerAnim { get { return m_ped.CurrentWeapon.RunAnim; } }


		public override void OnBecameActive ()
		{
			base.OnBecameActive ();
		//	m_ped.PlayerModel.Play2Anims (m_ped.CurrentWeapon.AimAnim, m_ped.CurrentWeapon.RunAnim);
		}

	}

}
