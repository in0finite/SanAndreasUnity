using System;
using System.Collections.Generic;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class MapObject : MonoBehaviour, IComparable<MapObject>
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
        private bool _isVisible;

        public Vector2 CellPos { get; private set; }

        public int RandomInt { get; private set; }

        internal float LoadOrder { get; private set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (_isVisible == value) return;

                _isVisible = value;
                gameObject.SetActive(value);
            }
        }

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

            IsVisible = false;
            gameObject.isStatic = true;
        }

        internal void FindLodChild()
        {
            if (Instance.LodInstance == null)  return;

            LodChild = Instance.LodInstance.MapObject;
            if (LodChild == null) return;

            LodChild.LodParent = this;
        }

        public bool ShouldBeVisible(Vector3 from)
        {
            if (!_canLoad) return false;

            var obj = Instance.Object;
            var dist = Vector3.Distance(from, transform.position);

            return (dist <= obj.DrawDist || (obj.DrawDist >= 300 && dist < 1500))
                && (!_loaded || LodParent == null || !LodParent.IsVisible || !LodParent.ShouldBeVisible(from));
        }

        public bool RefreshLoadOrder(Vector3 from)
        {
            var visible = ShouldBeVisible(from);
            LoadOrder = float.PositiveInfinity;

            if (!IsVisible) {
                if (visible) LoadOrder = Vector3.Distance(from, transform.position);
                return visible;
            }

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
                    _canLoad = false;

                    Debug.LogWarningFormat("Failed to load {0} ({1})", Instance.ObjectId, e.Message);
                    name = string.Format("Failed ({0})", Instance.ObjectId);
                    return;
                }
            }

            IsVisible = LodParent == null || !LodParent.IsVisible;
            LoadOrder = float.PositiveInfinity;

            if (IsVisible && LodChild != null) {
                LodChild.Hide();
            }
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public int CompareTo(MapObject other)
        {
            return LoadOrder > other.LoadOrder ? 1 : LoadOrder == other.LoadOrder ? 0 : -1;
        }
    }
}
