using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Weapons;
using UnityEngine;
using System.Linq;

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

	//	public int totalAmmo = 0;
	//	public int ammoInClip = 0;

		public Texture2D HudTexture { get; private set; }


		private static GameObject weaponsContainer = null;

		public static Texture2D CrosshairTexture { get; set; }
		public static Texture2D FistTexture { get; set; }



		public static Weapon Load (int modelId)
		{
			WeaponDef def = Item.GetDefinition<WeaponDef> (modelId);
			if (null == def)
				return null;

			var geoms = Geometry.Load (def.ModelName, def.TextureDictionaryName);
			if (null == geoms)
				return null;

			if (null == weaponsContainer) {
				weaponsContainer = new GameObject ("Weapons");
			//	weaponsContainer.SetActive (false);
			}

			GameObject go = new GameObject (def.ModelName);
			go.transform.SetParent (weaponsContainer.transform);

			geoms.AttachFrames (go.transform, MaterialFlags.Default);

			Weapon weapon = go.AddComponent<Weapon> ();
			weapon.definition = def;
			weapon.data = WeaponData.LoadedWeaponsData.FirstOrDefault (wd => wd.modelId1 == def.Id);
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

		public bool HasFlag( GunFlag gunFlag ) {

			if (this.data != null && this.data.gunData != null)
				return this.data.gunData.HasFlag (gunFlag);

			return false;
		}

	}

}
