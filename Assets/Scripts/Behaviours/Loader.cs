using System.IO;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.GXT;

namespace SanAndreasUnity.Behaviours
{
	
    public class Loader : MonoBehaviour
    {
        
		public static bool HasLoaded { get; private set; }
		public static bool IsLoading { get; private set; }

		public static string LoadingStatus { get; private set; }

		private static int m_currentStepIndex = 0;

		private static float m_totalEstimatedLoadingTime = 0;

		private static bool m_hasErrors = false;
		private static System.Exception m_loadException;

		public class LoadingStep
		{
			public IEnumerator Coroutine { get; private set; }
			public System.Action LoadFunction { get; private set; }
			public string Description { get; set; }
			public bool StopLoadingOnException { get; private set; }
			public float TimeElapsed { get; internal set; }
			public float EstimatedTime { get; private set; }

			public LoadingStep (System.Action loadFunction, string description, float estimatedTime = 0f, bool stopLoadingOnException = true)
			{
				this.LoadFunction = loadFunction;
				this.Description = description;
				this.EstimatedTime = estimatedTime;
				this.StopLoadingOnException = stopLoadingOnException;
			}

			public LoadingStep (IEnumerator coroutine, string description, float estimatedTime = 0f, bool stopLoadingOnException = true)
			{
				this.Coroutine = coroutine;
				this.Description = description;
				this.EstimatedTime = estimatedTime;
				this.StopLoadingOnException = stopLoadingOnException;
			}
			
		}

		private static List<LoadingStep> m_loadingSteps = new List<LoadingStep> ();

		public static Texture2D CurrentSplashTex { get; set; }
		public static Texture2D SplashTex1 { get; set; }
		public static Texture2D SplashTex2 { get; set; }

		private static bool m_showFileBrowser = false;
		private static FileBrowser m_fileBrowser = null;

		public static event System.Action onLoadSpecialTextures = delegate { };



		void Start ()
		{

			AddLoadingSteps ();

			StartCoroutine (LoadCoroutine ());

		}

		private static void AddLoadingSteps ()
		{
			
			LoadingStep[] steps = new LoadingStep[] {
				new LoadingStep ( StepConfigure, "Configuring", 0f ),
				new LoadingStep ( StepSelectGTAPath(), "Select path to GTA", 0.0f ),
				new LoadingStep ( StepLoadArchives, "Loading archives", 1.7f ),
				new LoadingStep ( StepLoadSplashScreen, "Loading splash screen", 0.06f ),
				new LoadingStep ( StepSetSplash1, "Set splash 1" ),
				new LoadingStep ( StepLoadAudio, "Loading audio" ),
				//new LoadingStep ( StepLoadFonts,"Loading fonts"),
				new LoadingStep ( StepLoadCollision, "Loading collision files", 0.9f ),
				new LoadingStep ( StepLoadItemInfo, "Loading item info", 2.4f ),
				new LoadingStep ( StepLoadHandling, "Loading handling", 0.01f ),
				//new LoadingStep ( () => { throw new System.Exception ("testing error handling"); }, "testing error handling", 0.01f ),
				new LoadingStep ( StepLoadAnimGroups, "Loading animation groups", 0.02f ),
				new LoadingStep ( StepLoadCarColors, "Loading car colors", 0.04f ),
				new LoadingStep ( StepLoadWeaponsData, "Loading weapons data", 0.05f ),
				new LoadingStep ( StepSetSplash2, "Set splash 2" ),
				new LoadingStep ( StepLoadMap, "Loading map", 2.1f ),
				new LoadingStep ( StepLoadSpecialTextures, "Loading special textures", 0.01f ),
			//	new LoadingStep ( StepLoadGXT, "Loading GXT", 0.15f),
                new LoadingStep ( StepLoadPaths, "Loading paths"),
			};


			for (int i = 0; i < steps.Length; i++) {
				AddLoadingStep (steps [i]);
			}


			if (Cell.Instance != null) {
				// add steps for cell
				AddLoadingStep( new LoadingStep( () => Cell.Instance.CreateStaticGeometry (), "Creating static geometry", 5.8f ) );
				AddLoadingStep( new LoadingStep( () => Cell.Instance.InitStaticGeometry (), "Init static geometry", 0.35f ) );
				AddLoadingStep( new LoadingStep( () => Cell.Instance.LoadParkedVehicles (), "Loading parked vehicles", 0.2f ) );
				AddLoadingStep( new LoadingStep( () => Cell.Instance.CreateEnexes (), "Creating enexes", 0.1f ) );
				AddLoadingStep( new LoadingStep( () => Cell.Instance.LoadWater (), "Loading water", 0.08f ) );
				AddLoadingStep( new LoadingStep( () => Cell.Instance.FinalizeLoad (), "Finalize world loading", 0.01f ) );

			}

		}

		private static void AddLoadingStep (LoadingStep step)
		{
			m_loadingSteps.AddIfNotPresent (step);
		}


		private static IEnumerator LoadCoroutine ()
		{

			var stopwatch = System.Diagnostics.Stopwatch.StartNew ();

			IsLoading = true;

			Debug.Log("Started loading GTA");

			// wait a few frames - to "unblock" the program, and to let other scripts initialize before
			// registering their loading steps
			yield return null;
			yield return null;

			// calculate total loading time
			m_totalEstimatedLoadingTime = m_loadingSteps.Sum( step => step.EstimatedTime );

			var stopwatchForSteps = new System.Diagnostics.Stopwatch ();

			foreach (var step in m_loadingSteps)
			{

				// wait some more time before going to next step, because sometimes Unity does something
				// in the background at the end of a frame, eg. it updates Collider positions if you changed them
				yield return null;

				// update description
				LoadingStatus = step.Description;
				yield return null;

				stopwatchForSteps.Restart ();

				var en = step.Coroutine;

				if (en != null) {
					// this step uses coroutine

					bool hasNext = true;

					while (hasNext) {

						hasNext = false;
						try {
							hasNext = en.MoveNext ();
						} catch (System.Exception ex) {
							HandleExceptionDuringLoad (ex);
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
						HandleExceptionDuringLoad (ex);
						if (step.StopLoadingOnException) {
							yield break;
						}
					}
				}

				// step finished it's work

				step.TimeElapsed = stopwatchForSteps.ElapsedMilliseconds;

				m_currentStepIndex++;

				Debug.LogFormat ("{0} - finished in {1} ms", step.Description, step.TimeElapsed);
			}

			// all steps finished loading

			HasLoaded = true;
			IsLoading = false;

			Debug.Log("GTA loading finished in " + stopwatch.Elapsed.TotalSeconds + " seconds");

			// notify all scripts
			F.SendMessageToObjectsOfType<MonoBehaviour>( "OnLoaderFinished" );

		}

		private static void HandleExceptionDuringLoad (System.Exception ex)
		{
			m_hasErrors = true;
			m_loadException = ex;

			Debug.LogException (ex);
		}

		private static void StepConfigure ()
		{
			TextureDictionary.DontLoadTextures = Config.Get<bool>("dontLoadTextures");
		}

		private static IEnumerator StepSelectGTAPath ()
		{
			yield return null;

			string path = Config.GetPath(Config.const_game_dir);

			if (string.IsNullOrEmpty (path)) {
				// path is not set
				// show file browser to user to select path
				m_showFileBrowser = true;
			} else {
				yield break;
			}

			// wait until user selects a path
			while (m_showFileBrowser) {
				yield return null;
			}

			// refresh path
			path = Config.GetPath(Config.const_game_dir);

			if (string.IsNullOrEmpty (path)) {
				// path was not set
				throw new System.Exception ("Path to GTA was not set");
			}

		}

		private static void StepLoadArchives ()
		{

			string[] directoriesToCheck = { "models", "data" };

			foreach (string directoryToCheck in directoriesToCheck)
			{
				string[] caseVariations =
				{
					directoryToCheck,
					directoryToCheck.FirstCharToUpper(),
					directoryToCheck.ToUpperInvariant(),
				};

				if (caseVariations.All(d => !Directory.Exists(Path.Combine(Config.GamePath, d))))
					throw new System.Exception($"Game folder seems to be invalid - failed to find '{directoryToCheck}' folder inside game folder");
			}

			ArchiveManager.LoadLooseArchive(Config.GamePath);

			foreach (string imgFilePath in ArchiveManager.GetFilePathsFromLooseArchivesWithExtension(".img"))
			{
				ArchiveManager.LoadImageArchive(imgFilePath);
			}

			Debug.Log($"num archives loaded: {ArchiveManager.GetNumArchives()}, num entries loaded: {ArchiveManager.GetTotalNumLoadedEntries()}");

		}

		private static void StepLoadSplashScreen ()
		{
			var txd = TextureDictionary.Load ("LOADSCS");

			int index1 = Random.Range (1, 15);
			int index2 = Random.Range (1, 15);

			SplashTex1 = txd.GetDiffuse ("loadsc" + index1).Texture;
			SplashTex2 = txd.GetDiffuse ("loadsc" + index2).Texture;

		}

		private static void StepSetSplash1 ()
		{
			CurrentSplashTex = SplashTex1;
		}

		private static void StepSetSplash2 ()
		{
			CurrentSplashTex = SplashTex2;
		}

		private static void StepLoadAudio ()
		{
			Audio.AudioManager.InitFromLoader ();
		}

		private static void StepLoadFonts()
		{
			Importing.FontsImporter.LoadFonts();
		}

		private static void StepLoadCollision ()
		{
			
			int numCollisionFiles = 0;

			foreach (var colFile in ArchiveManager.GetFileNamesWithExtension(".col"))
			{
				CollisionFile.Load(colFile);
				numCollisionFiles++;
			}

			Debug.Log("Number of collision files " + numCollisionFiles);

		}

		private static void StepLoadItemInfo ()
		{
			
			foreach (var p in Config.GetPaths("item_paths"))
			{
				string path = ArchiveManager.PathToCaseSensitivePath(p);
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

		private static void StepLoadHandling ()
		{
			Handling.Load(ArchiveManager.PathToCaseSensitivePath(Config.GetPath("handling_path")));
		}

		private static void StepLoadAnimGroups ()
		{
			foreach (string fileName in Config.Get<string[]>("anim_group_files"))
			{
				AnimationGroup.Load(fileName);
			}

			// load custom anim groups from resources
			TextAsset textAsset = Resources.Load<TextAsset>("Data/auxanimgrp");
			AnimationGroup.LoadFromStreamReader( new StreamReader(new MemoryStream(textAsset.bytes)) );
			
		}

		private static void StepLoadCarColors ()
		{
			CarColors.Load(ArchiveManager.PathToCaseSensitivePath(Config.GetPath("car_colors_path")));
		}

		private static void StepLoadWeaponsData ()
		{
			Importing.Weapons.WeaponData.Load(ArchiveManager.PathToCaseSensitivePath(Config.GetPath("weapons_path")));
		}

		private static void StepLoadMap ()
		{
			MiniMap.Instance.Load ();
		}

		private static void StepLoadSpecialTextures ()
		{
			
			// Load mouse cursor texture
			F.RunExceptionSafe(() =>
			{
				Texture2D mouse = TextureDictionary.Load("fronten_pc").GetDiffuse("mouse",
					new TextureLoadParams(){makeNoLongerReadable = false}).Texture;
				Texture2D mouseFix = new Texture2D(mouse.width, mouse.height);

				for (int x = 0; x < mouse.width; x++)
					for (int y = 0; y < mouse.height; y++)
						mouseFix.SetPixel(x, mouse.height - y - 1, mouse.GetPixel(x, y));

				mouseFix.Apply();

				Cursor.SetCursor(mouseFix, Vector2.zero, CursorMode.Auto);
			});

			// fist texture
			Weapon.FistTexture = TextureDictionary.Load("hud").GetDiffuse("fist").Texture;

			
			onLoadSpecialTextures();

		}

		private static void StepLoadGXT()
		{
			GXT.Load();
		}

        private static void StepLoadPaths()
        {
            Assets.Scripts.Importing.Paths.NodeReader.StepLoadPaths();
        }


        public static float GetProgressPerc ()
		{
			if (m_currentStepIndex <= 0)
				return 0f;

			if (m_currentStepIndex >= m_loadingSteps.Count)
				return 1f;

			float estimatedTimePassed = 0f;
			for (int i = 0; i < m_currentStepIndex; i++) {
				estimatedTimePassed += m_loadingSteps [i].EstimatedTime;
			}

			return Mathf.Clamp01 (estimatedTimePassed / m_totalEstimatedLoadingTime);
		}


        private void Update()
        {
			
        }

        private void OnGUI()
        {
            if (HasLoaded)
                return;

			// background

			if (CurrentSplashTex != null) {
				GUIUtils.DrawTextureWithYFlipped (new Rect (0, 0, Screen.width, Screen.height), CurrentSplashTex);
			} else {
				GUIUtils.DrawRect (new Rect (0, 0, Screen.width, Screen.height), Color.black);
			}

            // display loading progress

			GUILayout.BeginArea(new Rect(10, 5, 400, Screen.height - 5));

			// current status
			GUILayout.Label("<size=25>" + LoadingStatus + "</size>");

			// progress bar
			GUILayout.Space (10);
			DisplayProgressBar ();

			// display error
			if (m_hasErrors) {
				GUILayout.Space (20);
				GUILayout.Label("<size=20>" + "The following exception occured during the current step:" + "</size>");
				GUILayout.TextArea( m_loadException.ToString () );
				GUILayout.Space (30);
				if (GUIUtils.ButtonWithCalculatedSize("Exit", 80, 30)) {
					GameManager.ExitApplication();
				}
				GUILayout.Space(5);
			}

			// display all steps
//			GUILayout.Space (10);
//			DisplayAllSteps ();

            GUILayout.EndArea();

			DisplayFileBrowser ();

        }

		private static void DisplayAllSteps ()
		{

			int i=0;
			foreach (var step in m_loadingSteps) {
				GUILayout.Label( step.Description + (m_currentStepIndex > i ? (" - " + step.TimeElapsed + " ms") : "") );
				i++;
			}

		}

		private static void DisplayProgressBar ()
		{
			float width = 200;
			float height = 12;

//			Rect rect = GUILayoutUtility.GetLastRect ();
//			rect.position += new Vector2 (0, rect.height);
//			rect.size = new Vector2 (width, height);

			Rect rect = GUILayoutUtility.GetRect( width, height );
			rect.width = width;

			float progressPerc = GetProgressPerc ();
			GUIUtils.DrawBar( rect, progressPerc, new Vector4(149, 185, 244, 255) / 256.0f, new Vector4(92, 147, 237, 255) / 256.0f, 2f );

		}

		private static void DisplayFileBrowser ()
		{
			if (!m_showFileBrowser)
				return;

			if (null == m_fileBrowser) {
				Rect rect = GUIUtils.GetCenteredRect (FileBrowser.GetRecommendedSize());

				m_fileBrowser = new FileBrowser(rect, "Select path to GTA", GUI.skin.window, (string path) => {
					m_showFileBrowser = false;
					Config.SetString (Config.const_game_dir, path);
					Config.SaveUserConfigSafe ();
				} );
				m_fileBrowser.BrowserType = FileBrowserType.Directory;
			}

			m_fileBrowser.OnGUI ();

		}

    }
}
