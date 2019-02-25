using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchMoveState : BaseMovementState
	{
		public override AnimId movementAnim {
			get
			{
				float angle = Vector3.Angle (m_ped.Movement, m_ped.transform.forward);
				if( angle > 110 )
				{
					// move backward
					return new AnimId ("ped", "GunCrouchBwd");
				}
				// move forward
				return new AnimId ("ped", "GunCrouchFwd");
			}
		}
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }



		protected override void SwitchToMovementState ()
		{
			// can only switch to Crouch state
			if( m_ped.Movement.sqrMagnitude < float.Epsilon )
			{
				m_ped.SwitchState<CrouchState>();
			}
		}

		protected override void SwitchToAimState ()
		{
			// TODO: can only switch to CrouchAim state

		}

		public override void OnJumpPressed ()
		{
			// ignore
		}

		public override void OnCrouchButtonPressed ()
		{
			m_ped.SwitchState<StandState>();
		}

	}

}
