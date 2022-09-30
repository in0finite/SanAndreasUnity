using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class FlyState : BaseScriptState
	{
		public float moveMultiplier = 10f;
		public float moveFastMultiplier = 100f;
		private bool m_flyThrough = false;



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

		public void EnterState (bool flyThrough)
		{
			m_ped.SwitchState<FlyState> ();
			m_flyThrough = flyThrough;
			m_ped.characterController.detectCollisions = !flyThrough;
		}

		public override void OnSubmitPressed ()
		{
			if (m_isServer)
				m_ped.TryEnterVehicleInRange ();
			else
				base.OnSubmitPressed();
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
			m_flyThrough = !m_flyThrough;
			m_ped.characterController.detectCollisions = !m_flyThrough;
		}

		protected override void UpdateHeading ()
		{
			m_ped.Heading = Vector3.Scale(m_ped.Movement, new Vector3(1f, 0f, 1f)).normalized;
		}

		protected override void UpdateMovement ()
		{
			Vector3 delta = m_ped.Movement * Time.fixedDeltaTime;
			delta *= m_ped.IsSprintOn ? this.moveFastMultiplier : this.moveMultiplier;
			if (m_flyThrough)
				m_ped.transform.position += delta;
			else
				m_ped.characterController.Move (delta);
		}

	}

}
