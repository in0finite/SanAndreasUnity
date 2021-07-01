using System;
using System.Collections.Generic;
using System.Diagnostics;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Profiler = UnityEngine.Profiling.Profiler;

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

            if (_sNoiseTex == null)
            {
                _sNoiseTex = new Texture2D(width, height, TextureFormat.Alpha8, false);
                _sNoiseTex.filterMode = FilterMode.Bilinear;
            }
            else
            {
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

        public bool IsVisibleInMapSystem { get; private set; } = false;

        private bool _loaded;

        public bool HasLoaded { get { return _loaded; } }

        public List<String> Flags;

        public Vector2 CellPos { get; private set; }

        public int RandomInt { get; private set; }

        internal float LoadOrder { get; private set; }

        protected static T Create<T>(GameObject prefab)
            where T : MapObject
        {
            GameObject go = Instantiate(prefab, Cell.Instance.transform);
            return go.GetComponent<T>();
        }

        protected void Initialize(Vector3 pos, Quaternion rot)
        {
            this.transform.position = pos;
            this.transform.localRotation = rot;

            CellPos = new Vector2(pos.x, pos.z);

            RandomInt = _sRandom.Next();

            _loaded = false;
        }

        public void SetDrawDistance(float f)
        {

        }

        public void Show()
        {
            if (this.IsVisibleInMapSystem)
                return;

            this.IsVisibleInMapSystem = true;

            if (!_loaded)
            {
				_loaded = true;
                
				Profiler.BeginSample ("OnLoad", this);
				OnLoad();
				Profiler.EndSample ();
            }

			Profiler.BeginSample ("OnShow", this);
            OnShow();
			Profiler.EndSample ();

            LoadOrder = float.PositiveInfinity;
        }

        public void UnShow()
        {
            if (!this.IsVisibleInMapSystem)
                return;

            this.IsVisibleInMapSystem = false;

            this.OnUnShow();
        }

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnShow()
        {
            this.gameObject.SetActive(true);
        }

        protected virtual void OnUnShow()
        {
            this.gameObject.SetActive(false);
        }

        public int CompareTo(MapObject other)
        {
            return LoadOrder > other.LoadOrder ? 1 : LoadOrder == other.LoadOrder ? 0 : -1;
        }
    }
}