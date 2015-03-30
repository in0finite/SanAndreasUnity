using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class MapObject : MonoBehaviour, IComparable<MapObject>
    {
        private static Texture2D _sNoiseTex;

        private static bool ShouldGenerateNoiseTex
        {
            get { return _sNoiseTex == null || _sNoiseTex.width != Screen.width || _sNoiseTex.height != Screen.height; }
        }

        private static int _sNoiseTexId = -1;
        protected static int NoiseTexId
        {
            get { return _sNoiseTexId == -1 ? _sNoiseTexId = Shader.PropertyToID("_NoiseTex") : _sNoiseTexId; }
        }

        private static int _sFadeId = -1;
        protected static int FadeId
        {
            get { return _sFadeId == -1 ? _sFadeId = Shader.PropertyToID("_Fade") : _sFadeId; }
        }

        private static void GenerateNoiseTex()
        {
            var width = Screen.width;
            var height = Screen.height;

            var timer = new Stopwatch();
            timer.Start();

            if (_sNoiseTex == null) {
                _sNoiseTex = new Texture2D(width, height, TextureFormat.Alpha8, false);
                _sNoiseTex.filterMode = FilterMode.Bilinear;
            } else {
                _sNoiseTex.Resize(width, height);
            }

            var rand = new System.Random(0x54e03b19);
            var buffer = new byte[width * height];
            rand.NextBytes(buffer);

            _sNoiseTex.LoadRawTextureData(buffer);
            _sNoiseTex.Apply(false, true);

            UnityEngine.Debug.LogFormat("Noise gen time: {0:F2} ms", timer.Elapsed.TotalMilliseconds);
        }

        protected static Texture2D NoiseTex
        {
            get
            {
                if (ShouldGenerateNoiseTex) GenerateNoiseTex();
                return _sNoiseTex;
            }
        }

        private static readonly System.Random _sRandom = new System.Random(0x54e03b19);

        public static MapObject Create()
        {
            return new GameObject().AddComponent<MapObject>();
        }

        protected Instance Instance { get; private set; }

        private bool _loaded;
        private bool _canLoad;
        private bool _isVisible;
        private bool _isFading;

        public List<String> Flags;

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

                gameObject.SetActive(true);
                StartCoroutine(Fade());

                if (value && LodChild != null) {
                    LodChild.Hide();
                }
            }
        }

        public MapObject LodParent { get; private set; }
        public MapObject LodChild { get; private set; }

        public void Initialize(Instance inst, Dictionary<Instance, MapObject> dict)
        {
            Instance = inst;
            Instance.Object = Instance.Object ?? Cell.GameData.GetObject(inst.ObjectId);

            transform.position = inst.Position;
            transform.localRotation = inst.Rotation;

            CellPos = new Vector2(inst.Position.x, inst.Position.z);

            _canLoad = Instance.Object != null;
            _loaded = false;

            RandomInt = _sRandom.Next();

            name = _canLoad ? Instance.Object.Geometry : string.Format("Unknown ({0})", Instance.ObjectId);

            if (_canLoad && Instance.LodInstance != null) {
                LodChild = dict[Instance.LodInstance];
                LodChild.LodParent = this;
            }

            _isVisible = false;
            gameObject.SetActive(false);
            gameObject.isStatic = true;
        }

        public bool ShouldBeVisible(Vector3 from)
        {
            if (!_canLoad) return false;

            var obj = Instance.Object;
            var dist = Vector3.Distance(from, transform.position);

            return (dist <= obj.DrawDist || (obj.DrawDist >= 300 && dist < 2560))
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
                        Instance.Object.Flags, mat => mat.SetTexture(NoiseTexId, NoiseTex),
                        out mesh, out materials);

                    Flags = Enum.GetValues(typeof(ObjectFlag))
                        .Cast<ObjectFlag>()
                        .Where(x => (Instance.Object.Flags & x) == x)
                        .Select(x => x.ToString())
                        .ToList();

                    var mf = gameObject.AddComponent<MeshFilter>();
                    var mr = gameObject.AddComponent<MeshRenderer>();

                    mf.mesh = mesh;
                    mr.materials = materials;
                } catch (Exception e) {
                    _canLoad = false;

                    UnityEngine.Debug.LogWarningFormat("Failed to load {0} ({1})", Instance.ObjectId, e.Message);
                    name = string.Format("Failed ({0})", Instance.ObjectId);
                    return;
                }
            }

            IsVisible = LodParent == null || !LodParent.IsVisible;
            LoadOrder = float.PositiveInfinity;
        }

        private IEnumerator Fade()
        {
            if (_isFading) yield break;

            _isFading = true;

            const float fadeRate = 2f;

            var mr = GetComponent<MeshRenderer>();
            var pb = new MaterialPropertyBlock();

            var val = IsVisible ? 0f : -1f;

            for (;;) {
                var dest = IsVisible ? 1f : 0f;
                var sign = Math.Sign(dest - val);
                val += sign * fadeRate * Time.deltaTime;

                if (sign == 0 || sign == 1 && val >= dest || sign == -1 && val <= dest) break;

                pb.SetFloat(FadeId, (float) val);
                mr.SetPropertyBlock(pb);
                yield return new WaitForEndOfFrame();
            }

            mr.SetPropertyBlock(null);

            if (!IsVisible) {
                gameObject.SetActive(false);
            }

            _isFading = false;
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
