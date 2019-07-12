using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Weapons;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Behaviours.Weapons;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all aim-movement states.
	/// </summary>
	public abstract class BaseAimMovementState : BaseScriptState, IAimState
	{
		protected Weapon m_weapon { get { return m_ped.CurrentWeapon; } }

		public abstract AnimId aimWithArm_LowerAnim { get; }

		public virtual float AimAnimMaxTime { get { return m_weapon.AimAnimMaxTime; } }
		public virtual float AimAnimFireMaxTime { get { return m_weapon.AimAnimFireMaxTime; } }



		public override void UpdateState()
		{

			base.UpdateState ();

			if (!this.IsActiveState)
				return;


			if (m_isServer)
			{
				if (this.SwitchToNonAimMovementState ())
					return;
				if (this.SwitchToFiringState ())
					return;
				if (this.SwitchToOtherAimMovementState ())
					return;
				if (this.SwitchToFallingState ())
					return;
			}

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

			ped.PlayerModel.Spine.forward = ped.AimDirection;

			// now apply offset to spine rotation
			// this has to be done because spine is not rotated properly - is it intentionally done, or is it error in model importing ?

			// TODO: actually, spine direction should be the same as ped's direction, not aim direction

			Vector3 eulers = ped.WeaponHolder.SpineOffset;
			if (ped.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
				eulers.y = 0;
			ped.PlayerModel.Spine.Rotate (eulers);
		//	PlayerModel.ChangeSpineRotation (this.CurrentWeaponTransform.forward, Camera.transform.position + Camera.transform.forward * Camera.farClipPlane - this.CurrentWeaponTransform.position, SpineRotationSpeed, ref tempSpineLocalEulerAngles, ref targetRot, ref spineRotationLastFrame);

		}

		protected virtual void RotatePedInDirectionOfAiming()
		{
			if (m_ped.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
				return;

			BaseAimMovementState.RotatePedInDirectionOfAiming( m_ped );
		}

		public static void RotatePedInDirectionOfAiming(Ped ped)
		{
			
//			Vector3 lookAtPos = Camera.transform.position + Camera.transform.forward * 500;
//			lookAtPos.y = m_player.transform.position.y;
//
//			m_player.transform.LookAt (lookAtPos, Vector3.up);

			Vector3 forward = ped.AimDirection;
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
			if (m_weapon != null)
			{
				AnimationState state;

				if (m_weapon.HasFlag (GunFlag.AIMWITHARM))
					state = this.UpdateAnimsAWA ();
				else
					state = this.UpdateAnimsNonAWA ();

				if (m_weapon)
				{
					m_weapon.AimAnimState = state;
					if (state)
						this.UpdateAimAnim (state);
				}

				if (m_weapon && m_weapon.HasFlag(GunFlag.AIMWITHARM))
				{
					// update arm transforms
					// this has to be done after updating aim anim
					this.UpdateArmTransformsForAWA();
				}

				// spine should be rotated no matter if state was changed or not during anim updating
				// this should be done AFTER updating anims
				this.RotateSpine ();
			}
		}

		/// <summary>
		/// Update anims for non AWA (AIMWITHARM) weapons.
		/// </summary>
		protected virtual AnimationState UpdateAnimsNonAWA()
		{
			return null;
		}

		/// <summary>
		/// Update anims for AWA (AIMWITHARM) weapons.
		/// </summary>
		protected virtual AnimationState UpdateAnimsAWA()
		{
			return BaseAimMovementState.UpdateAnimsAWA (m_ped, this.aimWithArm_LowerAnim);
		}

		public static AnimationState UpdateAnimsAWA(Ped ped, AnimId aimWithArm_LowerAnim)
		{

			// aim with arm
			// eg: pistol, tec9, sawnoff

			var model = ped.PlayerModel;

			model.Play2Anims (new AnimId (AnimGroup.Colt45, AnimIndex.colt45_fire), aimWithArm_LowerAnim);

			var state = model.LastAnimState;
			model.LastAnimState.wrapMode = WrapMode.ClampForever;

			model.RemoveAllMixingTransforms (model.LastAnimState);
			model.AddMixingTransform (model.LastAnimState, model.RightClavicle, true);

			model.AddMixingTransform (model.LastSecondaryAnimState, model.LeftClavicle, true);
			model.AddMixingTransforms (model.LastSecondaryAnimState, model.Pelvis, model.Belly, model.Spine, model.UpperSpine, model.RBreast, model.LBreast, model.Neck);

			if (model.AnimsChanged) {
				// reset model state
				model.ResetModelState ();
				// sample the animation, because otherwise, model will remain in original state for 1 frame
				model.AnimComponent.Sample ();
			}

			return state;
		}

		protected virtual void UpdateAimAnim(AnimationState state)
		{
			BaseAimMovementState.UpdateAimAnim (m_ped, state, this.AimAnimMaxTime, this.AimAnimFireMaxTime, () => this.TryFire());
		}

		public static void UpdateAimAnim(Ped ped, AnimationState state, float aimAnimMaxTime, float aimAnimFireMaxTime, 
			System.Func<bool> tryFireFunc)
		{
			
			if (state.time > aimAnimMaxTime) {

				if (ped.IsFiring) {
					state.enabled = true;

					// check if anim reached end
					if(state.time >= aimAnimFireMaxTime) {
						// anim reached end, revert it to start

						state.time = aimAnimMaxTime;
						ped.AnimComponent.Sample ();

					//	if (!ped.IsFireOn || !ped.IsAimOn)
						{
							// no longer firing
							if (Net.NetStatus.IsServer)
							{
								ped.StopFiring ();
							}
						}
					}
				} else {
					// check if we should start firing

					if (ped.IsFireOn && tryFireFunc()) {
						// we started firing

					} else {
						// we should remain in aim state
						state.time = aimAnimMaxTime;
						ped.AnimComponent.Sample ();
						state.enabled = false;
					}
				}

			}


		}

		protected virtual void UpdateArmTransformsForAWA()
		{
			BaseAimMovementState.UpdateArmTransformsForAWA (m_ped);
		}

		public static void UpdateArmTransformsForAWA(Ped ped)
		{
			var player = ped;
			var model = ped.PlayerModel;
			var weapon = ped.CurrentWeapon;


			float timePerc = Mathf.Clamp01 (model.LastAnimState.time / weapon.AimAnimMaxTime);

			// rotate arm to match direction of player

			// we'll need a few adjustments, because arm's right vector is player's forward vector,
			// and arm's forward vector is player's down vector => arm's up is player's left
		//	Vector3 forward = - player.transform.right ; // -player.transform.up;
		//	Vector3 up = player.transform.up; // -player.transform.right;
		//	Vector3 lookAtPos = player.transform.position + forward * 500;

			model.ResetFrameState (model.RightUpperArm);
			model.ResetFrameState (model.RightForeArm);
			model.ResetFrameState (model.RightHand);

			Vector3 aimDir = player.AimDirection;
			Vector3 aimDirLocal = player.transform.InverseTransformDirection (aimDir);

			bool isAimingOnOppositeSide = aimDirLocal.x < 0f;
			float oppositeSideAngle = Vector3.Angle( Vector3.forward, aimDirLocal.WithXAndZ () );
			bool isAimingBack = oppositeSideAngle > WeaponsManager.Instance.AIMWITHARM_maxAimAngle;

			Quaternion startRot = Quaternion.LookRotation( -player.transform.up, player.transform.forward ); // Quaternion.Euler (WeaponsManager.Instance.AIMWITHARM_upperArmStartRotationEulers);
			Quaternion endRot = Quaternion.LookRotation (aimDir); //Quaternion.LookRotation( forward, up );
		//	Vector3 endForwardLocal = new Vector3(0.9222f, -0.3429f, 0.179f);
		//	Vector3 endUpLocal = new Vector3(-0.3522f, -0.9357f, 0.02171f);
			Quaternion endRotForeArm = endRot;

			if (isAimingBack) {
				// aim in the air

				endRot = Quaternion.LookRotation ( Vector3.Lerp(-player.transform.up, player.transform.forward, 0.7f).normalized );

				// we need to apply rotation that is opposite of given offset for x axis - to assure that forehand's up matches ped's back
				Quaternion q = Quaternion.AngleAxis( 90f - WeaponsManager.Instance.AIMWITHARM_foreArmRotationOffset.x, player.transform.up );
				endRotForeArm = Quaternion.LookRotation (q * player.transform.up, q * (-player.transform.forward));

			} else if (isAimingOnOppositeSide) {
				// upper arm will slightly follow direction of aiming, but only along y and z axes
				// forearm will have direction of aiming

				Vector3 dir = aimDirLocal;
				dir.x = 0; // no looking left or right
				if (oppositeSideAngle != 0) {
				//	dir.y = Mathf.Sign (dir.y) * ( Mathf.Abs (dir.y) - 1.0f * oppositeSideAngle / 90f );
					dir.y -= 1.0f * oppositeSideAngle / 90f;
					dir.z += 1.0f * oppositeSideAngle / 90f;
					if (dir.y > 0)
						dir.y /= (1.0f + oppositeSideAngle / 90f);
				//	if (Mathf.Abs(dir.y) > dir.z)
				//		dir.z = Mathf.Abs(dir.y);
				}
				dir.Normalize ();
				dir = player.transform.TransformDirection (dir);
				endRot = Quaternion.LookRotation (dir);
			}

			// lerp
			Quaternion rot = Quaternion.Lerp( startRot, endRot, timePerc );
			Quaternion rotForeArm = Quaternion.Lerp (startRot, endRotForeArm, timePerc);

			if (timePerc == 1.0f) {
			//	Vector3 localForward = player.transform.InverseTransformDirection (model.RightUpperArm.forward);
			//	Vector3 localUp = player.transform.InverseTransformDirection (model.RightUpperArm.up);
			//	Debug.LogFormat ("local forward {0}, local up {1}", localForward.ToString("G4"), localUp.ToString("G4"));
			}

		//	Quaternion deltaRot = Quaternion.FromToRotation (Vector3.forward, aimDirLocal);

		//	Quaternion worldRot = player.transform.TransformRotation( rot );

		//	worldRot *= deltaRot;

			// assign new rotation
			// 'rot' is in player space
		//	model.RightUpperArm.rotation = worldRot;

		//	Quaternion convertRot = Quaternion.Euler (WeaponsManager.Instance.AIMWITHARM_upperArmEndRotationEulers);

			// head rotation
			Vector3 clampedAimDir = F.ClampDirection (aimDir, player.transform.forward, WeaponsManager.Instance.AIMWITHARM_maxHeadRotationAngle);
			Quaternion headRot = isAimingBack ? player.transform.rotation : Quaternion.LookRotation (clampedAimDir);
		//	headRot = Quaternion.Lerp( model.Head.rotation, headRot, 0.3f);


			// set new rotations and apply aim rotation offsets

			model.Head.rotation = headRot;
			model.Head.Rotate (WeaponsManager.Instance.AIMWITHARM_headRotationOffset);

			model.RightClavicle.Rotate (WeaponsManager.Instance.AIMWITHARM_clavicleRotationOffset);

			model.RightUpperArm.rotation = rot;
			model.RightUpperArm.Rotate (WeaponsManager.Instance.AIMWITHARM_upperArmRotationOffset);

			model.RightForeArm.rotation = rotForeArm;
			model.RightForeArm.Rotate (WeaponsManager.Instance.AIMWITHARM_foreArmRotationOffset);

			model.RightHand.localRotation = Quaternion.identity;
			model.RightHand.Rotate (WeaponsManager.Instance.AIMWITHARM_handRotationOffset);


		}


		protected virtual bool TryFire ()
		{
			if (m_weapon != null)
				return this.TryFire(m_weapon.GetFirePos(), m_weapon.GetFireDir());
			return false;
		}

		protected virtual bool TryFire (Vector3 firePos, Vector3 fireDir)
		{
			Ped ped = m_ped;
			var weapon = ped.CurrentWeapon;


			if (null == weapon)
				return false;

			if (ped.IsFiring)	// already firing
				return false;

			// check if there is ammo in clip
			if (weapon.AmmoInClip < 1)
				return false;

			if (m_isServer)
			{
				ped.StartFiring ();

				if (!ped.IsFiring)	// failed to start firing
					return false;
				
			}

			// reduce ammo
			weapon.AmmoInClip --;

			// update gun flash
		//	this.EnableOrDisableGunFlash (ped);
			if (weapon.GunFlash != null)
				weapon.GunFlash.gameObject.SetActive (true);
			weapon.UpdateGunFlashRotation ();

			// fire projectile
			if (m_isServer)
				F.RunExceptionSafe( () => weapon.FireProjectile (firePos, fireDir) );

			// send fire event to server
			if (Net.NetStatus.IsClientOnly)
			{
				
			}

			// play firing sound
			F.RunExceptionSafe (() => weapon.PlayFireSound() );

			// notify clients
			if (m_isServer)
				Net.PedSync.OnWeaponFired(m_ped, weapon, firePos);


			return true;
		}

		public virtual void OnClientTriedToFire(Vector3 firePos, Vector3 fireDir)
		{
			if (null == m_weapon)
				return;
			
			if (m_weapon.AmmoInClip < 1)
				return;

			if (!m_ped.IsFiring)
				m_ped.StartFiring();
			

		}


		public override void RotateCamera ()
		{
			base.RotateCamera ();

			// this must be called from here (right after the camera transform is changed), otherwise camera will shake
			// must check if weapon is null here, because camera is now updated in LateUpdate(), and weapon can become null between Update() and LateUpdate()
			if (m_weapon)
				this.RotatePedInDirectionOfAiming ();
		}

		public override void UpdateCameraZoom()
		{
			// ignore
		}

		public override void CheckCameraCollision ()
		{
			Vector3 cameraFocusPos = this.GetCameraFocusPos ();
			Vector3 castFrom = cameraFocusPos;
			float distance;
			Vector3 castDir = -m_ped.Camera.transform.forward;

			// use distance from gun aiming offset ?
			if (m_ped.CurrentWeapon != null && m_ped.CurrentWeapon.GunAimingOffset != null)
			{
			//	Vector3 desiredCameraPos = this.transform.TransformPoint (- _player.CurrentWeapon.GunAimingOffset.Aim) + Vector3.up * .5f;
			//	Vector3 desiredCameraPos = this.transform.TransformPoint( new Vector3(0.8f, 1.0f, -1) );
				Vector3 desiredCameraPos = cameraFocusPos + m_ped.Camera.transform.TransformVector (m_ped.WeaponHolder.cameraAimOffset);
				Vector3 diff = desiredCameraPos - castFrom;
				distance = diff.magnitude;
				castDir = diff.normalized;
			}
			else
			{
				distance = m_ped.CameraDistance;
			}

			BaseScriptState.CheckCameraCollision(m_ped, castFrom, castDir, distance);

		}


		public override void OnSubmitPressed()
		{

			// try to enter vehicle
			if (m_isServer)
				m_ped.TryEnterVehicleInRange ();
			else
				base.OnSubmitPressed();

		}

		public override void OnJumpPressed()
		{
			// ignore

		}

	}

}
