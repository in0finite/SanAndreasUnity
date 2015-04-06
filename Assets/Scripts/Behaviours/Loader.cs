using System.Diagnostics;
using System.IO;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Vehicles;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class Loader : MonoBehaviour
    {
        private void Awake()
        {
            if (GameData.HasLoaded) return;

            var timer = new Stopwatch();

            var archivePaths = new[] {
                ArchiveManager.GameDir,
                ArchiveManager.GetPath("models", "gta3.img"),
                ArchiveManager.GetPath("models", "gta_int.img"),
                ArchiveManager.GetPath("models", "player.img")
            };

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
            GameData.Load(ArchiveManager.GetPath("data", "gta.dat"));
            GameData.ReadIde("data/vehicles.ide");
            GameData.ReadIde("data/peds.ide");

            Handling.Load(ArchiveManager.GetPath("data", "handling.cfg"));
            timer.Stop();

            UnityEngine.Debug.LogFormat("Game Data load time: {0} ms", timer.Elapsed.TotalMilliseconds);
            timer.Reset();
        }
    }
}
