using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Weapons;
using UGameCore.Utilities;
using SanAndreasUnity.Behaviours.Weapons;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using SanAndreasUnity.Importing.Animation;
using System.Reflection;
using SanAndreasUnity.Net;

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

		public static readonly int RocketProjectile = 345;

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

    public struct WeaponAttackParams
    {
		public List<int> LayersToIgnoreWhenRaycasting { get; set; }
		public List<GameObject> GameObjectsToIgnoreWhenRaycasting { get; set; }

		public static WeaponAttackParams Default { get => new WeaponAttackParams(); }
    }

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
		public int AmmoInClip { get => m_netWeapon.AmmoInClip; set => m_netWeapon.AmmoInClip = value; }
		public int AmmoOutsideOfClip { get => m_netWeapon.AmmoOutsideOfClip; set => m_netWeapon.AmmoOutsideOfClip = value; }
		public int TotalAmmo { get { return this.AmmoInClip + this.AmmoOutsideOfClip; } }

		public Texture2D HudTexture { get; private set; }

		protected Ped m_ped => this.PedOwner;
		public Ped PedOwner { get { return m_netWeapon.PedOwner; } internal set { m_netWeapon.PedOwner = value; } }


		private static List<System.Type> s_weaponTypes = new List<System.Type> ();

		protected WeaponsManager WeaponsSettings { get { return WeaponsManager.Instance; } }

		private static GameObject s_weaponsContainer = null;

		public static Texture2D CrosshairTexture { get; set; }
		public static Texture2D RocketCrosshairTexture { get; set; }
		public static Texture2D FistTexture { get; set; }

		public AnimationState AimAnimState { get; set; }
		public Transform GunFlash { get; private set; }

        public double LastTimeWhenFired { get; protected set; } = double.NegativeInfinity;
        public double TimeSinceFired => Time.timeAsDouble - this.LastTimeWhenFired;


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

		static Dictionary<int, AudioClip> s_loadedAudioClips = new Dictionary<int, AudioClip>();

		// used to play weapon sound
		AudioSource m_audioSource;

		NetworkedWeapon m_netWeapon;
		public NetworkedWeapon NetWeapon => m_netWeapon;

        // "size of the array determines how many raycasts will occur" - I don't think this is true
        static readonly RaycastHit[] s_raycastHitBuffer = new RaycastHit[100];

        WeaponAttackParams m_lastRaycastWeaponAttackParams = WeaponAttackParams.Default;

        static readonly HashSet<int> s_weaponsUsingProjectile = new HashSet<int>()
        {
	        WeaponId.RocketLauncher,
	        WeaponId.RocketLauncherHS,
        };

        private static Geometry.GeometryParts s_projectileModel;
        public static Geometry.GeometryParts ProjectileModel
        {
	        get
	        {
		        if (s_projectileModel != null)
			        return s_projectileModel;
		        F.RunExceptionSafe(LoadProjectileModel);
		        return s_projectileModel;
	        }
        }

        private static AudioClip s_projectileSound;
        public static AudioClip ProjectileSound
        {
	        get
	        {
		        if (s_projectileSound != null)
			        return s_projectileSound;
		        F.RunExceptionSafe(LoadProjectileAudio);
		        return s_projectileSound;
	        }
        }

		public class AttackConductedEventData
        {
			public Weapon Weapon { get; }
			public DamageInfo DamageInfo { get; }
			public Damageable Damageable { get; }

			public AttackConductedEventData(Weapon weapon)
			{
				Weapon = weapon;
			}

            public AttackConductedEventData(Weapon weapon, DamageInfo damageInfo, Damageable damageable)
            {
				Weapon = weapon;
                DamageInfo = damageInfo;
                Damageable = damageable;
            }
        }

		public static event System.Action<AttackConductedEventData> onWeaponConductedAttack = delegate {};



        static Weapon ()
		{
			// obtain all weapon types
			var myType = typeof (Weapon);
			foreach (Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				s_weaponTypes.AddRange (a.GetTypes ().Where (t => t.IsSubclassOf (myType)));
			}

		}


		public static Weapon Create (int modelId, Ped initialPedOwner)
		{
			NetStatus.ThrowIfNotOnServer();

			WeaponDef def;
			WeaponData weaponData;
			GameObject go = CreatePart1(modelId, null, out def, out weaponData);
			if (null == go)
				return null;

			// asign syncvars before spawning it
			var networkedWeapon = go.GetComponentOrThrow<NetworkedWeapon>();
			networkedWeapon.ModelId = modelId;
			networkedWeapon.PedOwner = initialPedOwner;

			// spawn game object here
			NetManager.Spawn(go);

			Weapon weapon = CreatePart2(go, def, weaponData);

			if (initialPedOwner != null)
				initialPedOwner.WeaponHolder.AddWeapon(weapon);

			return weapon;
		}

		static GameObject CreatePart1(int modelId, GameObject go, out WeaponDef def, out WeaponData weaponData)
		{
			def = null;
			weaponData = null;

			def = Item.GetDefinition<WeaponDef> (modelId);
			if (null == def)
				return null;

			var defCopyRef = def;
			weaponData = WeaponData.LoadedWeaponsData.FirstOrDefault (wd => wd.modelId1 == defCopyRef.Id);
			if (null == weaponData)
				return null;

			var geoms = Geometry.Load (def.ModelName, def.TextureDictionaryName);
			if (null == geoms)
				return null;

			if (null == s_weaponsContainer) {
				s_weaponsContainer = new GameObject ("Weapons");
			//	weaponsContainer.SetActive (false);
			}

			if (null == go)
				go = Object.Instantiate(WeaponsManager.Instance.weaponPrefab);
			go.name = def.ModelName;
			go.transform.SetParent (s_weaponsContainer.transform);

			geoms.AttachFrames (go.transform, MaterialFlags.Default);

			return go;
		}

		static Weapon CreatePart2(GameObject go, WeaponDef def, WeaponData weaponData)
		{

			int modelId = def.Id;

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

					AudioClip audioClip = null;
					if (s_loadedAudioClips.ContainsKey(modelId))
					{
						audioClip = s_loadedAudioClips[modelId];
					}
					else
					{
						audioClip = Audio.AudioManager.CreateAudioClipFromSfx ("GENRL", 136, weaponSoundIndexes [modelId]);
						s_loadedAudioClips[modelId] = audioClip;
					}
					
					audioSource.clip = audioClip;
					weapon.m_audioSource = audioSource;
				}
			});

			weapon.InitWeapon();

			return weapon;
		}

		static void LoadProjectileModel()
		{
			if (s_projectileModel != null)
				return;

			var def = Item.GetDefinition<WeaponDef>(WeaponId.RocketProjectile);
			s_projectileModel = Geometry.Load (def.ModelName, def.TextureDictionaryName);
		}

		static void LoadProjectileAudio()
		{
			if (null == s_projectileSound)
			{
				s_projectileSound = Audio.AudioManager.CreateAudioClipFromSfx("GENRL", 136, 68);
			}
		}

		internal static void OnWeaponCreatedByServer(NetworkedWeapon networkedWeapon)
		{

			WeaponDef def;
			WeaponData weaponData;
			GameObject go = CreatePart1(networkedWeapon.ModelId, networkedWeapon.gameObject, out def, out weaponData);
			if (null == go)
				return;

			Weapon weapon = CreatePart2(go, def, weaponData);

			weapon.AssignGunFlashTransform();

			if (weapon.PedOwner != null)
				weapon.PedOwner.WeaponHolder.AddWeapon(weapon);

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
			m_netWeapon = this.GetComponentOrThrow<NetworkedWeapon>();
			this.AssignGunFlashTransform();
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

			this.CrouchSpineRotationOffset = WeaponsSettings.crouchSpineRotationOffset;

			this.FiresProjectile = s_weaponsUsingProjectile.Contains(this.Data.modelId1);
			if (this.FiresProjectile)
			{
				this.ProjectilePrefab = WeaponsSettings.projectilePrefab;
				this.ReloadTime = WeaponsSettings.projectileReloadTime;
				F.RunExceptionSafe(LoadProjectileModel);
				F.RunExceptionSafe(LoadProjectileAudio);
			}

		}

		protected virtual void Start ()
		{

		}

		protected virtual void Update ()
		{
			
			if (WeaponsSettings.drawLineFromGun)
			{
				if (m_ped != null && m_ped.CurrentWeapon == this)
				{
					Vector3 start, end;
					this.GetLineFromGun (out start, out end, m_lastRaycastWeaponAttackParams);
					GLDebug.DrawLine (start, end, Color.red, 0, true);
				}
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

		public Vector3 CrouchSpineRotationOffset { get; set; }

		/// <summary>
		/// True if weapon doesn't inflict damage instantly, but instead fires a projectile.
		/// </summary>
		public bool FiresProjectile { get; set; } = false;

		public GameObject ProjectilePrefab { get; set; }

		public float ReloadTime { get; set; } = 0;

		public void AddRandomAmmoAmount()
		{
			if (!NetStatus.IsServer)
				return;

			Weapon weapon = this;

			weapon.AmmoInClip = weapon.AmmoClipSize;
			weapon.AmmoOutsideOfClip += weapon.AmmoClipSize * Random.Range( 0, 11 );
			weapon.AmmoOutsideOfClip += Random.Range (50, 200);
		}


		void AssignGunFlashTransform()
		{
			this.GunFlash = this.transform.FindChildRecursive("gunflash");
		}

		public virtual void EnableOrDisableGunFlash ()
		{
			
			if (null == this.GunFlash)
				return;

			// enable/disable gun flash
			
			Ped ped = m_ped;

			bool shouldBeVisible = false;

			if (ped != null && ped.IsFiring)
			{
				if (AimAnimState != null && AimAnimState.enabled) {
					// aim anim is being played

					if (AimAnimState.time.BetweenExclusive (this.AimAnimMaxTime, this.AimAnimMaxTime + this.GunFlashDuration)) {
						// muzzle flash should be visible
						shouldBeVisible = true;
					}
				}
			}

			this.GunFlash.gameObject.SetActive (shouldBeVisible);
			
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

		public void FireProjectile (WeaponAttackParams parameters)
		{
			this.FireProjectile(this.GetFirePos(), this.GetFireDir(parameters), parameters);
		}

		public virtual void FireProjectile (Vector3 firePos, Vector3 fireDir, WeaponAttackParams parameters)
		{

            this.LastTimeWhenFired = Time.timeAsDouble;

			F.RunExceptionSafe(() => this.PlayFireSound());

			if (this.FiresProjectile)
            {
	            Projectile.Create(this.ProjectilePrefab, firePos, Quaternion.LookRotation(fireDir), m_ped);
				F.InvokeEventExceptionSafe(onWeaponConductedAttack, new AttackConductedEventData(this));
	            return;
            }

			var damagable = this.ProjectileRaycastForDamagable(
				firePos, fireDir, parameters, out bool attackWillBeConducted, out DamageInfo damageInfo);

			if (attackWillBeConducted && damagable != null)
				damagable.Damage(damageInfo);

			if (attackWillBeConducted)
				F.InvokeEventExceptionSafe(onWeaponConductedAttack, new AttackConductedEventData(this, damageInfo, damagable));

		}

		public bool ProjectileRaycast (Vector3 source, Vector3 dir, out RaycastHit hit, WeaponAttackParams parameters)
		{
            m_lastRaycastWeaponAttackParams = parameters;


            if (null == parameters.GameObjectsToIgnoreWhenRaycasting)
                return Physics.Raycast(source, dir, out hit, this.MaxRange, WeaponsManager.Instance.projectileRaycastMask);


			int numHits = Physics.RaycastNonAlloc (source, dir, s_raycastHitBuffer, this.MaxRange, WeaponsManager.Instance.projectileRaycastMask);

            if (numHits < 1)
            {
                hit = new RaycastHit();
                return false;
            }

            // "Note that the order of the results is undefined"
            // - that's why we need to search for closest hit

            var validHits = s_raycastHitBuffer
                .Take(numHits)
				.Where(h => h.collider != null && !ShouldIgnoreObjectWhenRaycasting(h.collider, parameters));

            if (!validHits.Any())
            {
                hit = new RaycastHit();
                return false;
            }

            hit = validHits.Aggregate((h1, h2) => h1.distance <= h2.distance ? h1 : h2);

            return true;
        }

		private static bool ShouldIgnoreObjectWhenRaycasting(Collider collider, WeaponAttackParams parameters)
		{
			if (!parameters.LayersToIgnoreWhenRaycasting.Contains(collider.gameObject.layer))
				return false;

			if (parameters.GameObjectsToIgnoreWhenRaycasting.Exists(go => go == collider.gameObject))
				return true;

			if (parameters.GameObjectsToIgnoreWhenRaycasting.Exists(go => go.transform.IsParentOf(collider.transform)))
				return true;

			return false;
		}

		public Damageable ProjectileRaycastForDamagable(
			Vector3 firePos,
			Vector3 fireDir,
			WeaponAttackParams parameters,
			out bool attackWillBeConducted,
			out DamageInfo damageInfo)
        {
			// 'attackWillBeConducted' can be false if target is owner ped, or the angle to target is big (eg. > 45 degrees)

			attackWillBeConducted = true;
			damageInfo = null;

			RaycastHit hit;
			if (this.ProjectileRaycast(firePos, fireDir, out hit, parameters))
			{
				// find Damagable script on target object

				var damageable = hit.collider.gameObject.GetComponentInParent<Damageable>();
				if (damageable != null)
				{
					// ray hit something that can be damaged
					
					// first check if target object is owner ped
					if (m_ped != null && m_ped.gameObject == damageable.gameObject)
					{
						// ray hit owner ped
						// attack should not be conducted

						attackWillBeConducted = false;
					}
					else
					{
						damageInfo = new DamageInfo()
						{
							amount = this.Damage,
							raycastHitTransform = hit.collider.transform,
							hitDirection = fireDir,
							hitPoint = hit.point,
							hitNormal = hit.normal,
							attacker = m_ped,
							attackingPlayer = m_ped != null ? m_ped.PlayerOwner : null,
							damageType = DamageType.Bullet,
						};
					}

				}

				return damageable;
			}

			return null;
		}


		public void GetLineFromGun (out Vector3 start, out Vector3 end, WeaponAttackParams parameters)
		{
			float distance = this.MaxRange;
			Vector3 firePos = this.GetFirePos ();
			Vector3 fireDir = this.GetFireDir (parameters);
			RaycastHit hit;
			if (this.ProjectileRaycast (firePos, fireDir, out hit, parameters))
			{
				distance = hit.distance;
			}

			start = firePos;
			end = firePos + fireDir * distance;
		}

        Vector3 GetFirePos()
        {
            return m_ped != null ? m_ped.FirePosition : this.GetFirePosWithoutPed();
        }

        public Vector3 GetFirePosWithoutPed ()
		{
			Vector3 firePos;

			if (this.GunFlash != null)
				firePos = this.GunFlash.transform.position;
			else if (this.Data.gunData != null)
				firePos = this.transform.TransformPoint (this.Data.gunData.fireOffset);
			else
				firePos = this.transform.position;

			return firePos;
		}

		Vector3 GetFireDir (WeaponAttackParams parameters)
		{
			if (m_ped != null)
			{
                return m_ped.FireDirection;
			}
			else
            {
                return this.GetFireDirWithoutPed();
            }
		}

        public Vector3 GetFireDirWithoutPed()
        {
            if (this.GunFlash)
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
			GizmosDrawCastedRay (firePos, this.GetFireDir (WeaponAttackParams.Default));

		}

		public void GizmosDrawCastedRay (Vector3 source, Vector3 dir)
		{
			float distance = 100f;
			RaycastHit hit;
			if (this.ProjectileRaycast (source, dir, out hit, WeaponAttackParams.Default))
			{
				distance = hit.distance;
			}

			Gizmos.DrawLine (source, source + dir * distance);
		}

	}

}
