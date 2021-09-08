using System;
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

        public FocusPointManager<MapObject> FocusPointManager { get; private set; }

		private struct AreaWithDistance
		{
			public WorldSystem<MapObject>.Area area;
			public float distance;
		}

		private class AreaWithDistanceComparer : IComparer<AreaWithDistance>
		{
			public int Compare(AreaWithDistance a, AreaWithDistance b)
			{
				if (a.distance < b.distance)
					return -1;

				if (a.distance > b.distance)
					return 1;

				// solve potential problem that 2 areas have same distance
				// - these areas may not be equal, they may belong to different world systems and have same index,
				// therefore their distances will be equal
				// - by comparing their ids, we are sure that comparer is always deterministic
				return a.area.Id.CompareTo(b.area.Id);
			}
		}

		private readonly SortedSet<AreaWithDistance> _areasToUpdate = new SortedSet<AreaWithDistance>(new AreaWithDistanceComparer());
		private readonly AreaWithDistance[] _bufferOfAreasToUpdate = new AreaWithDistance[64];
		private int _indexOfBufferOfAreasToUpdate = 0;
		private int _numElementsInBufferOfAreasToUpdate = 0;

		public ushort maxTimeToUpdatePerFrameMs = 10;

		private System.Diagnostics.Stopwatch _updateTimeLimitStopwatch = new System.Diagnostics.Stopwatch();

        public Water Water;

		public static Cell Instance { get ; private set; }

		public float divisionRefreshDistanceDelta = 20;

		private float _maxDrawDistance = 0;
		public float MaxDrawDistance
		{
			get => _maxDrawDistance;
			set
			{
				if (_maxDrawDistance == value)
					return;

				_maxDrawDistance = value;

				this.OnMaxDrawDistanceChanged();
			}
		}

		public int WorldSize { get; } = 6000;

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

			_worldSystem = new WorldSystemWithDistanceLevels<MapObject>(
				this.drawDistancesPerLayers,
				this.xzWorldSystemNumAreasPerDrawDistanceLevel.Select(_ => new WorldSystemParams { worldSize = (uint) this.WorldSize, numAreasPerAxis = _ }).ToArray(),
				Enumerable.Range(0, this.drawDistancesPerLayers.Length).Select(_ => new WorldSystemParams { worldSize = this.yWorldSystemSize, numAreasPerAxis = this.yWorldSystemNumAreas }).ToArray(),
				this.OnAreaChangedVisibility);

			this.FocusPointManager = new FocusPointManager<MapObject>(_worldSystem, this.MaxDrawDistance);
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
			if (F.IsInHeadlessMode)
				return;

			if (Water != null)
			{
				Water.Initialize(
					new WaterFile(Importing.Archive.ArchiveManager.PathToCaseSensitivePath(Config.GetPath("water_path"))),
					Vector2.one * this.WorldSize);
			}
		}

		internal void FinalizeLoad ()
		{
			// set layer recursively for all game objects
			//	this.gameObject.SetLayerRecursive( this.gameObject.layer );

		}


		private void OnAreaChangedVisibility(WorldSystem<MapObject>.Area area, bool visible)
		{
			if (null == area.ObjectsInside || area.ObjectsInside.Count == 0)
				return;

			Profiler.BeginSample("OnAreaChangedVisibility");

			Vector3 areaCenter = area.WorldSystem.GetAreaCenter(area);

			float minSqrDistance = area.FocusPointsThatSeeMe?.Min(f => (areaCenter - f.Position).sqrMagnitude) ?? float.PositiveInfinity;

			_areasToUpdate.Add(new AreaWithDistance
			{
				area = area,
				distance = minSqrDistance,
			});

			Profiler.EndSample();
		}

		public void RegisterFocusPoint(Transform tr, FocusPointParameters parameters)
		{
			this.FocusPointManager.RegisterFocusPoint(tr, parameters);
		}

		public void UnRegisterFocusPoint(Transform tr)
		{
			this.FocusPointManager.UnRegisterFocusPoint(tr);
		}

		private void OnMaxDrawDistanceChanged()
		{
			if (this.FocusPointManager != null)
				this.FocusPointManager.ChangeDefaultRevealRadius(this.MaxDrawDistance);
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

			_updateTimeLimitStopwatch.Restart();

			this.FocusPointManager.Update();

            /*if (this._focusPoints.Count > 0)
            {
                // only update divisions loading if there are focus points - because otherwise, 
                // load order of divisions is not updated

            }*/

            UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.Update()");
            _worldSystem.Update();
            UnityEngine.Profiling.Profiler.EndSample();

            Profiler.BeginSample("UpdateAreasLoop");

            while (true)
            {
	            if (_areasToUpdate.Count == 0 && _numElementsInBufferOfAreasToUpdate == 0)
		            break;

	            if (_updateTimeLimitStopwatch.ElapsedMilliseconds >= this.maxTimeToUpdatePerFrameMs)
		            break;

	            /*_areasToUpdate.CopyTo();
	            _areasToUpdate.Clear(); // very good
	            _areasToUpdate.ExceptWith(); // seems good
	            _areasToUpdate.UnionWith(); // catastrophic*/

	            if (_numElementsInBufferOfAreasToUpdate == 0)
	            {
		            Profiler.BeginSample("TakeFromSortedSet");

		            // we processed all areas from buffer
		            // take some more from SortedSet
		            int numToCopy = Mathf.Min(_bufferOfAreasToUpdate.Length, _areasToUpdate.Count);
		            _areasToUpdate.CopyTo(_bufferOfAreasToUpdate, 0, numToCopy);

		            for (int i = 0; i < numToCopy; i++)
		            {
			            if (!_areasToUpdate.Remove(_bufferOfAreasToUpdate[i]))
				            throw new Exception($"Failed to remove area {_bufferOfAreasToUpdate[i].area.Id} from SortedSet");
		            }

		            //_areasToUpdate.ExceptWith(new System.ArraySegment<AreaWithDistance>(_bufferOfAreasToUpdate, 0, numToCopy));

		            _indexOfBufferOfAreasToUpdate = 0;
		            _numElementsInBufferOfAreasToUpdate = numToCopy;

		            Profiler.EndSample();
	            }

	            // process 1 area from buffer

	            var areaWithDistance = _bufferOfAreasToUpdate[_indexOfBufferOfAreasToUpdate];
	            _indexOfBufferOfAreasToUpdate++;
	            _numElementsInBufferOfAreasToUpdate--;

	            this.UpdateArea(areaWithDistance);

            }

            Profiler.EndSample();

        }

        void UpdateArea(AreaWithDistance areaWithDistance)
        {
	        var area = areaWithDistance.area;
	        bool visible = area.WasVisibleInLastUpdate;

	        for (int i = 0; i < area.ObjectsInside.Count; i++)
	        {
		        var obj = area.ObjectsInside[i];

		        if (visible == obj.IsVisibleInMapSystem)
			        continue;

		        F.RunExceptionSafe(() =>
		        {
			        if (visible)
			        {
				        obj.Show(areaWithDistance.distance);
			        }
			        else
				        obj.UnShow();
		        });
	        }

        }
    }
}