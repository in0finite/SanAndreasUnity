using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class FlyState : BaseScriptState
	{



		public override void OnBecameActive ()
		{
			base.OnBecameActive ();

			m_model.PlayAnim(AnimGroup.WalkCycle, AnimIndex.RoadCross, PlayMode.StopAll); // play 'flying' animation
		}

		public override void OnBecameInactive ()
		{
			m_ped.characterController.detectCollisions = true;
			base.OnBecameInactive ();
		}

		public void EnterState (bool enableCollision)
		{
			m_ped.SwitchState<FlyState> ();
			m_ped.characterController.detectCollisions = enableCollision;
		}

		public override void OnSubmitPressed ()
		{
			m_ped.TryEnterVehicleInRange ();
		}

		public override void OnJumpPressed ()
		{
			// ignore
		}

		public override void OnFlyButtonPressed ()
		{
			m_ped.SwitchState<StandState> ();
		}

		public override void OnFlyThroughButtonPressed ()
		{
			// toggle collision detection
			m_ped.characterController.detectCollisions = ! m_ped.characterController.detectCollisions;
		}

	}

}
