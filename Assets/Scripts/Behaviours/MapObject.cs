using System.Collections.Generic;
using System.Diagnostics;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Profiler = UnityEngine.Profiling.Profiler;

namespace SanAndreasUnity.Behaviours
{
    public abstract class MapObject : MonoBehaviour
    {
        private static Texture2D _sNoiseTex;

        private static bool ShouldGenerateNoiseTex => _sNoiseTex == null || _sNoiseTex.width != Screen.width || _sNoiseTex.height != Screen.height;

        private static int _sBreakableLayer = -1;
        public static int BreakableLayer => _sBreakableLayer == -1 ? _sBreakableLayer = LayerMask.NameToLayer("Breakable") : _sBreakableLayer;

        private static int _sNoiseTexPropertyId = -1;
        protected static int NoiseTexPropertyId => _sNoiseTexPropertyId == -1 ? _sNoiseTexPropertyId = Shader.PropertyToID("_NoiseTex") : _sNoiseTexPropertyId;

        private static int _sFadePropertyId = -1;
        protected static int FadePropertyId => _sFadePropertyId == -1 ? _sFadePropertyId = Shader.PropertyToID("_Fade") : _sFadePropertyId;

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

        public bool IsVisibleInMapSystem { get; private set; } = false;

        public float LoadPriority { get; private set; } = float.PositiveInfinity;

        private bool _loaded;
        public bool HasLoaded => _loaded;

        public Vector3 CachedPosition { get; private set; }

        protected static T Create<T>(GameObject prefab)
            where T : MapObject
        {
            GameObject go = Object.Instantiate(prefab, Cell.Instance.transform);
            return go.GetComponent<T>();
        }

        protected void Initialize(Vector3 pos, Quaternion rot)
        {
            this.transform.position = pos;
            this.transform.localRotation = rot;

            this.CachedPosition = pos;

            _loaded = false;
        }

        public void SetDrawDistance(float f)
        {

        }

        public void Show(float loadPriority)
        {
            if (this.IsVisibleInMapSystem)
                return;

            this.IsVisibleInMapSystem = true;

            this.LoadPriority = loadPriority;

            if (!_loaded)
            {
				_loaded = true;
                
				Profiler.BeginSample ("OnLoad", this);
				this.OnLoad();
				Profiler.EndSample ();
            }

			Profiler.BeginSample ("OnShow", this);
            this.OnShow();
			Profiler.EndSample ();
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

        public virtual void AddNavMeshBuildSources(List<NavMeshBuildSource> list)
        {
        }
    }
}