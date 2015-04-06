using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class Loader : MonoBehaviour
    {
        public static bool HasLoaded { get; private set; }

        private static string _gameDir;
        private static string GameDir { get { return _gameDir ?? (_gameDir = (string) Config.Get("game_dir")); } }

        private static string FormatPath(string path)
        {
            return path.Replace("${game_dir}", GameDir);
        }

        private enum LoadOrderItemType
        {
            LoadList,
            IDE,
            IPL,
            Handling
        }

        private void Awake()
        {
            if (HasLoaded) return;

            var timer = new Stopwatch();

            var archivePaths = Config.Get("archive_paths")
                .Select(x => FormatPath((string) x))
                .ToArray();

            timer.Start();
            var archives = archivePaths.Select(x => 
                File.Exists(x) ? (IArchive) ArchiveManager.LoadImageArchive(x)
                : Directory.Exists(x) ? ArchiveManager.LoadLooseArchive(x)
                : null).Where(x => x != null).ToArray();
            timer.Stop();

            UnityEngine.Debug.LogFormat("Archive load time: {0} ms", timer.Elapsed.TotalMilliseconds);
            timer.Reset();

            timer.Start();
            foreach (var archive in archives) {
                foreach (var colFile in archive.GetFileNamesWithExtension(".col")) {
                    CollisionFile.Load(colFile);
                }
            }
            timer.Stop();

            UnityEngine.Debug.LogFormat("Collision load time: {0} ms", timer.Elapsed.TotalMilliseconds);
            timer.Reset();

            timer.Start();

            foreach (var elem in Config.Get("load_order")) {
                var type = (LoadOrderItemType) Enum.Parse(typeof(LoadOrderItemType), (string) elem["type"]);
                var path = FormatPath((string) elem["path"]);

                switch (type) {
                    case LoadOrderItemType.LoadList:
                        GameData.ReadLoadList(path); break;
                    case LoadOrderItemType.IDE:
                        GameData.ReadIde(path); break;
                    case LoadOrderItemType.IPL:
                        GameData.ReadIpl(path); break;
                    case LoadOrderItemType.Handling:
                        Handling.Load(path); break;
                }
            }

            timer.Stop();

            UnityEngine.Debug.LogFormat("Game Data load time: {0} ms", timer.Elapsed.TotalMilliseconds);
            timer.Reset();

            HasLoaded = true;
        }
    }
}
