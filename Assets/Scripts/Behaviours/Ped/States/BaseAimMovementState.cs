using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Weapons;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all aim-movement states.
	/// </summary>
	public abstract class BaseAimMovementState : BaseScriptState, IAimState
	{

		public abstract AnimId aimWithArm_LowerAnim { get; }



		public override void UpdateState()
		{

			base.UpdateState ();

			if (!this.IsActiveState)
				return;


			if (this.SwitchToNonAimMovementState ())
				return;
			if (this.SwitchToFiringState ())
				return;
			if (this.SwitchToOtherAimMovementState ())
				return;
			if (this.SwitchToFallingState ())
				return;

		}

		public override void LateUpdateState ()
		{
			base.LateUpdateState ();

			if (!this.IsActiveState)
				return;

			m_ped.WeaponHolder.UpdateWeaponTransform ();

		}


		protected virtual bool SwitchToNonAimMovementState ()
		{
			// check if we should exit aiming state
			if (!m_ped.IsHoldingWeapon || !m_ped.IsAimOn)
			{
			//	Debug.LogFormat ("Exiting aim state, IsHoldingWeapon {0}, IsAimOn {1}", m_ped.IsHoldingWeapon, m_ped.IsAimOn);
				BaseMovementState.SwitchToMovementStateBasedOnInput (m_ped);
				return true;
			}
			return false;
		}

		protected virtual bool SwitchToFiringState ()
		{
			return false;
		}

		protected virtual bool SwitchToOtherAimMovementState ()
		{
			BaseAimMovementState.SwitchToAimMovementStateBasedOnInput (m_ped);
			return ! this.IsActiveState;
		}

		protected virtual bool SwitchToFallingState ()
		{
			return false;
		}


		public static void SwitchToAimMovementStateBasedOnInput (Ped ped)
		{

			if (ped.IsWalkOn)
			{
				ped.SwitchState<WalkAimState> ();
			}
			else if (ped.IsRunOn)
			{
				if (ped.CurrentWeapon != null && ped.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
					ped.SwitchState<RunAimState> ();
				else
					ped.SwitchState<WalkAimState> ();
			}
			else if (ped.IsSprintOn)
			{
				ped.SwitchState<StandAimState> ();
			}
			else
			{
				ped.SwitchState<StandAimState> ();
			}

		}


		protected virtual void RotateSpine()
		{
			BaseAimMovementState.RotateSpineToMatchAimDirection (m_ped);
		}

		public static void RotateSpineToMatchAimDirection (Ped ped)
		{
			if (null == ped.CurrentWeapon)
				return;
			
			if (ped.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
				return;

			// TODO: spine's forward vector should be the same as aim direction
			// this will not work for peds without camera
			ped.PlayerModel.Spine.LookAt(ped.Camera.transform.position + ped.Camera.transform.forward * 500);

			// now apply offset to spine rotation
			// this has to be done because spine is not rotated properly - is it intentionally done, or is it error in model importing ?

			Vector3 eulers = ped.WeaponHolder.SpineOffset;
			if (ped.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
				eulers.y = 0;
			ped.PlayerModel.Spine.Rotate (eulers);
		//	PlayerModel.ChangeSpineRotation (this.CurrentWeaponTransform.forward, Camera.transform.position + Camera.transform.forward * Camera.farClipPlane - this.CurrentWeaponTransform.position, SpineRotationSpeed, ref tempSpineLocalEulerAngles, ref targetRot, ref spineRotationLastFrame);

		}

		public static void RotatePedInDirectionOfAiming(Ped ped)
		{
			if (!ped.WeaponHolder.rotatePlayerInDirectionOfAiming)
				return;

			if (ped.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
				return;

//			Vector3 lookAtPos = Camera.transform.position + Camera.transform.forward * 500;
//			lookAtPos.y = m_player.transform.position.y;
//
//			m_player.transform.LookAt (lookAtPos, Vector3.up);

			// TODO: this will not work if ped doesn't have camera

			Vector3 forward = ped.Camera.transform.forward;
			forward.y = 0;
			forward.Normalize ();
		//	m_player.transform.forward = forward;
			ped.Heading = forward;


		}


		public virtual void StartFiring()
		{
			BaseFireMovementState.SwitchToFireMovementStateBasedOnInput(m_ped);
		}


		protected override void UpdateAnims ()
		{
			if (m_ped.CurrentWeapon != null)
			{
				m_ped.CurrentWeapon.UpdateAnimWhileAiming (this.aimWithArm_LowerAnim);

				// spine should be rotated no matter if state was changed or not during anim updating
				// this should be done AFTER updating anims
				this.RotateSpine ();
			}
		}


		public override void OnSubmitPressed()
		{

			// try to enter vehicle
			m_ped.TryEnterVehicleInRange ();

		}

		public override void OnJumpPressed()
		{
			// ignore

		}

	}

}
