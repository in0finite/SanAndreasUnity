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

		public List<Transform> focusPoints = new List<Transform> ();

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
		}

		internal void InitStaticGeometry ()
		{
			foreach (var inst in m_insts)
			{
				inst.Value.Initialize(inst.Key, m_insts);
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
                m_enexes.Add(EntranceExitMapObject.Create(enex));
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

            this.focusPoints.RemoveDeadObjects();
            if (this.focusPoints.Count > 0)
            {
                // only update divisions loading if there are focus points - because otherwise, 
                // load order of divisions is not updated

            }

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

			this.focusPoints.RemoveAll (t => null == t);

			if (this.focusPoints.Count < 1)
				return;

            numDivisionsUpdatedLoadOrder = 0;
            numMapObjectsUpdatedLoadOrder = 0;

            _timer.Reset();
            _timer.Start();

			List<Vector3> positions = this.focusPoints.Select (f => f.position).ToList ();

			bool toLoad = false; // _leaves.Aggregate(false, (current, leaf) => current | leaf.RefreshLoadOrder(pos));
            
			UnityEngine.Profiling.Profiler.BeginSample ("Update divisions", this);

			UnityEngine.Profiling.Profiler.EndSample ();

            measuredTimes[0] = (float)_timer.Elapsed.TotalMilliseconds;

            if (!toLoad) return;
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
            GUILayout.Label("num focus points " + this.focusPoints.Count);
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