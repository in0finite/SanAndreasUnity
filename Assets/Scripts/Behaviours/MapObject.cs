using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public abstract class MapObject : MonoBehaviour, IComparable<MapObject>
    {
        private static Texture2D _sNoiseTex;

        private static bool ShouldGenerateNoiseTex
        {
            get { return _sNoiseTex == null || _sNoiseTex.width != Screen.width || _sNoiseTex.height != Screen.height; }
        }

        private static int _sBreakableLayer = -1;
        public static int BreakableLayer
        {
            get { return _sBreakableLayer == -1 ? _sBreakableLayer = LayerMask.NameToLayer("Breakable") : _sBreakableLayer; }
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
            _sNoiseTex.Apply(false, false);

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

        private bool _loaded;

        public bool HasLoaded { get { return _loaded; } }

        public List<String> Flags;

        public Vector2 CellPos { get; private set; }

        public int RandomInt { get; private set; }

        internal float LoadOrder { get; private set; }

        protected void Initialize(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.localRotation = rot;

            CellPos = new Vector2(pos.x, pos.z);

            RandomInt = _sRandom.Next();

            _loaded = false;
        }

        public bool RefreshLoadOrder(Vector3 from)
        {
            LoadOrder = OnRefreshLoadOrder(from);
            return !float.IsPositiveInfinity(LoadOrder);
        }

        protected abstract float OnRefreshLoadOrder(Vector3 from);

        public void Show()
        {
            if (!_loaded) {
                _loaded = true;
                OnLoad();
            }

            OnShow();
            LoadOrder = float.PositiveInfinity;
        }

        protected virtual void OnLoad() { }

        protected virtual void OnShow() { }

        public int CompareTo(MapObject other)
        {
            return LoadOrder > other.LoadOrder ? 1 : LoadOrder == other.LoadOrder ? 0 : -1;
        }
    }
}
