using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;
using System.Collections;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class JumpState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId (AnimGroup.WalkCycle, AnimIndex.Idle); } }
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }

		private Coroutine m_coroutine;
		private int m_currentAnimIndex = 0;

		[Range(0, 10)] public float launchVelocityMultiplier = 1f;
		[Range(0, 10)] public float glideVelocity = 2f;
		[Range(0, 10)] public float landVelocityMultiplier = 1f;

		public float reEntranceTime = 0.4f;
        public override float TimeUntilStateCanBeEnteredFromOtherMovementState => this.reEntranceTime;



        public override void OnBecameActive ()
		{
			base.OnBecameActive ();

			m_currentAnimIndex = 0;
			m_coroutine = this.StartCoroutine (this.Coroutine());
		}

		public override void OnBecameInactive ()
		{
			if (m_coroutine != null)
				this.StopCoroutine (m_coroutine);
			
			m_coroutine = null;

			base.OnBecameInactive ();
		}

		IEnumerator Coroutine()
		{
			// play 3 anims one after another

			m_currentAnimIndex = 0;

			var state = m_model.PlayAnim ("ped", "JUMP_launch");
			state.wrapMode = WrapMode.Once;
			while( state.enabled )
			{
				yield return null;
			}

			m_currentAnimIndex++;

			state = m_model.PlayAnim ("ped", "JUMP_glide");
			state.wrapMode = WrapMode.Once;
			while (state.enabled)
				yield return null;

			m_currentAnimIndex++;

			state = m_model.PlayAnim ("ped", "JUMP_land");
			state.wrapMode = WrapMode.Once;
			while (state.enabled)
				yield return null;

			// all anims finished
			// switch to other state
			// let it be done both on server and client
		//	Debug.LogFormat("All 3 anims finished, switching state");
			m_ped.SwitchState<StandState>();

		}

		public bool CanJump()
		{
			if (m_ped.CurrentWeapon != null && m_ped.CurrentWeapon.IsHeavy)
				return false;
			if( !m_ped.IsGrounded )
				return false;
			return true;
		}

		public bool Jump()
		{
			if (!this.CanJump ())
				return false;

			m_ped.SwitchState<JumpState> ();
			return true;
		}


		public override void UpdateState ()
		{
			base.UpdateState ();

			if (!this.IsActiveState)
				return;

			if (m_isServer)
			{
				// if character is grounded, then we are no longer in jump state
				if (m_currentAnimIndex == 1 && m_ped.IsGrounded)
				{
				//	Debug.LogFormat ("Character is grounded, exiting jump state");
					m_ped.SwitchState<StandState> ();
				}
			}
			
		}

		protected override void SwitchToMovementState ()
		{
			// ignore
		}

		protected override void SwitchToAimState ()
		{
			// ignore
		}

		protected override void UpdateAnims ()
		{
			// ignore
		}

		public override void OnSubmitPressed ()
		{
			// ignore
		}

		public override void OnJumpPressed ()
		{
			// ignore
		}

		public override void OnCrouchButtonPressed ()
		{
			// ignore
		}

		public override void OnFlyButtonPressed ()
		{
			// ignore
		}

		public override void OnFlyThroughButtonPressed ()
		{
			// ignore
		}

		protected override void UpdateHeading ()
		{
			// not required, since we don't use heading
			// do it both on server and client
			m_ped.Heading = m_ped.transform.forward;
		}

		protected override void UpdateRotation ()
		{
			// ignore
			// rotation can not be changed while jumping
		}

		protected override void UpdateMovement ()
		{
			// do it both on server and client

			Vector3 modelVelocity = m_model.Velocity;
			if( m_currentAnimIndex == 0 )
				modelVelocity.z *= this.launchVelocityMultiplier;
			else if( m_currentAnimIndex == 1 )
				modelVelocity.z = this.glideVelocity;
			else if( m_currentAnimIndex == 2 )
			{
				modelVelocity.y = Mathf.Min(modelVelocity.y, 0f);	// don't allow to move upwards
				modelVelocity.z *= this.landVelocityMultiplier;
			}

			Vector3 velocity = m_ped.transform.forward.WithXAndZ().normalized * modelVelocity.z + Vector3.up * modelVelocity.y;
			// we won't apply gravity

			m_ped.characterController.Move (velocity * Time.deltaTime);
		}


	}

}
