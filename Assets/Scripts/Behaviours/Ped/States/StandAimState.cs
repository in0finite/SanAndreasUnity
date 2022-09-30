using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{
	
	public class StandAimState : BaseAimMovementState
	{
		public override AnimId aimWithArm_LowerAnim { get { return m_ped.CurrentWeapon.IdleAnim; } }

        public override float TimeUntilStateCanBeSwitchedToOtherAimMovementState => 0f;


        public override void OnBecameActive ()
		{
			base.OnBecameActive ();
		//	m_ped.PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.GUN_STAND);
		}

		protected override AnimationState UpdateAnimsNonAWA ()
		{
			return StandAimState.UpdateAnimsNonAWA (m_ped);
		}

		public static AnimationState UpdateAnimsNonAWA(Ped ped)
		{

			ped.PlayerModel.Play2Anims( ped.CurrentWeapon.AimAnim, ped.CurrentWeapon.AimAnimLowerPart );

			// some anims don't set root frame velocity, so we have to set it
			ped.PlayerModel.RootFrame.LocalVelocity = Vector3.zero;

			var state = ped.PlayerModel.LastAnimState;
			state.wrapMode = WrapMode.ClampForever;


			return state;
		}

	}

}
