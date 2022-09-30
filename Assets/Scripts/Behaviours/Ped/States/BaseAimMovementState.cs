using SanAndreasUnity.Behaviours.Peds.AI;
using UnityEngine;
using UGameCore.Utilities;
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

		protected bool m_wasAimingBackWithAWAWeapon = false;
		protected float m_timeSinceAimingBackWithAWAWeapon = 0f;

		public virtual float TimeUntilStateCanBeSwitchedToOtherAimMovementState => PedManager.Instance.timeUntilAimMovementStateCanBeSwitchedToOtherAimMovementState;
		public virtual float TimeUntilStateCanBeEnteredFromOtherAimMovementState => PedManager.Instance.timeUntilAimMovementStateCanBeEnteredFromOtherAimMovementState;

		protected double m_timeWhenDidUnderAimDetection = 0f;




		public override void OnBecameActive()
        {
            base.OnBecameActive();

			m_wasAimingBackWithAWAWeapon = false;
			m_timeSinceAimingBackWithAWAWeapon = 0f;
		}

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


			if (null == m_weapon)
            {
				BaseMovementState.SwitchToMovementStateBasedOnInput(m_ped);
				return true;
			}

			// prevent fast state switching
			if (!EnoughTimePassedToSwitchToNonAimState(m_ped, Mathf.Max(this.AimAnimMaxTime, PedManager.Instance.minTimeToReturnToNonAimStateFromAimState)))
				return false;
			
			if (!m_ped.IsAimOn)
			{
				BaseMovementState.SwitchToMovementStateBasedOnInput (m_ped);
				return true;
			}

			if (m_ped.IsSprintOn)
			{
				if (m_weapon.CanSprintWithIt)
				{
					m_ped.SwitchState<SprintState>();
					return true;
				}
			}

			return false;
		}

		public static bool EnoughTimePassedToSwitchToNonAimState(Ped ped, float timeRequiredToPass)
        {
			var states = ped.CachedNonAimStates;

            for (int i = 0; i < states.Count; i++)
            {
				if (states[i].TimeSinceDeactivated < timeRequiredToPass)
					return false;
            }

			return true;
        }

		protected virtual bool SwitchToFiringState ()
		{
			return false;
		}

		protected virtual bool SwitchToOtherAimMovementState ()
		{
			System.Type type = GetAimMovementStateToSwitchToBasedOnInput(m_ped);
			var state = (BaseAimMovementState)m_ped.GetStateOrLogError(type);

			if (!EnoughTimePassedToSwitchBetweenAimMovementStates(this, state))
				return false;

			m_ped.SwitchState(type);

			return ! this.IsActiveState;
		}

		public static bool EnoughTimePassedToSwitchBetweenAimMovementStates(
			BaseAimMovementState currentState,
			BaseAimMovementState targetState)
		{
			if (currentState.TimeSinceActivated < currentState.TimeUntilStateCanBeSwitchedToOtherAimMovementState)
				return false;

			if (targetState.TimeSinceDeactivated < targetState.TimeUntilStateCanBeEnteredFromOtherAimMovementState)
				return false;

			return true;
		}

		protected virtual bool SwitchToFallingState ()
		{
			return false;
		}


		public static void SwitchToAimMovementStateBasedOnInput (Ped ped)
		{
            System.Type type = GetAimMovementStateToSwitchToBasedOnInput(ped);
			ped.SwitchState(type);
		}

		public static System.Type GetAimMovementStateToSwitchToBasedOnInput(Ped ped)
        {
			if (ped.IsWalkOn)
			{
				return typeof(WalkAimState);
			}
			else if (ped.IsRunOn || ped.IsSprintOn)
			{
				if (ped.CurrentWeapon != null && ped.CurrentWeapon.HasFlag(GunFlag.AIMWITHARM))
					return typeof(RunAimState);
				else
					return typeof(WalkAimState);
			}
			else
			{
				return typeof(StandAimState);
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
                    {
                        this.UpdateAimAnim (state);

						// do this right after UpdateAimAnim(), because that's the state when weapon conducts attack
						if (m_isServer && Time.timeAsDouble - m_timeWhenDidUnderAimDetection >= PedManager.Instance.timeIntervalToUpdateUnderAimStatus)
                        {
							m_timeWhenDidUnderAimDetection = Time.timeAsDouble;
							UpdateUnderAimDetection(m_ped);
						}
                    }
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

		public static void UpdateUnderAimDetection(Ped ped)
        {
			GetEffectiveFirePosAndDir(ped, WeaponAttackParams.Default, out Vector3 pos, out Vector3 dir);
			Damageable damagable = ped.CurrentWeapon.ProjectileRaycastForDamagable(
				pos, dir, WeaponAttackParams.Default, out bool attackWillBeConducted, out DamageInfo damageInfo);
			if (attackWillBeConducted && damagable != null)
			{
				var targetPed = damagable.GetComponent<Ped>();
				if (targetPed != null)
				{
					targetPed.OnUnderAimOfOtherPed(damageInfo);
				}
			}
		}

		protected virtual void UpdateArmTransformsForAWA()
		{
			BaseAimMovementState.UpdateArmTransformsForAWA (
				m_ped, ref m_wasAimingBackWithAWAWeapon, ref m_timeSinceAimingBackWithAWAWeapon);
		}

		public static void UpdateArmTransformsForAWA(Ped ped, ref bool wasAimingBack, ref float timeSinceAimingBack)
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

			bool isAimingBack = oppositeSideAngle > WeaponsManager.Instance.AIMWITHARM_maxAimAngle
				|| (wasAimingBack && timeSinceAimingBack < WeaponsManager.Instance.AIMWITHARM_timeUntilAbleToStopAimingBack);

			if (isAimingBack)
				timeSinceAimingBack += Time.deltaTime;
			else
				timeSinceAimingBack = 0f;

			wasAimingBack = isAimingBack;

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
				// This is a fix for when arm rotates in opposite direction - we need to define in which way to rotate arm to target rotation.
				// We do this by lerping from starting rotation to target rotation based on timePerc.
				Vector3 forward = Vector3.Lerp(player.transform.forward, player.transform.up, timePerc);
				Vector3 up = Vector3.Lerp(player.transform.up, -player.transform.forward, timePerc);
				endRotForeArm = Quaternion.LookRotation (q * forward, q * up);

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
			Vector3 clampedAimDir = MathUtils.ClampDirection (aimDir, player.transform.forward, WeaponsManager.Instance.AIMWITHARM_maxHeadRotationAngle);
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


		public static void GetEffectiveFirePosAndDir(Ped ped, WeaponAttackParams weaponAttackParams, out Vector3 pos, out Vector3 dir)
        {
			if (ped.IsControlledByLocalPlayer || null == ped.PlayerOwner)
            {
				pos = ped.FirePosition;
				dir = ped.FireDirection;
            }
            else    // this ped is owned by remote client
            {
				pos = ped.NetFirePos;
				dir = ped.NetFireDir;
            }
        }

		public static bool TryFire (Ped ped, WeaponAttackParams weaponAttackParams)
		{
			if (ped.CurrentWeapon != null)
			{
				if (Net.NetStatus.IsServer)
				{
					GetEffectiveFirePosAndDir(ped, weaponAttackParams, out Vector3 pos, out Vector3 dir);
					return TryFire(ped, pos, dir, weaponAttackParams);
				}
			}
			return false;
		}

        protected virtual bool TryFire()
        {
            return TryFire(m_ped, WeaponAttackParams.Default);
        }

		public static bool TryFire (Ped ped, Vector3 firePos, Vector3 fireDir, WeaponAttackParams weaponAttackParams)
		{
            bool isServer = Net.NetStatus.IsServer;
			var weapon = ped.CurrentWeapon;


			if (!isServer)	// for now, do this only on server
				return false;

			if (Net.NetStatus.IsClientOnly && ! ped.IsControlledByLocalPlayer)
				return false;

			if (null == weapon)
				return false;

			if (ped.IsFiring)	// already firing
				return false;

			// check if there is ammo in clip
			if (weapon.AmmoInClip < 1)
				return false;

			if (isServer)
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
			if (isServer)
				F.RunExceptionSafe( () => weapon.FireProjectile (firePos, fireDir, weaponAttackParams) );

			// send fire event to server
			if (Net.NetStatus.IsClientOnly && ped.IsControlledByLocalPlayer)
			{

			}

			// notify clients
			if (isServer)
				Net.PedSync.OnWeaponFired(ped, weapon, firePos);


			return true;
		}

        public virtual Vector3 GetFirePosition()
        {
            return GetFirePosition(m_ped);
        }

        public static Vector3 GetFirePosition(Ped ped)
        {
            return ped.CurrentWeapon != null ? ped.CurrentWeapon.GetFirePosWithoutPed() : ped.transform.position;
        }

        public virtual Vector3 GetFireDirection()
        {
            return GetFireDirection(m_ped, () => IsAimingBack(m_ped), WeaponAttackParams.Default);
        }

        public static Vector3 GetFireDirection(Ped ped, System.Func<bool> isAimingBackFunc, WeaponAttackParams weaponAttackParams)
        {
            if (null == ped.CurrentWeapon)
                return ped.AimDirection;

            if (isAimingBackFunc())
                return ped.transform.up;

            if (ped.IsControlledByLocalPlayer && ped.Camera != null)
            {
                // find ray going into the world
                Ray ray = ped.Camera.GetRayFromCenter();

                // raycast
                RaycastHit hit;
                if (ped.CurrentWeapon.ProjectileRaycast(ray.origin, ray.direction, out hit, weaponAttackParams))
                {
                    return (hit.point - ped.FirePosition).normalized;
                }

                // if any object is hit, direction will be from fire position to hit point

                // if not, direction will be same as aim direction

            }

            return ped.AimDirection;
        }

        public static bool IsAimingBack(Ped ped)
        {
            if (null == ped.CurrentWeapon)
                return false;

            if (!ped.CurrentWeapon.HasFlag(GunFlag.AIMWITHARM))
                return false;

            if (!ped.IsAiming)
                return false;

            Vector3 aimDirLocal = ped.transform.InverseTransformDirection(ped.AimDirection);

            float oppositeSideAngle = Vector3.Angle(Vector3.forward, aimDirLocal.WithXAndZ());
            return oppositeSideAngle > WeaponsManager.Instance.AIMWITHARM_maxAimAngle;
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

        public override void CheckCameraCollision()
        {
            CheckCameraCollision(m_ped, this.GetCameraFocusPos());
        }

        public static void CheckCameraCollision (Ped ped, Vector3 cameraFocusPos)
		{
			Vector3 castFrom = cameraFocusPos;
			float distance;
			Vector3 castDir = -ped.Camera.transform.forward;
            
			// use distance from gun aiming offset ?
			if (ped.CurrentWeapon != null && ped.CurrentWeapon.GunAimingOffset != null)
			{
			//	Vector3 desiredCameraPos = this.transform.TransformPoint (- _player.CurrentWeapon.GunAimingOffset.Aim) + Vector3.up * .5f;
			//	Vector3 desiredCameraPos = this.transform.TransformPoint( new Vector3(0.8f, 1.0f, -1) );
				Vector3 desiredCameraPos = cameraFocusPos + ped.Camera.transform.TransformVector (ped.WeaponHolder.cameraAimOffset);
				Vector3 diff = desiredCameraPos - castFrom;
				distance = diff.magnitude;
				castDir = diff.normalized;
			}
			else
			{
				distance = ped.CameraDistance;
			}

			BaseScriptState.CheckCameraCollision(ped, castFrom, castDir, distance);

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

		protected override void OnButtonPressedOnServer(string buttonName)
		{
			if (buttonName == "G")
			{
				// try to recruit peds to follow you
				RecruitPedUnderWeaponToFollowPed(m_ped);
			}
		}

		public static void RecruitPedUnderWeaponToFollowPed(Ped pedToFollow)
		{
			if (null == pedToFollow.CurrentWeapon)
				return;

			RaycastHit hit;
			if (!pedToFollow.CurrentWeapon.ProjectileRaycast(
				pedToFollow.IsControlledByLocalPlayer ? pedToFollow.FirePosition : pedToFollow.NetFirePos,
				pedToFollow.IsControlledByLocalPlayer ? pedToFollow.FireDirection : pedToFollow.NetFireDir,
				out hit,
				WeaponAttackParams.Default))
			{
				return;
			}

			// see if ped is hit

			var damageable = hit.collider.gameObject.GetComponentInParent<Damageable>();
			if (null == damageable)
				return;

			var hitPedAI = damageable.GetComponent<PedAI>();
			if (hitPedAI != null && hitPedAI.MyPed != pedToFollow)
			{
				// ray hit NPC ped
				hitPedAI.Recruit(pedToFollow);
			}
			
		}

	}

}
