using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Placements;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Profiler = UnityEngine.Profiling.Profiler;

namespace SanAndreasUnity.Behaviours.World
{
    public class Cell : MonoBehaviour
    {
	    private Dictionary<Instance, StaticGeometry> m_insts;
        public int NumStaticGeometries => m_insts.Count;
		private MapObject[] m_cars;
        private List<EntranceExitMapObject> m_enexes;

        private List<int> CellIds = Enumerable.Range(0, 19).ToList();

        public bool HasMainExterior => this.CellIds.Contains(0);

        public Camera PreviewCamera;

        public struct FocusPointInfo
        {
	        public long id;
	        public Transform transform;
	        public float timeToKeepRevealingAfterDestroyed;
	        public float timeWhenRemoved;
        }

		private List<FocusPointInfo> _focusPoints = new List<FocusPointInfo>();
		public IReadOnlyList<FocusPointInfo> FocusPoints => _focusPoints;

		private List<FocusPointInfo> _focusPointsToRemoveAfterTimeout = new List<FocusPointInfo>();

        public Water Water;

		public static Cell Instance { get ; private set; }

		public float divisionRefreshDistanceDelta = 20;

		public float maxDrawDistance = 500;

        public float[] drawDistancesPerLayers = new float[] { 301, 801, 1501 };

        private WorldSystemWithDistanceLevels<MapObject> _worldSystem;
        public WorldSystemWithDistanceLevels<MapObject> WorldSystem => _worldSystem;

        public uint yWorldSystemSize = 25000;
        public ushort yWorldSystemNumAreas = 3;

        public ushort[] xzWorldSystemNumAreasPerDrawDistanceLevel = { 100, 100, 100 };

        public float interiorHeightOffset = 5000f;

        public float fadeRate = 2f;

        public bool loadParkedVehicles = true;

        public GameObject mapObjectActivatorPrefab;

        public GameObject staticGeometryPrefab;

        public GameObject enexPrefab;

        public GameObject lightSourcePrefab;

        public float lightScaleMultiplier = 1f;

        public float redTrafficLightDuration = 7;
        public float yellowTrafficLightDuration = 2;
        public float greenTrafficLightDuration = 7;



        private void Awake()
        {
			if (null == Instance)
				Instance = this;
			
        }


        internal void CreateStaticGeometry ()
		{
			var placements = Item.GetPlacements<Instance>(CellIds.ToArray());

			m_insts = new Dictionary<Instance,StaticGeometry> (48 * 1024);
			foreach (var plcm in placements) {
				m_insts.Add (plcm, StaticGeometry.Create ());
			}
			//m_insts = placements.ToDictionary(x => x, x => StaticGeometry.Create());

			UnityEngine.Debug.Log("Num static geometries " + m_insts.Count);

			uint worldSize = 6000;

			_worldSystem = new WorldSystemWithDistanceLevels<MapObject>(
				this.drawDistancesPerLayers,
				this.xzWorldSystemNumAreasPerDrawDistanceLevel.Select(_ => new WorldSystemParams { worldSize = worldSize, numAreasPerAxis = _ }).ToArray(),
				Enumerable.Range(0, this.drawDistancesPerLayers.Length).Select(_ => new WorldSystemParams { worldSize = this.yWorldSystemSize, numAreasPerAxis = this.yWorldSystemNumAreas }).ToArray(),
				this.OnAreaChangedVisibility);
		}

		internal void InitStaticGeometry ()
		{
			foreach (var inst in m_insts)
			{
				var staticGeometry = inst.Value;
				staticGeometry.Initialize(inst.Key, m_insts);
				_worldSystem.AddObjectToArea(
					staticGeometry.transform.position,
					staticGeometry.ObjectDefinition?.DrawDist ?? 0,
					staticGeometry);
			}
		}

		internal void LoadParkedVehicles ()
		{
			if (loadParkedVehicles)
			{
				var parkedVehicles = Item.GetPlacements<ParkedVehicle> (CellIds.ToArray ());
				m_cars = parkedVehicles.Select (x => VehicleSpawnMapObject.Create (x))
					.Cast<MapObject> ()
					.ToArray ();

				UnityEngine.Debug.Log ("Num parked vehicles " + m_cars.Length);
			}
		}

        internal void CreateEnexes()
        {
            m_enexes = new List<EntranceExitMapObject>(256);
            foreach(var enex in Item.Enexes.Where(enex => this.CellIds.Contains(enex.TargetInterior)))
            {
	            var enexComponent = EntranceExitMapObject.Create(enex);
	            m_enexes.Add(enexComponent);
	            _worldSystem.AddObjectToArea(enexComponent.transform.position, 100f, enexComponent);
            }
        }

        internal void LoadWater ()
		{
			if (Water != null)
			{
				Water.Initialize(new WaterFile(Importing.Archive.ArchiveManager.PathToCaseSensitivePath(Config.GetPath("water_path"))));
			}
		}

		internal void FinalizeLoad ()
		{
			// set layer recursively for all game objects
			//	this.gameObject.SetLayerRecursive( this.gameObject.layer );

		}


		private void OnAreaChangedVisibility(WorldSystem<MapObject>.Area area, bool visible)
		{
			if (null == area.ObjectsInside)
				return;

			Profiler.BeginSample("OnAreaChangedVisibility");

			WorldSystem<MapObject>.FocusPoint[] focusPointsThatSeeMe = null;
			if (visible)
				focusPointsThatSeeMe = area.FocusPointsThatSeeMe.ToArray();

			for (int i = 0; i < area.ObjectsInside.Count; i++)
			{
				var obj = area.ObjectsInside[i];

				if (visible == obj.IsVisibleInMapSystem)
					continue;

				F.RunExceptionSafe(() =>
				{
					if (visible)
					{
						float minSqrDistance = focusPointsThatSeeMe.Min(f => (obj.CachedPosition - f.Position).sqrMagnitude);
						obj.Show(minSqrDistance);
					}
					else
						obj.UnShow();
				});
			}

			Profiler.EndSample();
		}

		private void RegisterFocusPoint(Transform tr, float revealRadius, float timeToKeepRevealingAfterDestroyed)
		{
			if (!_focusPoints.Exists(f => f.transform == tr))
			{
				long registeredFocusPointId = _worldSystem.RegisterFocusPoint(revealRadius, tr.position);
				_focusPoints.Add(new FocusPointInfo
				{
					id = registeredFocusPointId,
					transform = tr,
					timeToKeepRevealingAfterDestroyed = timeToKeepRevealingAfterDestroyed,
				});
			}
		}

		public void RegisterFocusPoint(Transform tr, float revealRadius) => this.RegisterFocusPoint(tr, revealRadius, 0f);

		public void RegisterFocusPoint(Transform tr) => this.RegisterFocusPoint(tr, this.maxDrawDistance);

		public void RegisterFocusPoint(Transform tr, FocusPoint.Parameters parameters)
		{
			if (parameters.hasRevealRadius)
				this.RegisterFocusPoint(tr, parameters.revealRadius);
			else
				this.RegisterFocusPoint(tr);
		}

		public void UnRegisterFocusPoint(Transform tr)
		{
			int index = _focusPoints.FindIndex(f => f.transform == tr);
			if (index < 0)
				return;

			// maybe we could just set transform to null, so it gets removed during next update ?

			var focusPoint = _focusPoints[index];

			if (focusPoint.timeToKeepRevealingAfterDestroyed > 0)
			{
				focusPoint.timeWhenRemoved = Time.time;
				_focusPointsToRemoveAfterTimeout.Add(focusPoint);
				_focusPoints.RemoveAt(index);
				return;
			}

			_worldSystem.UnRegisterFocusPoint(focusPoint.id);
			_focusPoints.RemoveAt(index);
		}


        public IEnumerable<EntranceExit> GetEnexesFromLoadedInteriors()
        {
            int[] loadedInteriors = this.CellIds.Where(id => !IsExteriorLevel(id)).ToArray();
            foreach(var enex in Importing.Items.Item.Enexes.Where(enex => loadedInteriors.Contains(enex.TargetInterior)))
            {
                yield return enex;
            }
        }

        public TransformDataStruct GetEnexExitTransform(EntranceExit enex)
        {
            return new TransformDataStruct(
	            this.GetPositionBasedOnInteriorLevel(enex.ExitPos + Vector3.up * 0.2f, enex.TargetInterior),
	            Quaternion.Euler(0f, enex.ExitAngle, 0f));
        }

        public TransformDataStruct GetEnexEntranceTransform(EntranceExit enex)
        {
            return new TransformDataStruct(
	            this.GetPositionBasedOnInteriorLevel(enex.EntrancePos + Vector3.up * 0.2f, enex.TargetInterior),
	            Quaternion.Euler(0f, enex.EntranceAngle, 0f));
        }

        public static bool IsExteriorLevel(int interiorLevel)
        {
	        return interiorLevel == 0 || interiorLevel == 13;
        }

        public Vector3 GetPositionBasedOnInteriorLevel(Vector3 originalPos, int interiorLevel)
        {
	        if (!IsExteriorLevel(interiorLevel))
		        originalPos.y += this.interiorHeightOffset;
	        return originalPos;
        }


        private void Update()
        {

			if (!Loader.HasLoaded)
				return;

			float timeNow = Time.time;

			UnityEngine.Profiling.Profiler.BeginSample("Update focus points");
            this._focusPoints.RemoveAll(f =>
            {
	            if (null == f.transform)
	            {
		            if (f.timeToKeepRevealingAfterDestroyed > 0f)
		            {
			            f.timeWhenRemoved = timeNow;
			            _focusPointsToRemoveAfterTimeout.Add(f);
			            return true;
		            }

		            UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.UnRegisterFocusPoint()");
		            _worldSystem.UnRegisterFocusPoint(f.id);
		            UnityEngine.Profiling.Profiler.EndSample();
		            return true;
	            }

	            _worldSystem.FocusPointChangedPosition(f.id, f.transform.position);

	            return false;
            });
            UnityEngine.Profiling.Profiler.EndSample();

            bool hasElementToRemove = false;
            _focusPointsToRemoveAfterTimeout.ForEach(_ =>
            {
	            if (timeNow - _.timeWhenRemoved > _.timeToKeepRevealingAfterDestroyed)
	            {
		            hasElementToRemove = true;
		            UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.UnRegisterFocusPoint()");
		            _worldSystem.UnRegisterFocusPoint(_.id);
		            UnityEngine.Profiling.Profiler.EndSample();
	            }
            });

            if (hasElementToRemove)
	            _focusPointsToRemoveAfterTimeout.RemoveAll(_ => timeNow - _.timeWhenRemoved > _.timeToKeepRevealingAfterDestroyed);

            if (this._focusPoints.Count > 0)
            {
                // only update divisions loading if there are focus points - because otherwise, 
                // load order of divisions is not updated

            }

            UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.Update()");
            _worldSystem.Update();
            UnityEngine.Profiling.Profiler.EndSample();

        }
    }
}