using System.IO;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Conversion;
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
        public static bool HasLoaded { get; private set; }

        public static string loadingStatusString
        {
            get
            {
                string[] statuses = { "archive paths", "archive", "collision files", "item info",
                    "handling info", "animation group info", "car colors", "special textures" };

                return "Loading " + statuses[loadingStatus];
            }
        }

        private static int loadingStatus = 0;
        private static string[] archivePaths;
        private static IArchive[] archives;

        protected static FileBrowser m_fileBrowser;

        private static void StaticUpdate()
        {
            if (HasLoaded)
                return;

            /*m_fileBrowser = new FileBrowser(
                new Rect(100, 100, 600, 500),
                "Choose GTA Instalattion Path",
                null
            );*/

            switch (loadingStatus)
            {
                case 0:
                    Debug.Log("Checking if there is available a GTA SA path.");
                    //DevProfiles.CheckDevProfiles(null);

                    /*() =>
                    {
                        m_fileBrowser.Toggle();
                        return m_fileBrowser.GetPath();
                    }*/

                    Debug.Log("Started loading GTA.");
                    archivePaths = Config.GetPaths("archive_paths");
                    break;

                case 1:
                    using (Utilities.Profiler.Start("Archive load time"))
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
                    break;

                case 2:
                    using (Utilities.Profiler.Start("Collision load time"))
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
                    break;

                case 3:
                    using (Utilities.Profiler.Start("Item info load time"))
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
                    break;

                case 4:
                    using (Utilities.Profiler.Start("Handling info load time"))
                    {
                        Handling.Load(Config.GetPath("handling_path"));
                    }
                    break;

                case 5:
                    using (Utilities.Profiler.Start("Animation group info load time"))
                    {
                        foreach (var path in Config.GetPaths("anim_groups_paths"))
                        {
                            AnimationGroup.Load(path);
                        }
                    }
                    break;

                case 6:
                    using (Utilities.Profiler.Start("Car color info load time"))
                    {
                        CarColors.Load(Config.GetPath("car_colors_path"));
                    }
                    break;

                case 7:
                    using (Utilities.Profiler.Start("special texture load time"))
                    {
                        //MiniMap.loadTextures();
                        MiniMap.AssingMinimap();

                        // Load mouse cursor texture
                        Texture2D mouse = TextureDictionary.Load("fronten_pc").GetDiffuse("mouse").Texture;
                        Texture2D mouseFix = new Texture2D(mouse.width, mouse.height);
                        for (int x = 0; x < mouse.width; x++)
                        {
                            for (int y = 0; y < mouse.height; y++)
                            {
                                mouseFix.SetPixel(x, mouse.height - y - 1, mouse.GetPixel(x, y));
                            }
                        }
                        mouseFix.Apply();
                        Cursor.SetCursor(mouseFix, Vector2.zero, CursorMode.Auto);
                    }
                    HasLoaded = true;
                    Debug.Log("GTA loading finished.");
                    break;
            }

            loadingStatus++;
        }

        private void Update()
        {
            StaticUpdate();
        }

        private void OnGUI()
        {
            if (HasLoaded)
                return;

            // display loading progress
            GUILayout.BeginArea(new Rect(10, 5, 400, 100));
            GUILayout.Label("<size=25>" + loadingStatusString + "</size>");
            GUILayout.EndArea();
        }
    }
}