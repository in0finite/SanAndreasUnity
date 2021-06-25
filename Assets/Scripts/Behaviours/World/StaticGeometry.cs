using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Items.Placements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.RenderWareStream;
using UnityEngine;
using UnityEngine.Profiling;
using Geometry = SanAndreasUnity.Importing.Conversion.Geometry;

namespace SanAndreasUnity.Behaviours.World
{
    public class StaticGeometry : MapObject
    {
        public static StaticGeometry Create()
        {
	        if (!s_registeredTimeChangeCallback)
	        {
		        s_registeredTimeChangeCallback = true;
		        DayTimeManager.Singleton.onHourChanged += OnHourChanged;
	        }

            return new GameObject().AddComponent<StaticGeometry>();
        }

        private static List<StaticGeometry> s_timedObjects = new List<StaticGeometry>();
        public static IReadOnlyList<StaticGeometry> TimedObjects => s_timedObjects;

        protected Instance Instance { get; private set; }

        public ISimpleObjectDefinition ObjectDefinition { get; private set; }

        private bool _canLoad;
		private bool _isGeometryLoaded = false;
        private bool _isVisible;
        private bool _isFading;

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (_isVisible == value) return;

                _isVisible = value;

                gameObject.SetActive(true);
                StartCoroutine(Fade());

                if (value && LodChild != null)
                {
                    LodChild.Hide();
                }
            }
        }

        public StaticGeometry LodParent { get; private set; }
        public StaticGeometry LodChild { get; private set; }

        private static bool s_registeredTimeChangeCallback = false;

        public bool IsVisibleBasedOnCurrentDayTime => this.ObjectDefinition is TimeObjectDef timeObjectDef ? IsObjectVisibleBasedOnCurrentDayTime(timeObjectDef) : true;

        private List<LightSource> m_lightSources = null;
        private List<LightSource> m_trafficLightSources = null;
        private int m_activeTrafficLightIndex = -1;
        //private float m_timeSinceUpdatedTrafficLights = 0;


        public void Initialize(Instance inst, Dictionary<Instance, StaticGeometry> dict)
        {
            Instance = inst;
            ObjectDefinition = Item.GetDefinition<Importing.Items.Definitions.ISimpleObjectDefinition>(inst.ObjectId);

            if (ObjectDefinition is TimeObjectDef)
            {
	            s_timedObjects.Add(this);
            }

            Initialize(inst.Position, inst.Rotation);

            _canLoad = ObjectDefinition != null;

            name = _canLoad ? ObjectDefinition.ModelName : string.Format("Unknown ({0})", Instance.ObjectId);

            if (_canLoad && Instance.LodInstance != null)
            {
                if (dict.TryGetValue(Instance.LodInstance, out StaticGeometry dictValue))
                {
                    LodChild = dictValue;
                    LodChild.LodParent = this;
                }
            }

            _isVisible = false;
            gameObject.SetActive(false);
            gameObject.isStatic = true;
        }

        public bool ShouldBeVisible(Vector3 from)
        {
            if (!_canLoad) return false;

            var obj = ObjectDefinition;

            //	if (obj.HasFlag (ObjectFlag.DisableDrawDist))
            //		return true;

            //    var dist = Vector3.Distance(from, transform.position);
            var distSquared = Vector3.SqrMagnitude(from - transform.position);

            if (distSquared > Cell.Instance.maxDrawDistance * Cell.Instance.maxDrawDistance)
                return false;

            if (distSquared > obj.DrawDist * obj.DrawDist)
                return false;

            if (!HasLoaded || LodParent == null || !LodParent.IsVisible)
                return true;

            if (!LodParent.ShouldBeVisible(from))
                return true;

            return false;

            //	return (distSquared <= obj.DrawDist * obj.DrawDist || (obj.DrawDist >= 300 && distSquared < 2560*2560))
            //        && (!HasLoaded || LodParent == null || !LodParent.IsVisible || !LodParent.ShouldBeVisible(from));
        }

        protected override float OnRefreshLoadOrder(Vector3 from)
        {
            var visible = ShouldBeVisible(from);

            if (!IsVisible)
            {
                return visible ? Vector3.SqrMagnitude(from - transform.position) : float.PositiveInfinity;
            }

            if (!visible) Hide();

            return float.PositiveInfinity;
        }

        protected override void OnLoad()
        {
            
            if (!_canLoad) return;

			Profiler.BeginSample ("StaticGeometry.OnLoad", this);


			// this was previously placed after loading geometry
			Flags = Enum.GetValues(typeof(ObjectFlag))
				.Cast<ObjectFlag>()
				.Where(x => (ObjectDefinition.Flags & x) == x)
				.Select(x => x.ToString())
				.ToList();

            //var geoms = Geometry.Load(Instance.Object.ModelName, Instance.Object.TextureDictionaryName);
			//OnGeometryLoaded (geoms);

			// we could start loading collision model here
			// - we can't, because we don't know the name of collision file until clump is loaded

			Geometry.LoadAsync( ObjectDefinition.ModelName, new string[] {ObjectDefinition.TextureDictionaryName}, (geoms) => {
				if(geoms != null)
				{
					// we can't load collision model asyncly, because it requires a transform to attach to
					// but, we can load collision file asyncly
					Importing.Collision.CollisionFile.FromNameAsync( geoms.Collisions != null ? geoms.Collisions.Name : geoms.Name, (cf) => {
						OnGeometryLoaded (geoms);
					});
				}
			});


			Profiler.EndSample ();

        }

		private void OnGeometryLoaded (Geometry.GeometryParts geoms)
		{

			Profiler.BeginSample ("Add mesh", this);

			var mf = gameObject.AddComponent<MeshFilter>();
			var mr = gameObject.AddComponent<MeshRenderer>();

			mf.sharedMesh = geoms.Geometry[0].Mesh;
			mr.sharedMaterials = geoms.Geometry[0].GetMaterials(ObjectDefinition.Flags, mat => mat.SetTexture(NoiseTexId, NoiseTex));

			Profiler.EndSample ();

			this.CreateLights(geoms);

			geoms.AttachCollisionModel(transform);

			Profiler.BeginSample ("Set layer", this);

			if (ObjectDefinition.Flags.HasFlag(ObjectFlag.Breakable))
			{
				gameObject.SetLayerRecursive(BreakableLayer);
			}

			Profiler.EndSample ();

			_isGeometryLoaded = true;

		}

		private void OnCollisionModelAttached ()
		{


		}

		private void CreateLights(
			Geometry.GeometryParts geometryParts)
		{
			var lights = CreateLights(this.transform, geometryParts);
			if (lights.Count == 0)
				return;

			m_lightSources = lights;

			m_trafficLightSources = lights
				.Where(l => l.LightInfo.CoronaShowModeFlags == TwoDEffect.Light.CoronaShowMode.TRAFFICLIGHT)
				.ToList();

			if (m_trafficLightSources.Count % 3 != 0)
				Debug.LogError($"Traffic lights count must be in multiple of 3");

			this.StartCoroutine(this.UpdateLightsCoroutine());
		}

		public static List<LightSource> CreateLights(
			Transform tr,
			Geometry.GeometryParts geometryParts)
		{
			Profiler.BeginSample("CreateLights()", tr);

			var lights = new List<LightSource>();

			foreach (var geometry in geometryParts.Geometry)
			{
				var twoDEffect = geometry.RwGeometry.TwoDEffect;
				if (twoDEffect != null && twoDEffect.Lights != null)
				{
					foreach (var lightInfo in twoDEffect.Lights)
					{
						lights.Add(LightSource.Create(tr, lightInfo));
					}
				}
			}

			Profiler.EndSample();

			return lights;
		}

        protected override void OnShow()
        {
			Profiler.BeginSample ("StaticGeometry.OnShow");
            IsVisible = LodParent == null || !LodParent.IsVisible;
			Profiler.EndSample ();
        }

        private IEnumerator Fade()
        {
            if (_isFading) yield break;

            _isFading = true;

			// wait until geometry gets loaded
			while (!_isGeometryLoaded)
				yield return null;

			var mr = GetComponent<MeshRenderer>();
			if (mr == null)
			{
				_isFading = false;
				yield break;
			}

            const float fadeRate = 2f;

            var pb = new MaterialPropertyBlock();

			// continuously change transparency until object becomes fully opaque or fully transparent

            var val = IsVisible ? 0f : -1f;

            for (; ; )
            {
                var dest = IsVisible ? 1f : 0f;
                var sign = Math.Sign(dest - val);
                val += sign * fadeRate * Time.deltaTime;

                if (sign == 0 || sign == 1 && val >= dest || sign == -1 && val <= dest) break;

                pb.SetFloat(FadeId, (float)val);
                mr.SetPropertyBlock(pb);
                yield return new WaitForEndOfFrame();
            }

            mr.SetPropertyBlock(null);

            if (!IsVisible || !IsVisibleBasedOnCurrentDayTime)
            {
                gameObject.SetActive(false);
            }

            _isFading = false;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        private static void OnHourChanged()
        {
	        foreach (var timedObject in s_timedObjects)
	        {
		        if (timedObject.IsVisible)
		        {
			        timedObject.gameObject.SetActive(timedObject.IsVisibleBasedOnCurrentDayTime);
		        }
	        }
        }

        private static bool IsObjectVisibleBasedOnCurrentDayTime(TimeObjectDef timeObjectDef)
        {
	        byte currentHour = DayTimeManager.Singleton.CurrentTimeHours;
	        if (timeObjectDef.TimeOnHours < timeObjectDef.TimeOffHours)
	        {
		        return currentHour >= timeObjectDef.TimeOnHours && currentHour < timeObjectDef.TimeOffHours;
	        }
	        else
	        {
		        return currentHour >= timeObjectDef.TimeOnHours || currentHour < timeObjectDef.TimeOffHours;
	        }
        }

        private IEnumerator UpdateLightsCoroutine()
        {
	        while (true)
	        {
		        var cam = Camera.current;
		        if (null == cam)
			        yield return null;

		        for (int i = 0; i < m_lightSources.Count; i++)
		        {
			        m_lightSources[i].transform.forward = - cam.transform.forward;
		        }

		        if (m_trafficLightSources.Count % 3 == 0)
		        {
			        // update active traffic light


			        // disable/enable traffic lights
			        int trafficIndex = 0;
			        for (int i = 0; i < m_trafficLightSources.Count; i++)
			        {
				        bool isActive = trafficIndex == m_activeTrafficLightIndex;
				        m_trafficLightSources[i].gameObject.SetActive(isActive);

				        trafficIndex = (trafficIndex + 1) % 3;
			        }
		        }
	        }
        }
    }
}