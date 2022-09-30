using System.Collections.Generic;
using System.IO;
using System.Linq;
using UGameCore.Utilities;

namespace SanAndreasUnity.Importing.Weapons
{
	public class WeaponData
	{

		// common for all weapons

		public readonly string firstChar;
		public readonly string weaponType;
		public readonly string eFireType;
		public readonly float targetRange, weaponRange;
		public readonly int modelId1, modelId2;
	//	int reloadSampleTime1, reloadSampleTime2;
		public readonly int weaponslot;

		// for gun weapons only
		public readonly GunData gunData;


		public class GunData
		{
			public readonly string AssocGroupId;

			public readonly int ammoClip;

			public readonly int damage;

			public readonly UnityEngine.Vector3 fireOffset;

			public readonly int skillLevel;// 	0:POOR	1:STD	2:PRO
			public readonly int reqStatLevel; // req stat level to get this weapon skill level

			public readonly float accuracy;// (0.5 - 2.0f)
			public readonly float moveSpeed;// (0.5 - 1.5)

			public readonly int animLoopStart, animLoopEnd, animLoopFire;
			public readonly int animLoop2Start, animLoop2End, animLoop2Fire;	// for crouching

			public readonly int breakoutTime;

			public readonly string hexFlags;

			private List<GunFlag> m_flags = new List<GunFlag> (0);
			public IEnumerable<GunFlag> Flags { get { return m_flags; } }

			// old_shot_data

			public readonly float speed, radius;
			public readonly float lifespan, spread;


			public GunData(string[] parts) {

				LoadWithReflection (parts, this, "m_flags");

				// calculate flags
				var enumValues = System.Enum.GetValues( typeof(GunFlag) );
				foreach(var enumValue in enumValues) {
					if( HasFlag( this.hexFlags, (GunFlag) enumValue) )
						m_flags.Add( (GunFlag) enumValue );
				}

			}

			public bool HasFlag (GunFlag flag)
			{
				return m_flags.Contains (flag);
			}

			public static bool HasFlag (string hexFlags, GunFlag flag) {
				// reverse hex flags
				hexFlags = new string( hexFlags.Reverse().ToArray() );

				// find index of flag
				int index = s_groupedFlags.FindIndex( grp => grp.Contains(flag) );
				if (index < 0)
					return false;

				// find char with this index
				if (index >= hexFlags.Length)
					return false;
				char c = hexFlags [index];

				int hex = ParseInt (c.ToString (), System.Globalization.NumberStyles.HexNumber);
				return (hex & ( 1 << s_groupedFlags [index].IndexOf (flag) )) != 0;
			}

			public GunAimingOffset aimingOffset {
				get { 
					return WeaponData.LoadedGunAimingOffsets.FirstOrDefault (offset => offset.animGroup == AssocGroupId);
				}
			}

		}

		private	static	GunFlag[][]	s_groupedFlags = new GunFlag[5][] {
			new GunFlag[]{ GunFlag.CANAIM, GunFlag.AIMWITHARM, GunFlag.FIRSTPERSON, GunFlag.ONLYFREEAIM },
			new GunFlag[]{ GunFlag.MOVEAIM, GunFlag.MOVEFIRE },
			new GunFlag[]{ GunFlag.THROW, GunFlag.HEAVY, GunFlag.CONTINUOUSFIRE, GunFlag.TWIN_PISTOL },
			new GunFlag[]{ GunFlag.RELOAD, GunFlag.CROUCHFIRE, GunFlag.RELOAD2START, GunFlag.LONG_RELOAD },
			new GunFlag[]{ GunFlag.SLOWSDWN, GunFlag.RANDSPEED, GunFlag.EXPANDS }
		};

		public class GunAimingOffset
		{

			public readonly string firstChar;

			public readonly string animGroup;

			public readonly float aimX, aimZ;
			public UnityEngine.Vector3 Aim { get { return new UnityEngine.Vector3 (this.aimX, 0, this.aimZ); } }

			public readonly float duckX, duckZ;

			public readonly int rloadA, rloadB;
			public readonly int crouchRLoadA, crouchRLoadB;


			public GunAimingOffset(string line)
			{
				LoadWithReflection (SplitLine (line), this);
			}

		}


		private	static	List<WeaponData>	m_loadedWeaponData = new List<WeaponData> ();
		public	static	IEnumerable<WeaponData>	LoadedWeaponsData { get { return m_loadedWeaponData; } }

		private	static	List<GunAimingOffset>	m_loadedGunAimingOffsets = new List<GunAimingOffset> ();
		public	static	IEnumerable<GunAimingOffset>	LoadedGunAimingOffsets { get { return m_loadedGunAimingOffsets; } }


		public static void Load (string path)
		{
			
			m_loadedWeaponData.Clear ();
			m_loadedGunAimingOffsets.Clear ();

			using (var reader = File.OpenText (path)) {
				string line;
				while ((line = reader.ReadLine ()) != null) {
					line = line.Trim ();

					if (line.Length == 0)
						continue;
					if (line.StartsWith ("#"))
						continue;

					// allow some lines to fail
					F.RunExceptionSafe(() =>
					{
						if (line.StartsWith ("$")) {
							// weapon
							m_loadedWeaponData.Add (new WeaponData (line));
						} else if (line.StartsWith ("%")) {
							// gun aiming offset
							m_loadedGunAimingOffsets.Add (new GunAimingOffset (line));
						}
					});

				}
			}

			UnityEngine.Debug.Log ("Loaded weapons data - " + m_loadedWeaponData.Count + " entries");

		}


		public WeaponData (string line) {

			var parts = SplitLine (line);

			int numEntriesUsed = LoadWithReflection (parts, this, "gunData");

			if (this.firstChar == "$") {
				// this weapon is gun - load gun data
				var gunParts = parts.Skip (numEntriesUsed).ToArray ();
				this.gunData = new GunData (gunParts);
			}

		}


		public static string[] SplitLine (string line)
		{
			return line.Split (new string[]{ "\t", " " }, System.StringSplitOptions.RemoveEmptyEntries);
		}

		public static int LoadWithReflection<T>(string[] parts, T obj, params string[] fieldsToIgnore) {
			
			var fields = typeof(T).GetFields (System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
				| System.Reflection.BindingFlags.Public);
			
		//	UnityEngine.Debug.Log ("Parsing weapons data line parts:\n" + string.Join("\n", parts));

			int partIndex = 0;

			foreach (var field in fields) {

				if (partIndex >= parts.Length)
					break;

				if (fieldsToIgnore.Contains (field.Name))
					continue;

				string part = parts [partIndex];

				if (field.FieldType == typeof(int)) {
					field.SetValue (obj, ParseInt (part));
				} else if (field.FieldType == typeof(float)) {
					field.SetValue (obj, ParseFloat (part));
				} else if (field.FieldType == typeof(string)) {
					field.SetValue (obj, part);
				} else if (field.FieldType == typeof(UnityEngine.Vector3)) {
					UnityEngine.Vector3 vec3 = new UnityEngine.Vector3 ();

					vec3.x = ParseFloat (part);
					partIndex++;

					part = parts [partIndex];
					vec3.y = ParseFloat (part);
					partIndex++;

					part = parts [partIndex];
					vec3.z = ParseFloat (part);

					field.SetValue (obj, vec3);
				}

			//	UnityEngine.Debug.LogFormat ("new value {0}, index {1}, field name {2}", field.GetValue (obj), partIndex,
			//		field.Name);

				partIndex++;
			}

			return partIndex;
		}

		private static float ParseFloat (string str)
		{
			return float.Parse( str, System.Globalization.CultureInfo.InvariantCulture );
		}

		private static int ParseInt (string str)
		{
			return int.Parse( str, System.Globalization.CultureInfo.InvariantCulture );
		}

		private static int ParseInt (string str, System.Globalization.NumberStyles numberStyles)
		{
			return int.Parse( str, numberStyles, System.Globalization.CultureInfo.InvariantCulture );
		}

	}

	public enum GunFlag
	{
		CANAIM, AIMWITHARM, FIRSTPERSON, ONLYFREEAIM,
		MOVEAIM, MOVEFIRE,
		THROW, HEAVY, CONTINUOUSFIRE, TWIN_PISTOL,
		RELOAD, CROUCHFIRE, RELOAD2START, LONG_RELOAD,
		SLOWSDWN, RANDSPEED, EXPANDS
	}

}
