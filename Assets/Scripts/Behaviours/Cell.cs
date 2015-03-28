using UnityEngine;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Archive;
using System.Linq;
using System.Diagnostics;
using System.Collections;

namespace SanAndreasUnity.Behaviours
{
    public class Cell : MonoBehaviour
    {
        public static GameData GameData { get; private set; }

        public Division RootDivision { get; private set; }

        public int CellId;

        void Awake()
        {
            var timer = new Stopwatch();

            if (GameData == null) {
                timer.Start();
                ResourceManager.LoadArchive(ResourceManager.GetPath("models", "gta3.img"));
                ResourceManager.LoadArchive(ResourceManager.GetPath("models", "gta_int.img"));
                ResourceManager.LoadArchive(ResourceManager.GetPath("models", "player.img"));
                timer.Stop();

                UnityEngine.Debug.LogFormat("Archive load time: {0} ms", timer.Elapsed.TotalMilliseconds);
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
            RootDivision.AddRange(GameData.GetInstances(0).Select(x => MapObject.Create(this, x)));
            timer.Stop();

            UnityEngine.Debug.LogFormat("Cell partitioning time: {0} ms", timer.Elapsed.TotalMilliseconds);
            timer.Reset();

            StartCoroutine(LoadAsync());
        }

        IEnumerator LoadAsync()
        {
            var timer = new Stopwatch();

            while (true) {
                var pos = Camera.main.transform.position;
                var order = RootDivision
                    .Where(x => x.RefreshLoadOrder(pos))
                    .OrderBy(x => x.LoadOrder)
                    .ToArray();

                timer.Reset();
                timer.Start();

                foreach (var div in order) {
                    if (!div.LoadWhile(() => timer.Elapsed.TotalSeconds < 1d / 60d)) break;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
