using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class FlyState : BaseScriptState
	{
		public float moveMultiplier = 10f;
		public float moveFastMultiplier = 100f;



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

		protected override void UpdateHeading ()
		{
			m_ped.Heading = Vector3.Scale(m_ped.Movement, new Vector3(1f, 0f, 1f)).normalized;
		}

		protected override void UpdateMovement ()
		{
			Vector3 delta = m_ped.Movement * Time.fixedDeltaTime;
			delta *= m_ped.IsSprintOn ? this.moveFastMultiplier : this.moveMultiplier;
			m_ped.characterController.Move (delta);
		}

	}

}
