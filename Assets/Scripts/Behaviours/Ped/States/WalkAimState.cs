using UnityEngine;
using UGameCore.Utilities;
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

		protected override AnimationState UpdateAnimsNonAWA ()
		{
			return WalkAimState.UpdateAnimsNonAWA (m_ped);
		}

		public static AnimationState UpdateAnimsNonAWA (Ped ped)
		{
			var player = ped;
			var PlayerModel = ped.PlayerModel;
			var weapon = ped.CurrentWeapon;


			float angle = Vector3.Angle (player.Movement, player.transform.forward);

			if (angle > 110) {
				// move backward
				PlayerModel.Play2Anims( weapon.AimAnim, new AnimId(AnimGroup.Gun, AnimIndex.GunMove_BWD) );
			} else if (angle > 70) {
				// strafe - move left/right
				float rightAngle = Vector3.Angle( player.Movement, player.transform.right );
				if (rightAngle > 90) {
					// left
					PlayerModel.Play2Anims( weapon.AimAnim, new AnimId(AnimGroup.Gun, AnimIndex.GunMove_L) );
				} else {
					// right
					PlayerModel.Play2Anims( weapon.AimAnim, new AnimId(AnimGroup.Gun, AnimIndex.GunMove_R) );
				}

				// we have to reset local position of root frame - for some reason, anim is changing position
				//	PlayerModel.RootFrame.transform.localPosition = Vector3.zero;
				Importing.Conversion.Animation.RemovePositionCurves( PlayerModel.LastSecondaryAnimState.clip, PlayerModel.Frames );

				PlayerModel.VelocityAxis = 0;
			} else {
				// move forward
				PlayerModel.Play2Anims( weapon.AimAnim, new AnimId(AnimGroup.Gun, AnimIndex.GunMove_FWD) );
			}

			PlayerModel.LastAnimState.wrapMode = WrapMode.ClampForever;
			var state = PlayerModel.LastAnimState;

			return state;
		}

	}

}
