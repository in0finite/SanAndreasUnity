using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Items;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using ResourceManager = SanAndreasUnity.Importing.Archive.ResourceManager;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;

namespace SanAndreasUnity.Behaviours
{
    public class Cell : MonoBehaviour
    {
        public static GameData GameData { get; private set; }

        public Division RootDivision { get; private set; }

        public List<int> CellIds = new List<int> { 0, 13 };

        public PlayerController Player;

        void Awake()
        {
            var timer = new Stopwatch();

            if (GameData == null) {
                var archivePaths = new[] {
                    ResourceManager.GetPath("models", "gta3.img"),
                    ResourceManager.GetPath("models", "gta_int.img"),
                    ResourceManager.GetPath("models", "player.img")
                };

                timer.Start();
                var archives = archivePaths.Select(x => ResourceManager.LoadArchive(x)).ToArray();
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
                GameData = new GameData(ResourceManager.GetPath("data", "gta.dat"));
                timer.Stop();

                UnityEngine.Debug.LogFormat("Game Data load time: {0} ms", timer.Elapsed.TotalMilliseconds);
                timer.Reset();
            }

            RootDivision = Division.Create(transform);
            RootDivision.SetBounds(
                new Vector2(-3000f, -3000f),
                new Vector2(+3000f, +3000f));

            timer.Start();

            var objs = GameData.GetInstances(CellIds.ToArray()).ToDictionary(x => x, x => MapObject.Create());

            foreach (var obj in objs) {
                obj.Value.Initialize(obj.Key, objs);
            }

            RootDivision.AddRange(objs.Values);
            timer.Stop();

            UnityEngine.Debug.LogFormat("Cell partitioning time: {0} ms", timer.Elapsed.TotalMilliseconds);
            timer.Reset();

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
