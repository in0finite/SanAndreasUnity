using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Weapons;
using UnityEngine;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
	public enum WeaponSlot
	{
		Hand = 0,
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

		Count = 13
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

	public class Weapon
	{
		private WeaponDef definition = null;
		public WeaponDef Definition { get { return this.definition; } }

		private WeaponData data = null;
		public WeaponData Data { get { return this.data; } }


	//	public int totalAmmo = 0;
	//	public int ammoInClip = 0;

		private GameObject m_gameObject = null;
		public GameObject gameObject { get { return m_gameObject; } }

		private static GameObject weaponsContainer = null;


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

			Weapon w = new Weapon ();
			w.definition = def;
			w.data = WeaponData.AllLoadedWeaponsData.FirstOrDefault (wd => wd.modelId1 == def.Id);
			w.m_gameObject = go;

			return w;
		}

	}

}
