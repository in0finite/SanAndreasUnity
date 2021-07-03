using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Items.Placements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.RenderWareStream;
using SanAndreasUnity.Utilities;
using UnityEngine;
using Geometry = SanAndreasUnity.Importing.Conversion.Geometry;
using Profiler = UnityEngine.Profiling.Profiler;

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

	        return Create<StaticGeometry>(Cell.Instance.staticGeometryPrefab);
        }

        private static List<StaticGeometry> s_timedObjects = new List<StaticGeometry>();
        public static IReadOnlyList<StaticGeometry> TimedObjects => s_timedObjects;

        protected Instance Instance { get; private set; }

        public ISimpleObjectDefinition ObjectDefinition { get; private set; }

        private bool _canLoad;
		private bool _isGeometryLoaded = false;
        private bool _isVisibleInMapSystem;
        private bool _isFading;

        public bool IsVisibleInMapSystem
        {
            get { return _isVisibleInMapSystem; }
            private set
            {
                if (_isVisibleInMapSystem == value) return;

                _isVisibleInMapSystem = value;

                this.gameObject.SetActive(this.ShouldBeVisibleNow);
                if (LodChild != null)
	                LodChild.gameObject.SetActive(LodChild.ShouldBeVisibleNow);
            }
        }

        public bool ShouldBeVisibleNow =>
	        this.IsVisibleInMapSystem
	        && this.IsVisibleBasedOnCurrentDayTime
			&& (LodParent == null || !LodParent.ShouldBeVisibleNow);

        public StaticGeometry LodParent { get; private set; }
        public StaticGeometry LodChild { get; private set; }

        private static bool s_registeredTimeChangeCallback = false;

        public bool IsVisibleBasedOnCurrentDayTime => this.ObjectDefinition is TimeObjectDef timeObjectDef ? IsObjectVisibleBasedOnCurrentDayTime(timeObjectDef) : true;

        // hashset is better because we do lookup/remove often, while iteration is done rarely
        private static HashSet<StaticGeometry> s_activeObjectsWithLights = new HashSet<StaticGeometry>();
        public static IReadOnlyCollection<StaticGeometry> ActiveObjectsWithLights => s_activeObjectsWithLights;

        // use arrays to save memory
        // set them to null to save memory
        private LightSource[] m_lightSources = null;
        private LightSource[] m_redTrafficLights = null;
        private LightSource[] m_yellowTrafficLights = null;
        private LightSource[] m_greenTrafficLights = null;
        private bool m_hasTrafficLights = false;
        private int m_activeTrafficLightIndex = -1;


        private void OnEnable()
        {
	        if (m_lightSources != null)
		        s_activeObjectsWithLights.Add(this);

	        this.UpdateLightsBasedOnDayTime();
	        this.UpdateTrafficLights();
        }

        private void OnDisable()
        {
	        s_activeObjectsWithLights.Remove(this);
        }

        private void OnDestroy()
        {
	        s_timedObjects.Remove(this);
        }

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

            this.SetDrawDistance(ObjectDefinition?.DrawDist ?? 0);

            _isVisibleInMapSystem = false;
            gameObject.SetActive(false);
            gameObject.isStatic = true;
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

			if (!F.IsInHeadlessMode)
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

		protected override void OnShow()
        {
			Profiler.BeginSample ("StaticGeometry.OnShow");
            IsVisibleInMapSystem = true;
			Profiler.EndSample ();
        }

		protected override void OnUnShow()
		{
			this.Hide();
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

            var val = IsVisibleInMapSystem ? 0f : -1f;

            for (; ; )
            {
                var dest = IsVisibleInMapSystem ? 1f : 0f;
                var sign = Math.Sign(dest - val);
                val += sign * fadeRate * Time.deltaTime;

                if (sign == 0 || sign == 1 && val >= dest || sign == -1 && val <= dest) break;

                pb.SetFloat(FadeId, (float)val);
                mr.SetPropertyBlock(pb);
                yield return new WaitForEndOfFrame();
            }

            mr.SetPropertyBlock(null);

            if (!IsVisibleInMapSystem || !IsVisibleBasedOnCurrentDayTime)
            {
                gameObject.SetActive(false);
            }

            _isFading = false;
        }

        public void Hide()
        {
            IsVisibleInMapSystem = false;
        }

        private static void OnHourChanged()
        {
	        foreach (var timedObject in s_timedObjects)
	        {
		        if (timedObject.IsVisibleInMapSystem)
		        {
			        timedObject.gameObject.SetActive(timedObject.IsVisibleBasedOnCurrentDayTime);
		        }
	        }

	        foreach (var activeObjectWithLight in s_activeObjectsWithLights)
	        {
		        activeObjectWithLight.UpdateLightsBasedOnDayTime();
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

        private void CreateLights(
	        Geometry.GeometryParts geometryParts)
        {
	        var lights = CreateLights(this.transform, geometryParts);
	        if (lights.Count == 0)
		        return;

	        m_lightSources = lights.ToArray();

	        if (this.gameObject.activeInHierarchy)
				s_activeObjectsWithLights.Add(this);

	        var trafficLightSources = lights
		        .Where(l => l.LightInfo.CoronaShowModeFlags == TwoDEffect.Light.CoronaShowMode.TRAFFICLIGHT)
		        .ToArray();

	        if (trafficLightSources.Length % 3 != 0)
		        Debug.LogError($"Traffic lights count should be multiple of 3, found {trafficLightSources.Length}");

	        var redLights = trafficLightSources.Where(l => IsColorInRange(l.LightInfo.Color, Color.red, 50, 30, 30)).ToArray();
	        var yellowLights = trafficLightSources.Where(l => IsColorInRange(l.LightInfo.Color, new Color32(255, 255, 0, 0), 50, 150, 80)).ToArray();
	        var greenLights = trafficLightSources.Where(l => IsColorInRange(l.LightInfo.Color, Color.green, 50, 50, 50)).ToArray();

	        if (redLights.Length + yellowLights.Length + greenLights.Length != trafficLightSources.Length)
	        {
		        Debug.LogError("Failed to identify some traffic light colors");
	        }

	        m_redTrafficLights = redLights.Length > 0 ? redLights : null;
	        m_yellowTrafficLights = yellowLights.Length > 0 ? yellowLights : null;
	        m_greenTrafficLights = greenLights.Length > 0 ? greenLights : null;

	        m_hasTrafficLights = m_redTrafficLights != null || m_yellowTrafficLights != null || m_greenTrafficLights != null;

	        this.UpdateLightsBasedOnDayTime();

	        this.gameObject.AddComponent<FaceTowardsCamera>().transformsToFace = m_lightSources.Select(l => l.transform).ToArray();

	        this.InvokeRepeating(nameof(this.UpdateLights), 0f, 0.2f);
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

        private float GetTrafficLightTimeOffset()
        {
	        // determine time offset based on rotation of object
	        float angle = Vector3.Angle(this.transform.forward.WithXAndZ(), Vector3.forward);
	        float perc = angle / 180f;
	        return perc * GetTrafficLightCycleDuration();
        }

        private static float GetTrafficLightCycleDuration()
        {
	        var cell = Cell.Instance;
	        return cell.redTrafficLightDuration + cell.yellowTrafficLightDuration + cell.greenTrafficLightDuration;
        }

        private int CalculateActiveTrafficLightIndex()
        {
	        int index = -1;

	        double currentTimeForThisObject = Net.NetManager.NetworkTime + (double)GetTrafficLightTimeOffset();
	        double timeInsideCycle = currentTimeForThisObject % (double)GetTrafficLightCycleDuration();

	        var cell = Cell.Instance;

	        if (timeInsideCycle <= cell.redTrafficLightDuration)
		        index = 0;
	        else if (timeInsideCycle <= cell.redTrafficLightDuration + cell.yellowTrafficLightDuration)
		        index = 1;
	        else if (timeInsideCycle <= cell.redTrafficLightDuration + cell.yellowTrafficLightDuration + cell.greenTrafficLightDuration)
		        index = 2;

	        return index;
        }

        private void UpdateLights()
        {
	        UpdateTrafficLights();
        }

        private void UpdateTrafficLights()
        {
	        if (!m_hasTrafficLights)
		        return;

	        // update active traffic light
	        m_activeTrafficLightIndex = this.CalculateActiveTrafficLightIndex();

	        // disable/enable traffic lights based on which one is active

	        if (m_redTrafficLights != null)
		        EnableLights(m_redTrafficLights, m_activeTrafficLightIndex == 0);
	        if (m_yellowTrafficLights != null)
		        EnableLights(m_yellowTrafficLights, m_activeTrafficLightIndex == 1);
	        if (m_greenTrafficLights != null)
		        EnableLights(m_greenTrafficLights, m_activeTrafficLightIndex == 2);
        }

        private void UpdateLightsBasedOnDayTime()
        {
	        if (null == m_lightSources)
		        return;

	        bool isDay = DayTimeManager.Singleton.CurrentTimeHours > 6 && DayTimeManager.Singleton.CurrentTimeHours < 18;
	        var flag = isDay ? TwoDEffect.Light.Flags1.AT_DAY : TwoDEffect.Light.Flags1.AT_NIGHT;

	        for (int i = 0; i < m_lightSources.Length; i++)
	        {
		        bool b = (m_lightSources[i].LightInfo.Flags_1 & flag) == flag;
		        m_lightSources[i].gameObject.SetActive(b);
	        }
        }

        private static void EnableLights(LightSource[] lights, bool enable)
        {
	        for (int i = 0; i < lights.Length; i++)
	        {
		        lights[i].gameObject.SetActive(enable);
	        }
        }

        private static bool IsColorInRange(Color32 targetColor, Color32 colorToCheck, int redVar, int greenVar, int blueVar)
        {
	        var diffR = targetColor.r - colorToCheck.r;
	        var diffG = targetColor.g - colorToCheck.g;
	        var diffB = targetColor.b - colorToCheck.b;

	        return Mathf.Abs(diffR) <= redVar && Mathf.Abs(diffG) <= greenVar && Mathf.Abs(diffB) <= blueVar;
        }
    }
}