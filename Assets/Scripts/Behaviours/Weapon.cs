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

		public AnimationState AimAnimState { get; set; }
		private	float	m_aimAnimTimeForAimWithArmWeapon = 0f;
		//public bool IsInsideFireAnim { get { return this.AimAnimState != null && this.AimAnimState.enabled && this.AimAnimState.time > this.AimAnimMaxTime; } }
		public Transform GunFlash { get; private set; }


		// weapon sounds are located in SFX -> GENRL -> BANK 137
		// these indexes represent indexes of sounds in that bank
		public static Dictionary<int, int> weaponSoundIndexes = new Dictionary<int, int>() {
			{WeaponId.Pistol, 6},	// not correct
			{WeaponId.PistolSilenced, 24},
			{WeaponId.DesertEagle, 6},
			{WeaponId.Shotgun, 21},
			{WeaponId.SawnOff, 21},
			{WeaponId.SPAS12, 22},
			{WeaponId.Tec9, 1},
			{WeaponId.MicroUzi, 0},
			{WeaponId.MP5, 18},
			{WeaponId.AK47, 4},
			{WeaponId.M4, 3},
			{WeaponId.CountryRifle, 26},
			{WeaponId.SniperRifle, 26},
			{WeaponId.MiniGun, 11},
		//	{WeaponId.RocketLauncher, 68},
		//	{WeaponId.RocketLauncherHS, 68},
		};

		// used to play weapon sound
		AudioSource m_audioSource;



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

			// weapon sound
			F.RunExceptionSafe (() => {
				if (weaponSoundIndexes.ContainsKey (modelId))
				{
					var audioSource = go.GetOrAddComponent<AudioSource> ();
					audioSource.playOnAwake = false;
					Debug.LogFormat("loading weapon sound, bank index {0}", weaponSoundIndexes [modelId] );
					var audioClip = Audio.AudioManager.CreateAudioClipFromSfx ("GENRL", 136, 0, 
						Audio.AudioManager.SfxGENRL137Timings[ weaponSoundIndexes [modelId] ] );
					audioSource.clip = audioClip;
					weapon.m_audioSource = audioSource;
				}
			});

			weapon.InitWeapon();

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

		public static string ExtractAnimGroupName(string assocGroupId)
		{
			if( assocGroupId.EndsWith( "bad" ) )
				return assocGroupId.Substring( 0, assocGroupId.Length - 3 );
			if( assocGroupId.EndsWith( "pro" ) )
				return assocGroupId.Substring( 0, assocGroupId.Length - 3 );

			return assocGroupId;
		}


		protected virtual void Awake ()
		{
			this.GunFlash = this.transform.FindChildRecursive("gunflash");
		}

		/// <summary>
		/// Called after creating a weapon and assigning it's parameters, such are WeaponData and HUD texture.
		/// Use this method to assign aim animations and timings, or initialize anything else related to weapon.
		/// </summary>
		protected virtual void InitWeapon()
		{
			// set default weapon anims and other params

			string animGroup = ExtractAnimGroupName( this.Data.gunData.AssocGroupId );

			this.CanCrouchAim = this.HasFlag( GunFlag.CROUCHFIRE );

			if( this.HasFlag( GunFlag.CROUCHFIRE ) )
				this.CrouchAimAnim = new AnimId( animGroup, animGroup + "_crouchfire" );
			else
				this.CrouchAimAnim = new AnimId( "RIFLE", "RIFLE_crouchfire" );
			
			this.CrouchAimAnimMaxTime = WeaponsManager.ConvertAnimTime (this.Data.gunData.animLoop2Start);
			this.CrouchAimAnimFireMaxTime = WeaponsManager.ConvertAnimTime (this.Data.gunData.animLoop2End);

			this.NeckRotationOffset = WeaponsSettings.neckRotationOffset;

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

		public bool IsHeavy { get { return this.HasFlag (GunFlag.HEAVY); } }

		public bool CanCrouchAim { get; set; }

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

			if (ped.IsRunOn) {

				return this.RunAnim;

			} else if (ped.IsWalkOn) {

				return this.WalkAnim;

			} else if (ped.IsSprintOn) {

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

		public AnimId CrouchAimAnim { get; set; }

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

		public float CrouchAimAnimMaxTime { get; set; }

		public float CrouchAimAnimFireMaxTime { get; set; }

		public virtual float GunFlashDuration {
			get {
				return Weapons.WeaponsManager.Instance.GunFlashDuration;
			}
		}

		public Vector3 NeckRotationOffset { get; set; }


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

		// TODO: this function should be removed, and new one should be created: OnAnimsUpdated
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

		public virtual void PlayFireSound ()
		{

			if (m_audioSource && m_audioSource.clip)
			{
				if(!m_audioSource.isPlaying)
				{
//						int bankIndex = weaponSoundIndexes [this.Definition.Id];

//						Audio.AudioManager.SfxGENRL137Timings [bankIndex];

//						int startTimeMs = 0;
//						int endTimeMs = 0;
//
//						float startTime = startTimeMs / 1000f;
//						float endTime = endTimeMs / 1000f;

//						Debug.LogFormat("playing weapon sound, start time {0}, end time {1}, bank index {2}", startTime, endTime, bankIndex);

//						m_audioSource.Stop();
//						m_audioSource.time = startTime;
//						m_audioSource.Play();
//						m_audioSource.SetScheduledEndTime( AudioSettings.dspTime + (endTime - startTime) );

				//	Debug.LogFormat("playing weapon sound");
					m_audioSource.Stop();
					m_audioSource.time = 0f;
					m_audioSource.Play();

				}
			}

		}

		public virtual void FireProjectile ()
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
