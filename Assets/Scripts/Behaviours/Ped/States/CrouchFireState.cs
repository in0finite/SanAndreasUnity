using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class CrouchFireState : CrouchAimState, IFireState
	{
		

		protected override bool SwitchToFiringState ()
		{
			// there are no other fire movement states to switch to
			return false;
		}

		protected override bool SwitchToOtherAimMovementState ()
		{
			// we'll switch to aim state when fire anim finishes
			return false;
		}

		public override void StartFiring ()
		{
			// ignore
		}

		public virtual void StopFiring ()
		{
			// switch to crouch-aim state
			m_ped.SwitchState<CrouchAimState>();
		}


	}

}
