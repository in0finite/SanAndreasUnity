using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Weapons;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Weapons;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using SanAndreasUnity.Importing.Animation;
using System.Reflection;

namespace SanAndreasUnity.Behaviours
{
	public static class WeaponSlot
	{
		public static readonly int Hand = 0,
		Melee = 1,
		Pistol = 2,
		Shotgun = 3,
		Submachine = 4, // uzi, mp5, tec9
		Machine = 5, // ak47, m4
		Rifle = 6,
		Heavy = 7, // rocket launcher, flame thrower, minigun
		SatchelCharge = 8,
		Misc = 9, // spraycan, extinguisher, camera
		Misc2 = 10, // dildo, vibe, flowers, cane
		Special = 11, // parachute, goggles
		Detonator = 12,

		Count = 13;
	}

	public static class WeaponId
	{
		
		public static readonly int Pistol = 346;
		public static readonly int PistolSilenced = 347;
		public static readonly int DesertEagle = 348;

		public static readonly int Shotgun = 349;
		public static readonly int SawnOff = 350;
		public static readonly int SPAS12 = 351;

		public static readonly int MicroUzi = 352;
		public static readonly int Tec9 = 372;
		public static readonly int MP5 = 353;

		public static readonly int AK47 = 355;
		public static readonly int M4 = 356;

		public static readonly int CountryRifle = 357;
		public static readonly int SniperRifle = 358;

		public static readonly int RocketLauncher = 359;
		public static readonly int RocketLauncherHS = 360;
		public static readonly int FlameThrower = 361;
		public static readonly int MiniGun = 362;

	}

//	public class WeaponData
//	{
//		public string type = "";
//		public string fireType = "";
//		public float targetRange = 0;
//		public float weaponRange = 0;
//		public int modelId = -1;
//		public int slot = -1;
//		public AnimGroup animGroup = AnimGroup.None;
//		public int clipCapacity = 0;
//		public int damage = 0;
//	}

	public class Weapon : MonoBehaviour
	{
		private WeaponDef definition = null;
		public WeaponDef Definition { get { return this.definition; } }

		private WeaponData data = null;
		public WeaponData Data { get { return this.data; } }

		private	WeaponData.GunAimingOffset gunAimingOffset;
		public WeaponData.GunAimingOffset GunAimingOffset { get { return this.gunAimingOffset; } }

		public int SlotIndex { get { return this.Data.weaponslot; } }

		public bool IsGun { get { return this.data.gunData != null; } }

		public int AmmoClipSize { get { return this.data.gunData != null ? this.data.gunData.ammoClip : 0 ; } }
		public int AmmoInClip { get; set; }
		public int AmmoOutsideOfClip { get; set; }
		public int TotalAmmo { get { return this.AmmoInClip + this.AmmoOutsideOfClip; } }

		public Texture2D HudTexture { get; private set; }

		protected Ped m_ped { get; private set; }
		public Ped PedOwner { get { return m_ped; } internal set { m_ped = value; } }


		private static List<System.Type> s_weaponTypes = new List<System.Type> ();

		protected WeaponsManager WeaponsSettings { get { return WeaponsManager.Instance; } }

		private static GameObject s_weaponsContainer = null;

		public static Texture2D CrosshairTexture { get; set; }
		public static Texture2D FistTexture { get; set; }

		public AnimationState AimAnimState { get; protected set; }
		private	float	m_aimAnimTimeForAimWithArmWeapon = 0f;
		//public bool IsInsideFireAnim { get { return this.AimAnimState != null && this.AimAnimState.enabled && this.AimAnimState.time > this.AimAnimMaxTime; } }
		public Transform GunFlash { get; private set; }



		static Weapon ()
		{
			// obtain all weapon types
			var myType = typeof (Weapon);
			foreach (Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				s_weaponTypes.AddRange (a.GetTypes ().Where (t => t.IsSubclassOf (myType)));
			}

		}


		public static Weapon Load (int modelId)
		{
			WeaponDef def = Item.GetDefinition<WeaponDef> (modelId);
			if (null == def)
				return null;

			WeaponData weaponData = WeaponData.LoadedWeaponsData.FirstOrDefault (wd => wd.modelId1 == def.Id);
			if (null == weaponData)
				return null;

			var geoms = Geometry.Load (def.ModelName, def.TextureDictionaryName);
			if (null == geoms)
				return null;

			if (null == s_weaponsContainer) {
				s_weaponsContainer = new GameObject ("Weapons");
			//	weaponsContainer.SetActive (false);
			}

			GameObject go = new GameObject (def.ModelName);
			go.transform.SetParent (s_weaponsContainer.transform);

			geoms.AttachFrames (go.transform, MaterialFlags.Default);

			Weapon weapon = AddWeaponComponent (go, weaponData);
			weapon.definition = def;
			weapon.data = weaponData;
			// cache gun aiming offset
			if (weapon.data.gunData != null)
				weapon.gunAimingOffset = weapon.data.gunData.aimingOffset;

			// load hud texture
			try {
				weapon.HudTexture = TextureDictionary.Load( def.TextureDictionaryName ).GetDiffuse( def.TextureDictionaryName + "icon" ).Texture;
			} catch {
				Debug.LogErrorFormat ("Failed to load hud icon for weapon: model {0}, txd {1}", def.ModelName, def.TextureDictionaryName);
			}

			return weapon;
		}

		private static Weapon AddWeaponComponent (GameObject go, WeaponData data)
		{
			// find type which inherits Weapon class, and whose name matches the one in data

			string typeName = data.weaponType.Replace ("_", "");

			var type = s_weaponTypes.Where (t => 0 == string.Compare (t.Name, typeName, true)).FirstOrDefault ();

			if (type != null) {
				return (Weapon)go.AddComponent (type);
			} else {
				return go.AddComponent<Weapon> ();
			}

		}


		public bool HasFlag( GunFlag gunFlag ) {

			if (this.data != null && this.data.gunData != null)
				return this.data.gunData.HasFlag (gunFlag);

			return false;
		}


		protected virtual void Awake ()
		{
			this.GunFlash = this.transform.FindChildRecursive("gunflash");
		}

		protected virtual void Start ()
		{

		}

		protected virtual void Update ()
		{

			if (WeaponsSettings.drawLineFromGun)
			{
				Vector3 start, end;
				this.GetLineFromGun (out start, out end);
				GLDebug.DrawLine (start, end, Color.red, 0, true);
			}

		}


		public virtual bool CanSprintWithIt {
			get {
				if (this.HasFlag (GunFlag.AIMWITHARM))
					return true;

				if (this.SlotIndex == WeaponSlot.Heavy || this.SlotIndex == WeaponSlot.Machine || this.SlotIndex == WeaponSlot.Rifle
				   || this.SlotIndex == WeaponSlot.Shotgun)
					return false;

				return true;
			}
		}

		public virtual bool CanTurnInDirectionOtherThanAiming {
			get {
				if (this.HasFlag (GunFlag.AIMWITHARM))
					return true;
				return false;
			}
		}

		public virtual AnimId IdleAnim {
			get {
				if (this.HasFlag (GunFlag.AIMWITHARM)) {
					return new AnimId (AnimGroup.WalkCycle, AnimIndex.Idle);
				} else {
					return new AnimId (AnimGroup.MyWalkCycle, AnimIndex.IdleArmed);
				}
			}
		}

		public virtual AnimId WalkAnim {
			get {
				if (this.HasFlag (GunFlag.AIMWITHARM)) {
					return new AnimId (AnimGroup.WalkCycle, AnimIndex.Walk);
				} else {
					return new AnimId (AnimGroup.Gun, AnimIndex.WALK_armed);
				}
			}
		}

		public virtual AnimId RunAnim {
			get {
				if (this.HasFlag (GunFlag.AIMWITHARM)) {
					return new AnimId (AnimGroup.WalkCycle, AnimIndex.Run);
				} else {
					return new AnimId (AnimGroup.Gun, AnimIndex.run_armed);
				}
			}
		}

		public virtual AnimId GetAnimBasedOnMovement (bool canSprint)
		{
			Ped ped = m_ped;

			if (ped.IsRunning) {

				return this.RunAnim;

			} else if (ped.IsWalking) {

				return this.WalkAnim;

			} else if (ped.IsSprinting) {

				if (canSprint) {
					return new AnimId (AnimGroup.MyWalkCycle, AnimIndex.sprint_civi);
				} else {
					return this.IdleAnim;
				}

			} else {
				// player is standing

				return this.IdleAnim;
			}

		}

		public virtual AnimId AimAnim {
			get {
				return new AnimId (AnimGroup.Rifle, AnimIndex.RIFLE_fire);
			}
		}

        public virtual AnimId AimAnimLowerPart
        {
            get
            {
                return new AnimId(AnimGroup.MyWalkCycle, AnimIndex.GUN_STAND);
            }
        }

		public virtual float AimAnimMaxTime {
			get {
				return Weapons.WeaponsManager.ConvertAnimTime (this.data.gunData.animLoopStart);
			}
		}

		public virtual float AimAnimFireMaxTime {
			get {
				return Weapons.WeaponsManager.ConvertAnimTime (this.data.gunData.animLoopEnd);
			}
		}

		public virtual float GunFlashDuration {
			get {
				return Weapons.WeaponsManager.Instance.GunFlashDuration;
			}
		}

		public bool IsAimingBack () {

			if (null == m_ped)
				return false;

			if (!this.HasFlag (GunFlag.AIMWITHARM))
				return false;

			if (!m_ped.IsAiming)
				return false;
			
			Vector3 aimDirLocal = m_ped.transform.InverseTransformDirection (m_ped.AimDirection);

			float oppositeSideAngle = Vector3.Angle( Vector3.forward, aimDirLocal.WithXAndZ () );
			return oppositeSideAngle > WeaponsSettings.AIMWITHARM_maxAimAngle;
		}

		public virtual void UpdateAnimWhileAiming ()
		{
			Ped player = m_ped;
			var CurrentWeapon = this;
			var PlayerModel = player.PlayerModel;
			var model = player.PlayerModel;


		//	this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.MyWalkCycle,
		//		AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.GUN_STAND);

			if (CurrentWeapon.HasFlag (GunFlag.AIMWITHARM)) {
				// aim with arm
				// eg: pistol, tec9, sawnoff

				model.Play2Anims (new AnimId (AnimGroup.Colt45, AnimIndex.colt45_fire), this.GetAnimBasedOnMovement (false));

				AimAnimState = model.LastAnimState;
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

				this.UpdateFireAnim (model.LastAnimState);


				/*
				PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Idle);

				// update fire state

				m_aimAnimTimeForAimWithArmWeapon += Time.deltaTime;

				if (player.WeaponHolder.NumFramesSinceStartedAiming <= 1 || player.WeaponHolder.NumFramesSinceSwitchedWeapon <= 1) {
					m_aimAnimTimeForAimWithArmWeapon = 0f;
				}

				if (m_aimAnimTimeForAimWithArmWeapon > this.AimAnimMaxTime) {

					if (player.WeaponHolder.IsFiring) {
						
						// check if anim reached end
						if(m_aimAnimTimeForAimWithArmWeapon >= this.AimAnimFireMaxTime) {
							// anim reached end, revert it to start

							m_aimAnimTimeForAimWithArmWeapon = this.AimAnimMaxTime;

							// no longer firing
							player.WeaponHolder.IsFiring = false;
						}
					} else {
						// check if we should start firing

						if (player.WeaponHolder.IsFireOn) {
							// we should start firing
							player.WeaponHolder.IsFiring = true;
						} else {
							// we should remain in aim state
							m_aimAnimTimeForAimWithArmWeapon = this.AimAnimMaxTime;
						}
					}

				}
				*/


				float timePerc = Mathf.Clamp01 (model.LastAnimState.time / this.AimAnimMaxTime);

				// rotate arm to match direction of player

				// we'll need a few adjustments, because arm's right vector is player's forward vector,
				// and arm's forward vector is player's down vector => arm's up is player's left
			//	Vector3 forward = - player.transform.right ; // -player.transform.up;
			//	Vector3 up = player.transform.up; // -player.transform.right;
			//	Vector3 lookAtPos = player.transform.position + forward * 500;

				model.ResetFrameState (model.RightUpperArm);
				model.ResetFrameState (model.RightForeArm);
				model.ResetFrameState (model.RightHand);

				Vector3 aimDir = player.Camera.transform.forward;
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


			} else {

				//	PlayerModel.PlayUpperLayerAnimations (AnimGroup.Rifle, AnimGroup.WalkCycle, AnimIndex.RIFLE_fire, AnimIndex.Idle);

				AnimationState state = null;

				if (player.IsRunning && player.Movement.sqrMagnitude > float.Epsilon) {
					// walk and aim at the same time

					float angle = Vector3.Angle (player.Movement, player.transform.forward);

					if (angle > 110) {
						// move backward
						PlayerModel.Play2Anims( this.AimAnim, new AnimId(AnimGroup.Gun, AnimIndex.GunMove_BWD) );
					} else if (angle > 70) {
						// strafe - move left/right
						float rightAngle = Vector3.Angle( player.Movement, player.transform.right );
						if (rightAngle > 90) {
							// left
							PlayerModel.Play2Anims( this.AimAnim, new AnimId(AnimGroup.Gun, AnimIndex.GunMove_L) );
						} else {
							// right
							PlayerModel.Play2Anims( this.AimAnim, new AnimId(AnimGroup.Gun, AnimIndex.GunMove_R) );
						}

						// we have to reset local position of root frame - for some reason, anim is changing position
					//	PlayerModel.RootFrame.transform.localPosition = Vector3.zero;
						Importing.Conversion.Animation.RemovePositionCurves( PlayerModel.LastSecondaryAnimState.clip, PlayerModel.Frames );

						PlayerModel.VelocityAxis = 0;
					} else {
						// move forward
						PlayerModel.Play2Anims( this.AimAnim, new AnimId(AnimGroup.Gun, AnimIndex.GunMove_FWD) );
					}

					PlayerModel.LastAnimState.wrapMode = WrapMode.ClampForever;
					state = PlayerModel.LastAnimState;

				} else {
					// just aim

					//state = PlayerModel.PlayAnim (this.AimAnim, true, false);
					PlayerModel.Play2Anims( this.AimAnim, this.AimAnimLowerPart );

					// some anims don't set root frame velocity, so we have to set it
					PlayerModel.RootFrame.LocalVelocity = Vector3.zero;

					state = PlayerModel.LastAnimState;
					state.wrapMode = WrapMode.ClampForever;
				}

				AimAnimState = state;


				this.UpdateFireAnim (state);
			}

		}

		protected virtual void UpdateFireAnim (AnimationState state)
		{
			Ped ped = m_ped;

			if (state.time > this.AimAnimMaxTime) {
				
				if (ped.WeaponHolder.IsFiring) {
					state.enabled = true;

					// check if anim reached end
					if(state.time >= this.AimAnimFireMaxTime) {
						// anim reached end, revert it to start

						state.time = this.AimAnimMaxTime;
						ped.AnimComponent.Sample ();

						// no longer firing
						ped.WeaponHolder.IsFiring = false;
					}
				} else {
					// check if we should start firing

					if (ped.WeaponHolder.IsFireOn && this.TryFire ()) {
						// we started firing

					} else {
						// we should remain in aim state
						state.time = this.AimAnimMaxTime;
						ped.AnimComponent.Sample ();
						state.enabled = false;
					}
				}

			}

		}

		public virtual void UpdateAnimWhileHolding ()
		{
			Ped ped = m_ped;
			ped.PlayerModel.PlayAnim (this.GetAnimBasedOnMovement (this.CanSprintWithIt));
		}

		public virtual void EnableOrDisableGunFlash ()
		{
			Ped ped = m_ped;

			// enable/disable gun flash
			if (this.GunFlash != null) {
				
				bool shouldBeVisible = false;

//				if (this.HasFlag (GunFlag.AIMWITHARM)) {
//					shouldBeVisible = m_aimAnimTimeForAimWithArmWeapon.BetweenExclusive (this.AimAnimMaxTime, this.AimAnimMaxTime + this.GunFlashDuration);
//				}
//				else
				{

					if (AimAnimState != null && AimAnimState.enabled) {
						// aim anim is being played

						if (AimAnimState.time.BetweenExclusive (this.AimAnimMaxTime, this.AimAnimMaxTime + this.GunFlashDuration)) {
							// muzzle flash should be visible
							shouldBeVisible = true;
						}
					}
				}

				shouldBeVisible &= ped.IsFiring;

				this.GunFlash.gameObject.SetActive (shouldBeVisible);
			}

		}

		public virtual void UpdateGunFlashRotation ()
		{

			if (null == this.GunFlash)
				return;

			if (!this.GunFlash.gameObject.activeInHierarchy)
				return;

			float randomFactor = Random.Range (0.75f, 1.25f);
			float delta = WeaponsManager.Instance.GunFlashRotationSpeed * Time.deltaTime * randomFactor;

			this.GunFlash.rotation *= Quaternion.AngleAxis (delta, Vector3.right);

		}


		#region Firing

		public float MaxRange { get { return this.Data.weaponRange; } }

		public float Damage { get { return this.Data.gunData.damage; } }

		public virtual bool TryFire ()
		{
			Ped ped = m_ped;

			if (ped.IsFiring)
				return false;

			// check if there is ammo in clip
			if (this.AmmoInClip < 1)
				return false;

			ped.IsFiring = true;

			// reduce ammo
			this.AmmoInClip --;

			// update gun flash
		//	this.EnableOrDisableGunFlash (ped);
			if (this.GunFlash != null)
				this.GunFlash.gameObject.SetActive (true);
			this.UpdateGunFlashRotation ();

			// fire projectile
			F.RunExceptionSafe( () => this.FireProjectile () );


			return true;
		}

		protected virtual void FireProjectile ()
		{
			// obtain fire position and direction

			Vector3 firePos = this.GetFirePos ();
			Vector3 fireDir = this.GetFireDir ();

			// raycast against all (non-breakable ?) objects

			RaycastHit hit;
			if (this.ProjectileRaycast (firePos, fireDir, out hit))
			{
				// if target object has damageable script, inflict damage to it

				var damageable = hit.transform.GetComponent<Damageable> ();
				if (damageable)
				{
					// ray hit something that can be damaged
					// damage it
					damageable.Damage( new DamageInfo() { amount = this.Damage } );
				}
			}

		}

		public bool ProjectileRaycast (Vector3 source, Vector3 dir, out RaycastHit hit)
		{
			return Physics.Raycast (source, dir, out hit, this.MaxRange, WeaponsManager.Instance.projectileRaycastMask);
		}

		public void GetLineFromGun (out Vector3 start, out Vector3 end)
		{
			float distance = this.MaxRange;
			Vector3 firePos = this.GetFirePos ();
			Vector3 fireDir = this.GetFireDir ();
			RaycastHit hit;
			if (this.ProjectileRaycast (firePos, fireDir, out hit))
			{
				distance = hit.distance;
			}

			start = firePos;
			end = firePos + fireDir * distance;
		}

		public virtual Vector3 GetFirePos ()
		{
			Vector3 firePos;

			if (this.GunFlash != null)
				firePos = this.GunFlash.transform.position;
			else
				firePos = this.transform.TransformPoint (this.Data.gunData.fireOffset);

			return firePos;
		}

		public virtual Vector3 GetFireDir ()
		{
			
			if (m_ped)
			{
				if (this.IsAimingBack ())
					return m_ped.transform.up;

				if (m_ped.IsLocalPlayer && m_ped.Camera != null)
				{
					// find ray going into the world
					Ray ray = m_ped.Camera.GetRayFromCenter ();

					// raycast
					RaycastHit hit;
					if (this.ProjectileRaycast (ray.origin, ray.direction, out hit))
					{
						return (hit.point - this.GetFirePos ()).normalized;
					}

					// if any object is hit, direction will be from fire position to hit point

					// if not, direction will be same as aim direction

				}

				return m_ped.WeaponHolder.AimDirection;
			}
			else if (this.GunFlash)
				return this.GunFlash.transform.right;
			else
				return this.transform.right;
		}

		#endregion


		public virtual void OnDrawGizmosSelected ()
		{
			// draw rays from gun


			Vector3 firePos = this.GetFirePos ();

			// ray based on transform
			Gizmos.color = Color.yellow;
			GizmosDrawCastedRay (firePos, this.transform.right);

			// ray based on gun flash transform
			if (this.GunFlash != null)
			{
				Gizmos.color = F.OrangeColor;
				GizmosDrawCastedRay (firePos, this.GunFlash.transform.right);
			}

			// ray based on aiming direction
			if (m_ped != null)
			{
				Gizmos.color = Color.red;
				GizmosDrawCastedRay (firePos, m_ped.WeaponHolder.AimDirection);
			}

			// ray based on firing direction
			Gizmos.color = Color.black;
			GizmosDrawCastedRay (firePos, this.GetFireDir ());

		}

		public void GizmosDrawCastedRay (Vector3 source, Vector3 dir)
		{
			float distance = 100f;
			RaycastHit hit;
			if (this.ProjectileRaycast (source, dir, out hit))
			{
				distance = hit.distance;
			}

			Gizmos.DrawLine (source, source + dir * distance);
		}

	}

}
