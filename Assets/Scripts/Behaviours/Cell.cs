using UnityEngine;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Archive;

namespace SanAndreasUnity.Behaviours
{
    public class Cell : MonoBehaviour
    {
        public GameData GameData { get; private set; }

        public int CellId;

        void Start()
        {
            ResourceManager.LoadArchive(ResourceManager.GetPath("models", "gta3.img"));
            ResourceManager.LoadArchive(ResourceManager.GetPath("models", "gta_int.img"));
            ResourceManager.LoadArchive(ResourceManager.GetPath("models", "player.img"));

            GameData = new GameData(ResourceManager.GetPath("data", "gta.dat"));

            Load();
        }

        private void Load()
        {
            foreach (var group in GameData.GetGroups()) {
                var parent = new GameObject(group.Substring(group.IndexOf('/') + 1));

                parent.transform.SetParent(transform);

                foreach (var raw in GameData.GetInstances(group)) {
                    if (raw.CellId != CellId) continue;

                    var obj = new GameObject();
                    obj.transform.parent = parent.transform;
                    obj.AddComponent<MapObject>().Initialize(this, raw);
                }
            }
        }
    }
}
