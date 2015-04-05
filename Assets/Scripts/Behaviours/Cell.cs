using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Items;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Items.Placements;
using System.IO;

namespace SanAndreasUnity.Behaviours
{
    public class Cell : MonoBehaviour
    {
        public static GameData GameData { get; private set; }

        public Division RootDivision { get; private set; }

        public List<int> CellIds = new List<int> { 0, 13 };

        public PlayerController Player;
        public Water Water;

        void Awake()
        {
            var timer = new Stopwatch();

            if (GameData == null) {
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
                GameData = new GameData(ArchiveManager.GetPath("data", "gta.dat"));
                GameData.ReadIde("data/vehicles.ide");
                timer.Stop();

                UnityEngine.Debug.LogFormat("Game Data load time: {0} ms", timer.Elapsed.TotalMilliseconds);
                timer.Reset();
            }

            RootDivision = Division.Create(transform);
            RootDivision.SetBounds(
                new Vector2(-3000f, -3000f),
                new Vector2(+3000f, +3000f));

            timer.Start();

            var insts = GameData.GetPlacements<Instance>(CellIds.ToArray())
                .ToDictionary(x => x, x => StaticGeometry.Create());

            foreach (var inst in insts) {
                inst.Value.Initialize(inst.Key, insts);
            }

            var cars = GameData.GetPlacements<ParkedVehicle>(CellIds.ToArray())
                .Select(x => VehicleSpawner.Create(x))
                .Cast<MapObject>()
                .ToArray();

            RootDivision.AddRange(insts.Values.Cast<MapObject>().Concat(cars));
            timer.Stop();

            UnityEngine.Debug.LogFormat("Cell partitioning time: {0} ms", timer.Elapsed.TotalMilliseconds);
            timer.Reset();

            if (Water != null) {
                Water.Initialize(new WaterFile(ArchiveManager.GetPath("data", "water.dat")));
            }

            StartCoroutine(LoadAsync());
        }

        IEnumerator LoadAsync()
        {
            var timer = new Stopwatch();
            var leaves = RootDivision.ToList();

            while (true) {
                var pos = Player.transform.position;
                var toLoad = leaves.Aggregate(false, (current, leaf) => current | leaf.RefreshLoadOrder(pos));

                if (toLoad) {
                    leaves.Sort();

                    timer.Reset();
                    timer.Start();

                    foreach (var div in leaves) {
                        if (float.IsPositiveInfinity(div.LoadOrder)) break;
                        if (!div.LoadWhile(() => timer.Elapsed.TotalSeconds < 1d / 60d)) break;
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
