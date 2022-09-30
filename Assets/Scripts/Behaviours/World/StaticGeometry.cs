using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Items.Placements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.RenderWareStream;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using Geometry = SanAndreasUnity.Importing.Conversion.Geometry;
using Profiler = UnityEngine.Profiling.Profiler;
using UnityEngine.AI;

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

        public Instance Instance { get; private set; }

        public ISimpleObjectDefinition ObjectDefinition { get; private set; }

		[SerializeField]
		[HideInInspector]
		private int m_serializedObjectDefinitionId = -1;
		public int SerializedObjectDefinitionId => m_serializedObjectDefinitionId;

		[SerializeField]
		[HideInInspector]
		private Vector3 m_serializedInstancePosition;
		public Vector3 SerializedInstancePosition => m_serializedInstancePosition;

		[SerializeField]
		[HideInInspector]
		private Quaternion m_serializedInstanceRotation;
		public Quaternion SerializedInstanceRotation => m_serializedInstanceRotation;

		private bool _canLoad;
		private bool _isGeometryLoaded = false;
		private bool _isFading;

		public bool ShouldBeVisibleNow =>
	        this.IsVisibleInMapSystem
	        && this.IsVisibleBasedOnCurrentDayTime
	        && _isGeometryLoaded
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

        public int NumLightSources => m_lightSources?.Length ?? 0;


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
			m_serializedObjectDefinitionId = inst.ObjectId;
			m_serializedInstancePosition = inst.Position;
			m_serializedInstanceRotation = inst.Rotation;

            ObjectDefinition = Item.GetDefinition<Importing.Items.Definitions.ISimpleObjectDefinition>(inst.ObjectId);

            if (ObjectDefinition is TimeObjectDef)
            {
	            s_timedObjects.Add(this);
            }

            this.Initialize(
	            Cell.Instance.GetPositionBasedOnInteriorLevel(inst.Position, inst.InteriorLevel),
	            inst.Rotation);

            _canLoad = ObjectDefinition != null;

            name = _canLoad ? ObjectDefinition.ModelName : string.Format("Unknown ({0})", Instance.ObjectId);

			if (LodChild != null)
				LodChild.LodParent = null;
			LodChild = null;
            
            if (_canLoad && Instance.LodInstance != null)
            {
                if (dict.TryGetValue(Instance.LodInstance, out StaticGeometry dictValue))
                {
                    LodChild = dictValue;
                    LodChild.LodParent = this;
                }
            }

            this.SetDrawDistance(ObjectDefinition?.DrawDist ?? 0);

			if (!F.IsAppInEditMode)
				gameObject.SetActive(false);
            gameObject.isStatic = true;
        }

        protected override void OnLoad()
        {
            
            if (!_canLoad) return;

			if (null != this.GetComponent<MeshFilter>()) // already loaded - this also works in edit mode
				return;

			Profiler.BeginSample ("StaticGeometry.OnLoad", this);

			// note: some Clumps have collision model inside, but they are only used for vehicles, so we don't
			// need to load Clump if rendering is disabled

			bool loadRendering = !F.IsInHeadlessMode && Config.GetBool("loadStaticRenderModels");

			bool renderingLoaded = false;
			bool collisionLoaded = false;
			Geometry.GeometryParts geoms = null;

			if (loadRendering)
            {
				Geometry.LoadAsync(
					this.ObjectDefinition.ModelName,
					new string[] { this.ObjectDefinition.TextureDictionaryName },
					this.LoadPriority,
                    g =>
                    {
						renderingLoaded = true;
                        geoms = g;
						this.OnOneAssetLoaded(geoms, loadRendering, renderingLoaded, collisionLoaded);
                    });
            }

            // we can't load collision model asyncly, because it requires a transform to attach to
            // but, we can load collision file asyncly
            Importing.Collision.CollisionFile.FromNameAsync(
				this.ObjectDefinition.ModelName,
				this.LoadPriority,
                cf =>
                {
                    collisionLoaded = true;
					this.OnOneAssetLoaded(geoms, loadRendering, renderingLoaded, collisionLoaded);
                });


			Profiler.EndSample ();

        }

		private void OnOneAssetLoaded(Geometry.GeometryParts geoms, bool loadRendering, bool renderingLoaded, bool collisionLoaded)
        {
			if (collisionLoaded && (!loadRendering || renderingLoaded))
				this.OnAssetsLoaded(geoms);
        }

		private void OnAssetsLoaded(Geometry.GeometryParts geoms)
		{

			if (geoms != null)
				this.AddRenderingParts(geoms);

			Profiler.BeginSample("Attach collision", this);
			Importing.Conversion.CollisionModel.Load(this.ObjectDefinition.ModelName, this.transform, false);
			Profiler.EndSample();

			Profiler.BeginSample ("Set layer", this);

			if (ObjectDefinition.Flags.HasFlag(ObjectFlag.Breakable))
			{
				gameObject.SetLayerRecursive(BreakableLayer);
			}

			Profiler.EndSample ();

			_isGeometryLoaded = true;

			this.UpdateVisibility();

			if (LodParent == null)
				Cell.Singleton.RegisterNavMeshObject(this);
            
		}

		private void AddRenderingParts(Geometry.GeometryParts geoms)
        {
			Profiler.BeginSample("Add mesh", this);

			var mf = gameObject.AddComponent<MeshFilter>();
			var mr = gameObject.AddComponent<MeshRenderer>();

			mr.receiveShadows = this.ShouldReceiveShadows();
			mr.shadowCastingMode = this.ShouldCastShadows() ? ShadowCastingMode.On : ShadowCastingMode.Off;

			mf.sharedMesh = geoms.Geometry[0].Mesh;
			mr.sharedMaterials = geoms.Geometry[0].GetMaterials(ObjectDefinition.Flags, mat => mat.SetTexture(NoiseTexPropertyId, NoiseTex));

			Profiler.EndSample();

			Profiler.BeginSample("CreateLights()", this);
			if (!F.IsInHeadlessMode)
				this.CreateLights(geoms);
			Profiler.EndSample();

		}

		protected override void OnShow()
        {
			Profiler.BeginSample ("StaticGeometry.OnShow");

			this.UpdateVisibility();

			Profiler.EndSample ();
        }

		protected override void OnUnShow()
		{
			this.UpdateVisibility();
		}

		private IEnumerator FadeCoroutine()
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

	        float fadeRate = Cell.Instance.fadeRate;

            var pb = new MaterialPropertyBlock();

			// continuously change transparency until object becomes fully opaque or fully transparent

            float val = this.ShouldBeVisibleNow ? 0f : -1f;

            for (; ; )
            {
                float dest = this.ShouldBeVisibleNow ? 1f : 0f;
                var sign = Math.Sign(dest - val);
                val += sign * fadeRate * Time.deltaTime;

                if (sign == 0 || sign == 1 && val >= dest || sign == -1 && val <= dest)
	                break;

                pb.SetFloat(FadePropertyId, val);
                mr.SetPropertyBlock(pb);
                yield return null;
            }

            _isFading = false;

            mr.SetPropertyBlock(null);

            this.gameObject.SetActive(this.ShouldBeVisibleNow);
        }

        private void UpdateVisibility()
        {
	        bool needsFading = this.NeedsFading();

	        this.gameObject.SetActive(needsFading || this.ShouldBeVisibleNow);

	        if (needsFading)
	        {
		        _isFading = false;
		        this.StopCoroutine(nameof(FadeCoroutine));
		        this.StartCoroutine(nameof(FadeCoroutine));
	        }

	        if (LodChild != null)
		        LodChild.UpdateVisibility();
        }

        private bool NeedsFading()
        {
			if (F.IsAppInEditMode)
				return false;

	        if (F.IsInHeadlessMode)
		        return false;

	        // always fade, except when parent should be visible, but he is still loading

	        if (LodParent == null)
		        return true;

	        if (LodParent.IsVisibleInMapSystem && !LodParent._isGeometryLoaded)
		        return false;

	        return true;
        }

        private bool ShouldReceiveShadows()
        {
	        if (null == this.ObjectDefinition)
		        return false;

	        var flags = ObjectFlag.Additive // transparent
	                    | ObjectFlag.NoZBufferWrite // transparent
	                    | ObjectFlag.DontReceiveShadows;

	        if ((this.ObjectDefinition.Flags & flags) != 0)
		        return false;

	        if (LodParent != null) // LOD models should not receive shadows
		        return false;

	        return true;
        }

        private bool ShouldCastShadows()
        {
	        if (null == this.ObjectDefinition)
		        return false;

	        var flags = ObjectFlag.Additive // transparent
	                    | ObjectFlag.NoZBufferWrite; // transparent

	        if ((this.ObjectDefinition.Flags & flags) != 0)
		        return false;

	        // if object is LOD, only cast shadows if it has large draw distance
	        // - that's because his shadow may be visible from long distance
	        if (LodParent != null && this.ObjectDefinition.DrawDist < 1000f)
		        return false;

	        return true;
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

		public override void AddNavMeshBuildSources(List<NavMeshBuildSource> list)
		{
			if (LodParent != null)
				return;

			list.AddRange(Cell.GetNavMeshBuildSources(this.transform, 0));
		}
	}
}