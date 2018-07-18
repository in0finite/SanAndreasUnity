using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanAndreasUnity.Importing.Weapons
{
	public class WeaponData
	{

		// common for all weapons

		string firstChar;
		public readonly string weaponType;
		public readonly string eFireType;
		public readonly float targetRange, weaponRange;
		public readonly int modelId1, modelId2;
	//	int reloadSampleTime1, reloadSampleTime2;
		public readonly int weaponslot;

		GunData gunData;


		public class GunData
		{
			string AssocGroupId;

			int ammoClip;

			int damage;

			UnityEngine.Vector3 fireOffset;

			int skillLevel;// 	0:POOR	1:STD	2:PRO
			int reqStatLevel; // req stat level to get this weapon skill level

			float accuracy;// (0.5 - 2.0f)
			float moveSpeed;// (0.5 - 1.5)

			int animLoopStart, animLoopEnd, animLoopFire;
			int animLoop2Start, animLoop2End, animLoop2Fire;

			int breakoutTime;

			string hexFlags;

			// old_shot_data

			float speed, radius;
			float lifespan, spread;


		}

		public enum GunFlags
		{


		}

		private	static	List<WeaponData>	m_loadedWeaponData = new List<WeaponData> ();
		public	static	IEnumerable<WeaponData>	AllLoadedWeaponsData { get { return m_loadedWeaponData; } }



		public static void Load (string path)
		{
			m_loadedWeaponData.Clear ();

			using (var reader = File.OpenText (path)) {
				string line;
				while ((line = reader.ReadLine ()) != null) {
					line = line.Trim ();

					if (line.Length == 0)
						continue;
					if (line.StartsWith ("#"))
						continue;
					if (!line.StartsWith ("$"))
						continue;
					
					m_loadedWeaponData.Add (new WeaponData (line));
				}
			}

			UnityEngine.Debug.Log ("Loaded weapons data: \"" + path + "\" - " + m_loadedWeaponData.Count + " entries");

		}


		public WeaponData (string line) {

			var parts = line.Split (new string[]{"\t", " "}, System.StringSplitOptions.RemoveEmptyEntries);

			int numEntriesUsed = LoadWithReflection (parts, this, "gunData");

			if (this.firstChar == "$") {
				var gunParts = parts.Skip (numEntriesUsed).ToArray ();
				this.gunData = new GunData ();
				LoadWithReflection (gunParts, this.gunData);
			}

		}


		public static int LoadWithReflection<T>(string[] parts, T obj, params string[] fieldsToIgnore) {
			
			var fields = typeof(T).GetFields (System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
				| System.Reflection.BindingFlags.Public);
			
			//	if (fields.Length != parts.Length)
			//		throw new System.ArgumentException ("Error loading weapons data: number of fields is not equal to number of entries in a line");

			UnityEngine.Debug.Log ("Parsing weapons data line parts:\n" + string.Join("\n", parts));

			int partIndex = 0;

			foreach (var field in fields) {

				if (partIndex >= parts.Length)
					break;

				if (fieldsToIgnore.Contains (field.Name))
					continue;

				string part = parts [partIndex];

				if (field.FieldType == typeof(int)) {
					field.SetValue (obj, int.Parse (part));
				} else if (field.FieldType == typeof(float)) {
					field.SetValue (obj, float.Parse (part));
				} else if (field.FieldType == typeof(string)) {
					field.SetValue (obj, part);
				} else if (field.FieldType == typeof(UnityEngine.Vector3)) {
					UnityEngine.Vector3 vec3 = new UnityEngine.Vector3 ();

					vec3.x = float.Parse (part);
					partIndex++;

					part = parts [partIndex];
					vec3.y = float.Parse (part);
					partIndex++;

					part = parts [partIndex];
					vec3.z = float.Parse (part);

					field.SetValue (obj, vec3);
				}

				UnityEngine.Debug.LogFormat ("new value {0}, index {1}, field name {2}", field.GetValue (obj), partIndex,
					field.Name);

				partIndex++;
			}

			return partIndex;
		}

	}

}
