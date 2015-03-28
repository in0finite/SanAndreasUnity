using System;
using System.Collections.Generic;
using SanAndreasUnity.Importing.Archive;
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
            mapObj.Initialize(inst);

            return mapObj;
        }

        protected Instance Instance { get; private set; }

        private bool _loaded;
        private bool _canLoad;

        public Vector2 CellPos { get; private set; }

        public int RandomInt { get; private set; }

        internal float LoadOrder { get; private set; }

        public MapObject LodParent;
        public MapObject LodChild;

        public void Initialize(Instance inst)
        {
            inst.MapObject = this;

            Instance = inst;
            Instance.Object = Instance.Object ?? Cell.GameData.GetObject(inst.ObjectId);

            transform.position = inst.Position;
            transform.localRotation = inst.Rotation;

            CellPos = new Vector2(inst.Position.x, inst.Position.z);

            _canLoad = Instance.Object != null;
            _loaded = false;

            RandomInt = _sRandom.Next();

            name = _canLoad ? Instance.Object.Geometry : string.Format("Unknown ({0})", Instance.ObjectId);

            gameObject.SetActive(false);
        }

        internal void FindLodChild()
        {
            if (Instance.LodInstance == null)  return;

            LodChild = Instance.LodInstance.MapObject;
            if (LodChild == null) return;

            LodChild.LodParent = this;
        }

        public bool IsVisible(Vector3 from)
        {
            var obj = Instance.Object;
            return (obj.DrawDist <= 0 || obj.HasFlag(ObjectFlag.DisableDrawDist) ||
                LoadOrder <= obj.DrawDist) && (LodParent == null || !LodParent.IsVisible(from));
        }

        public bool ShouldShow(Vector3 from)
        {
            if (!_canLoad) return false;
            
            LoadOrder = Vector3.Distance(from, transform.position);

            var visible = IsVisible(from);

            if (!isActiveAndEnabled) return visible;
            if (!visible) Hide();

            return false;
        }

        public void Show()
        {
            if (!_canLoad) return;

            if (!_loaded) {
                try {
                    _loaded = true;

                    Mesh mesh;
                    Material[] materials;

                    Geometry.Load(Instance.Object.Geometry, Instance.Object.TextureDictionary,
                        Instance.Object.Flags, out mesh, out materials);

                    var mf = gameObject.AddComponent<MeshFilter>();
                    var mr = gameObject.AddComponent<MeshRenderer>();

                    mf.mesh = mesh;
                    mr.materials = materials;
                } catch (Exception e) {
                    Debug.LogWarningFormat("Failed to load {0} ({1})", Instance.ObjectId, e.Message);
                    name = string.Format("Failed ({0})", Instance.ObjectId);
                    return;
                }
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
