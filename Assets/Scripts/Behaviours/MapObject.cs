using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class MapObject : MonoBehaviour
    {
        private static readonly System.Random _sRandom = new System.Random();

        public static MapObject Create(Cell cell, Instance inst)
        {
            var obj = new GameObject();
            var mapObj = obj.AddComponent<MapObject>();
            mapObj.Initialize(cell, inst);

            return mapObj;
        }

        protected Cell Cell { get; private set; }
        protected Instance Instance { get; private set; }
        protected Importing.Items.Object Object { get; private set; }

        protected MapObject Lod { get; private set; }

        private bool _loaded;
        private bool _canLoad;

        public Vector2 CellPos { get; private set; }

        public int RandomInt { get; private set; }

        public void Initialize(Cell cell, Instance inst)
        {
            Cell = cell;
            Instance = inst;
            Object = Cell.GameData.GetObject(Instance.ObjectId);

            transform.position = inst.Position;
            transform.localRotation = inst.Rotation;

            CellPos = new Vector2(inst.Position.x, inst.Position.z);

            _canLoad = Object != null;
            _loaded = false;

            RandomInt = _sRandom.Next();

            name = _canLoad ? Object.Geometry : string.Format("Unknown ({0})", Instance.ObjectId);

            gameObject.SetActive(false);
        }

        public void Load()
        {
            if (_loaded || !_canLoad) return;
            
            Mesh mesh;
            Material[] materials;

            try {
                _loaded = true;

                Geometry.Load(Object.Geometry, Object.TextureDictionary, Object.Flags, out mesh, out materials);

                var mf = gameObject.AddComponent<MeshFilter>();
                var mr = gameObject.AddComponent<MeshRenderer>();

                mf.mesh = mesh;
                mr.materials = materials;

                gameObject.SetActive(true);
            } catch {
                name = string.Format("Failed ({0})", Instance.ObjectId);
            }
        }
    }
}
