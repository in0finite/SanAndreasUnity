using System.IO;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace SanAndreasUnity.Behaviours
{
#if UNITY_EDITOR
    //	[ExecuteInEditMode]
    [InitializeOnLoad]
#endif
    public class Loader : MonoBehaviour
    {
        
		public static bool HasLoaded { get; private set; }

		public static string LoadingStatus { get; private set; }

        private static string[] archivePaths;
        private static IArchive[] archives;

        protected static FileBrowser m_fileBrowser;

		private	static	System.Diagnostics.Stopwatch	m_stopwatch = new System.Diagnostics.Stopwatch();

		public class LoadingStep
		{
			public IEnumerator Coroutine { get; set; }
			public System.Action LoadFunction { get; set; }
			public string Description { get; set; }
			public bool StopLoadingOnException { get; set; }

			public LoadingStep (System.Action loadFunction, string description, bool stopLoadingOnException = true)
			{
				this.LoadFunction = loadFunction;
				this.Description = description;
				this.StopLoadingOnException = stopLoadingOnException;
			}

			public LoadingStep (IEnumerator coroutine, string description, bool stopLoadingOnException = true)
			{
				this.Coroutine = coroutine;
				this.Description = description;
				this.StopLoadingOnException = stopLoadingOnException;
			}
			
		}

		private static List<LoadingStep> m_loadingSteps = new List<LoadingStep> ();



		void Start ()
		{

			AddLoadingSteps ();

			StartCoroutine (LoadCoroutine ());

		}

		private static void AddLoadingSteps ()
		{
			
			System.Action[] loadFunctions = new System.Action[] {
				StepGetPaths,
				StepLoadArchives,
				StepLoadCollision,
				StepLoadItemInfo,
				StepLoadHandling,
				StepLoadAnimGroups,
				StepLoadCarColors,
				StepLoadWeaponsData,
				StepLoadMap,
				StepLoadSpecialTextures
			};

			string[] descriptions = new string[]{
				"archive paths", "archive", "collision files", "item info",
				"handling", "animation groups", "car colors", "weapons data", "map", "special textures"
			};


			for (int i = 0; i < loadFunctions.Length; i++) {
				AddLoadingStep (new LoadingStep (loadFunctions [i], descriptions [i]));
			}

		}

		private static void AddLoadingStep (LoadingStep step)
		{
			m_loadingSteps.AddIfNotPresent (step);
		}


		private static IEnumerator LoadCoroutine ()
		{

			foreach (var step in m_loadingSteps) {

				// update description
				LoadingStatus = step.Description;
				yield return null;

				var en = step.Coroutine;

				if (en != null) {
					// this step uses coroutine

					bool hasNext = true;

					while (hasNext) {

						hasNext = false;
						try {
							hasNext = en.MoveNext ();
						} catch (System.Exception ex) {
							Debug.LogException (ex);
							if (step.StopLoadingOnException) {
								yield break;
							}
						}

						// update description
						LoadingStatus = step.Description;
						yield return null;

					}
				} else {
					// this step uses a function

					try {
						step.LoadFunction ();
					} catch(System.Exception ex) {
						Debug.LogException (ex);
						if (step.StopLoadingOnException) {
							yield break;
						}
					}
				}

				// step finished it's work

				yield return null;
			}

			// all steps finished loading

			HasLoaded = true;

			Debug.Log("GTA loading finished in " + m_stopwatch.Elapsed.TotalSeconds + " seconds");

		}


		private static void StepGetPaths ()
		{
			//Debug.Log("Checking if there is available a GTA SA path.");

			//DevProfiles.CheckDevProfiles(null);

			/*() =>
                    {
                        m_fileBrowser.Toggle();
                        return m_fileBrowser.GetPath();
                    }*/

			m_stopwatch.Start ();

			Debug.Log("Started loading GTA");

			archivePaths = Config.GetPaths("archive_paths");

		}

		private static void StepLoadArchives ()
		{
			
			using (Profiler.Start("Archive load time"))
			{
				List<IArchive> listArchives = new List<IArchive>();
				foreach (var path in archivePaths)
				{
					if (File.Exists(path))
					{
						listArchives.Add(ArchiveManager.LoadImageArchive(path));
					}
					else if (Directory.Exists(path))
					{
						listArchives.Add(ArchiveManager.LoadLooseArchive(path));
					}
					else
					{
						Debug.Log("Archive not found: " + path);
					}
				}
				archives = listArchives.FindAll(a => a != null).ToArray();
			}

		}

		private static void StepLoadCollision ()
		{
			using (Profiler.Start("Collision load time"))
			{
				int numCollisionFiles = 0;
				foreach (var archive in archives)
				{
					foreach (var colFile in archive.GetFileNamesWithExtension(".col"))
					{
						CollisionFile.Load(colFile);
						numCollisionFiles++;
					}
				}
				Debug.Log("Number of collision files " + numCollisionFiles);
			}
		}

		private static void StepLoadItemInfo ()
		{
			using (Profiler.Start("Item info load time"))
			{
				foreach (var path in Config.GetPaths("item_paths"))
				{
					var ext = Path.GetExtension(path).ToLower();
					switch (ext)
					{
					case ".dat":
						Item.ReadLoadList(path);
						break;

					case ".ide":
						Item.ReadIde(path);
						break;

					case ".ipl":
						Item.ReadIpl(path);
						break;
					}
				}
			}
		}

		private static void StepLoadHandling ()
		{
			using (Profiler.Start("Handling info load time"))
			{
				Handling.Load(Config.GetPath("handling_path"));
			}
		}

		private static void StepLoadAnimGroups ()
		{
			using (Profiler.Start("Animation group info load time"))
			{
				foreach (var path in Config.GetPaths("anim_groups_paths"))
				{
					AnimationGroup.Load(path);
				}
			}
		}

		private static void StepLoadCarColors ()
		{
			using (Profiler.Start("Car color info load time"))
			{
				CarColors.Load(Config.GetPath("car_colors_path"));
			}
		}

		private static void StepLoadWeaponsData ()
		{
			using (Profiler.Start("Weapons data load time"))
			{
				Importing.Weapons.WeaponData.Load(Config.GetPath("weapons_path"));
			}
		}

		private static void StepLoadMap ()
		{
			using (Profiler.Start ("Minimap load time")) {
				//MiniMap.loadTextures();
				MiniMap.AssingMinimap ();
			}
		}

		private static void StepLoadSpecialTextures ()
		{
			using (Profiler.Start("Special texture load time"))
			{

				// Load mouse cursor texture
				Texture2D mouse = TextureDictionary.Load("fronten_pc").GetDiffuse("mouse").Texture;
				Texture2D mouseFix = new Texture2D(mouse.width, mouse.height);

				for (int x = 0; x < mouse.width; x++)
					for (int y = 0; y < mouse.height; y++)
						mouseFix.SetPixel(x, mouse.height - y - 1, mouse.GetPixel(x, y));

				mouseFix.Apply();
				Cursor.SetCursor(mouseFix, Vector2.zero, CursorMode.Auto);

				// load crosshair texture
				Weapon.CrosshairTexture = TextureDictionary.Load("hud").GetDiffuse("siteM16").Texture;

				// fist texture
				Weapon.FistTexture = TextureDictionary.Load("hud").GetDiffuse("fist").Texture;

			}
		}



        private void Update()
        {
			
        }

        private void OnGUI()
        {
            if (HasLoaded)
                return;

            // display loading progress
            GUILayout.BeginArea(new Rect(10, 5, 400, 100));
			GUILayout.Label("<size=25>" + LoadingStatus + "</size>");
            GUILayout.EndArea();
        }

    }
}
