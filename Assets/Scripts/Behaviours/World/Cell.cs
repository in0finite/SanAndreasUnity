using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Placements;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

//using Facepunch.Networking;

namespace SanAndreasUnity.Behaviours.World
{
    public class Cell : MonoBehaviour
    {
        private Stopwatch _timer;

        private Dictionary<Instance, StaticGeometry> m_insts;
		private MapObject[] m_cars;
        private List<EntranceExitMapObject> m_enexes;

        private List<int> CellIds = Enumerable.Range(0, 19).ToList();

        public bool HasExterior => this.CellIds.Contains(0);

        public Camera PreviewCamera;

		private List<(long id, Transform transform)> _focusPoints = new List<(long id, Transform transform)>();

        public Water Water;

		public static Cell Instance { get ; private set; }

        // Statistics

        private int totalNumObjects = 0;
        private int numLeavesLoadedThisFrame = 0;
        private int numObjectsLoadedThisFrame = 0;
        private float[] measuredTimes = new float[3];
        private int numDivisionsUpdatedLoadOrder = 0;
        private int numMapObjectsUpdatedLoadOrder = 0;

        public float divisionLoadOrderDistanceFactor = 16;

        public float divisionRefreshDistanceDelta = 20;

		[Range(0.1f, 3f)]
		public float divisionsUpdateInterval = 0.3f;

        public float maxDrawDistance = 500;

        public float[] drawDistancesPerLayers = new float[] { 301, 801, 1501 };

        private WorldSystemWithDistanceLevels<MapObject> _worldSystem;

        public uint yWorldSystemSize = 25000;
        public ushort yWorldSystemNumAreas = 3;

        public ushort[] xzWorldSystemNumAreasPerDrawDistanceLevel = { 100, 100, 100 };

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

        private void Start()
        {
            //InvokeRepeating("UpdateDivisions", 0f, 0.1f);
			StartCoroutine( this.UpdateDivisionsCoroutine () );
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

			totalNumObjects = m_insts.Count;

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
				m_cars = parkedVehicles.Select (x => VehicleSpawner.Create (x))
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

			_timer = new Stopwatch();

		}


		private void OnAreaChangedVisibility(WorldSystem<MapObject>.Area area, bool visible)
		{
			if (null == area.ObjectsInside)
				return;

			for (int i = 0; i < area.ObjectsInside.Count; i++)
			{
				var obj = area.ObjectsInside[i];
				F.RunExceptionSafe(() =>
				{
					if (visible)
						obj.Show();
					else
						obj.UnShow();
				});
			}

		}

		public void RegisterFocusPoint(Transform tr, float revealRadius)
		{
			if (!_focusPoints.Exists(f => f.transform == tr))
			{
				var registeredFocusPoint = _worldSystem.RegisterFocusPoint(revealRadius, tr.position);
				_focusPoints.Add((registeredFocusPoint, tr));
			}
		}

		public void RegisterFocusPoint(Transform tr) => this.RegisterFocusPoint(tr, this.maxDrawDistance);

		public void UnRegisterFocusPoint(Transform tr)
		{
			int index = _focusPoints.FindIndex(f => f.transform == tr);
			if (index < 0)
				return;

			// var temp = _focusPoints[index];
			// temp.transform = null; // it will be removed in next Update()
			// _focusPoints[index] = temp;

			_worldSystem.UnRegisterFocusPoint(_focusPoints[index].id);
			_focusPoints.RemoveAt(index);
		}


        public IEnumerable<EntranceExit> GetEnexesFromLoadedInteriors()
        {
            int[] loadedInteriors = this.CellIds.Where(id => id != 0 && id != 13).ToArray();
            foreach(var enex in Importing.Items.Item.Enexes.Where(enex => loadedInteriors.Contains(enex.TargetInterior)))
            {
                yield return enex;
            }
        }

        public static TransformDataStruct GetEnexExitTransform(EntranceExit enex)
        {
            return new TransformDataStruct(enex.ExitPos + Vector3.up * 0.2f, Quaternion.Euler(0f, enex.ExitAngle, 0f));
        }

        public static TransformDataStruct GetEnexEntranceTransform(EntranceExit enex)
        {
            return new TransformDataStruct(enex.EntrancePos + Vector3.up * 0.2f, Quaternion.Euler(0f, enex.EntranceAngle, 0f));
        }


        private void Update()
        {

			if (!Loader.HasLoaded)
				return;

			//this.Setup ();

			_timer.Reset();
            _timer.Start();
            numLeavesLoadedThisFrame = 0;
            numObjectsLoadedThisFrame = 0;

            UnityEngine.Profiling.Profiler.BeginSample("Update focus points");
            this._focusPoints.RemoveAll(f =>
            {
	            if (null == f.transform)
	            {
		            UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.UnRegisterFocusPoint()");
		            _worldSystem.UnRegisterFocusPoint(f.id);
		            UnityEngine.Profiling.Profiler.EndSample();
		            return true;
	            }

	            _worldSystem.FocusPointChangedPosition(f.id, f.transform.position);

	            return false;
            });
            UnityEngine.Profiling.Profiler.EndSample();

            if (this._focusPoints.Count > 0)
            {
                // only update divisions loading if there are focus points - because otherwise, 
                // load order of divisions is not updated

            }

            UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.Update()");
            _worldSystem.Update();
            UnityEngine.Profiling.Profiler.EndSample();

            measuredTimes[2] = (float)_timer.Elapsed.TotalMilliseconds;

        }

        System.Collections.IEnumerator UpdateDivisionsCoroutine ()
		{

			while (true)
			{
				// wait 100 ms
				float timePassed = 0;
				while (timePassed < this.divisionsUpdateInterval)
				{
					yield return null;
					timePassed += Time.unscaledDeltaTime;
				}

				F.RunExceptionSafe (() => this.UpdateDivisions ());

			}

		}

        private void UpdateDivisions()
        {
			if (!Loader.HasLoaded)
				return;

			numDivisionsUpdatedLoadOrder = 0;
            numMapObjectsUpdatedLoadOrder = 0;

            _timer.Reset();
            _timer.Start();


        }

        /*
        private static Rect windowRect = new Rect(10, 10, 250, 330);
        private const int windowID = 0;

        private void OnGUI()
        {
            if (!Loader.HasLoaded)
                return;

            if (!PlayerController._showMenu)
                return;

            windowRect = GUILayout.Window(windowID, windowRect, showWindow, "World statistics");
        }
        */

        public void showWindow(int windowID)
        {
            GUILayout.Label("draw distance " + this.maxDrawDistance);
            GUILayout.Label("num focus points " + this._focusPoints.Count);
            GUILayout.Label("total num objects " + totalNumObjects);
            GUILayout.Label("geometry parts loaded " + SanAndreasUnity.Importing.Conversion.Geometry.NumGeometryPartsLoaded);
            GUILayout.Label("num TOBJ objects " + StaticGeometry.TimedObjects.Count);
            GUILayout.Label("num active objects with lights " + StaticGeometry.ActiveObjectsWithLights.Count);
            GUILayout.Label("num divisions updated " + numDivisionsUpdatedLoadOrder);
            GUILayout.Label("num objects updated " + numMapObjectsUpdatedLoadOrder);
            GUILayout.Label("num divisions loading this frame " + numLeavesLoadedThisFrame);
            GUILayout.Label("num objects loading this frame " + numObjectsLoadedThisFrame);

            GUILayout.Space(10);

            string[] timeNames = new string[] { "refresh load order ", "sort ", "load / update display " };
            int i = 0;
            foreach (float time in measuredTimes)
            {
                GUILayout.Label(timeNames[i] + Mathf.RoundToInt(time));
                i++;
            }

            GUI.DragWindow();
        }
    }
}