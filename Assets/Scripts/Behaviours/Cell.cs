using UnityEngine;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Archive;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
    public class Cell : MonoBehaviour
    {
        public static GameData GameData { get; private set; }

        public Division RootSplit { get; private set; }

        public int CellId;

        void Awake()
        {
            if (GameData == null) {
                ResourceManager.LoadArchive(ResourceManager.GetPath("models", "gta3.img"));
                ResourceManager.LoadArchive(ResourceManager.GetPath("models", "gta_int.img"));
                ResourceManager.LoadArchive(ResourceManager.GetPath("models", "player.img"));

                GameData = new GameData(ResourceManager.GetPath("data", "gta.dat"));
            }

            RootSplit = Division.Create(transform);
            RootSplit.SetBounds(
                new Vector2(float.NegativeInfinity, float.NegativeInfinity),
                new Vector2(float.PositiveInfinity, float.PositiveInfinity));

            RootSplit.AddRange(GameData.GetInstances(0).Select(x => MapObject.Create(this, x)));

            StartCoroutine(RootSplit.LoadAsync());
        }
    }
}
