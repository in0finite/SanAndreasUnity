using System.IO;
using System.Linq;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
using UnityEngine;
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
	//	public	bool	asynchronousLoad = false;

        public static bool HasLoaded { get; private set; }
		public	static	string	loadingStatusString {
			get { 
				string[] statuses = { "archive paths", "archive", "collision files", "item info",
					"handling info", "animation group info", "car colors" };
			
				return "Loading " + statuses [loadingStatus];
			}
		}
		private	static	int	loadingStatus = 0 ;
		private	static	string[]	archivePaths;
		private	static	IArchive[] archives;


		static	Loader() {
			// called on editor startup
			// start loading game data

		//	EditorApplication.update += StaticUpdate;

		}

        private void Awake()
        {
            if (HasLoaded) return;
			
			/*
			var archivePaths = Config.GetPaths ("archive_paths");

			IArchive[] archives;
			using (Utilities.Profiler.Start("Archive load time")) {
				archives = archivePaths.Select(x => 
					File.Exists(x) ? (IArchive) ArchiveManager.LoadImageArchive(x)
					: Directory.Exists(x) ? ArchiveManager.LoadLooseArchive(x)
					: null).Where(x => x != null).ToArray();
			}

			using (Utilities.Profiler.Start("Collision load time")) {
				foreach (var archive in archives) {
					foreach (var colFile in archive.GetFileNamesWithExtension(".col")) {
						CollisionFile.Load(colFile);
					}
				}
			}

			using (Utilities.Profiler.Start("Item info load time")) {
				foreach (var path in Config.GetPaths("item_paths")) {
					var ext = Path.GetExtension(path).ToLower();
					switch (ext) {
					case ".dat":
						Item.ReadLoadList(path); break;
					case ".ide":
						Item.ReadIde(path); break;
					case ".ipl":
						Item.ReadIpl(path); break;
					}
				}
			}

			using (Utilities.Profiler.Start("Handling info load time")) {
				Handling.Load(Config.GetPath("handling_path"));
			}

			using (Utilities.Profiler.Start("Animation group info load time")) {
				foreach (var path in Config.GetPaths("anim_groups_paths")) {
					AnimationGroup.Load(path);
				}
			}

			using (Utilities.Profiler.Start("Car color info load time")) {
				CarColors.Load(Config.GetPath("car_colors_path"));
			}

			HasLoaded = true;
			*/

        }

		static	void StaticUpdate() {

			#if UNITY_EDITOR
			if (!EditorApplication.isPlaying && !HasLoaded) {
				// display loading progress in editor

			}
			#endif

			if (HasLoaded)
				return;


			switch (loadingStatus) {

			case 0:

				Debug.Log ("Started loading GTA.");

				archivePaths = Config.GetPaths ("archive_paths");

				break;
			case 1:
				
				using (Utilities.Profiler.Start ("Archive load time")) {
					
					List<IArchive> listArchives = new List<IArchive> ();
					foreach( var path in archivePaths) {
						if(File.Exists(path)) {
							listArchives.Add (ArchiveManager.LoadImageArchive (path));
						} else if( Directory.Exists (path) ) {
							listArchives.Add (ArchiveManager.LoadLooseArchive (path));
						} else {
							Debug.Log("Archive not found: " + path);	
						}
					}
					archives = listArchives.FindAll (a => a != null).ToArray ();

				}

				break;
			case 2:

				using (Utilities.Profiler.Start ("Collision load time")) {
					int numCollisionFiles = 0;
					foreach (var archive in archives) {
						foreach (var colFile in archive.GetFileNamesWithExtension(".col")) {
							CollisionFile.Load (colFile);
							numCollisionFiles++;
						}
					}
					Debug.Log ("Number of collision files " + numCollisionFiles);
				}

				break;
			case 3:

				using (Utilities.Profiler.Start ("Item info load time")) {
					foreach (var path in Config.GetPaths("item_paths")) {
						var ext = Path.GetExtension (path).ToLower ();
						switch (ext) {
						case ".dat":
							Item.ReadLoadList (path);
							break;
						case ".ide":
							Item.ReadIde (path);
							break;
						case ".ipl":
							Item.ReadIpl (path);
							break;
						}
					}
				}

				break;
			case 4:

				using (Utilities.Profiler.Start ("Handling info load time")) {
					Handling.Load (Config.GetPath ("handling_path"));
				}

				break;
			case 5:

				using (Utilities.Profiler.Start ("Animation group info load time")) {
					foreach (var path in Config.GetPaths("anim_groups_paths")) {
						AnimationGroup.Load (path);
					}
				}

				break;
			case 6:

				using (Utilities.Profiler.Start ("Car color info load time")) {
					CarColors.Load (Config.GetPath ("car_colors_path"));
				}

				HasLoaded = true;

				Debug.Log ("GTA loading finished.");

				break;

			}

			loadingStatus++;


		}

		void Update() {

			StaticUpdate ();

		}

		void	OnGUI() {

			if (HasLoaded)
				return;

			// display loading progress

			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.Label ("<size=25>" + loadingStatusString + "</size>");
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

		}


	}

}
