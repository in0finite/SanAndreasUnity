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
        private List<Division> _leaves;

		private Dictionary<Instance, StaticGeometry> m_insts;
		private MapObject[] m_cars;

        public Division RootDivision { get; private set; }

        public List<int> CellIds = new List<int> { 0, 13 };

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
        private Division containingDivision = null;

        // variable used by Division class
        public float divisionLoadOrderDistanceFactor = 16;

        // variable used by Division class
        public float divisionRefreshDistanceDelta = 20;

		[Range(0.1f, 3f)]
		public float divisionsUpdateInterval = 0.3f;

        // variable used by MapObject class
        public float maxDrawDistance = 500;

        public bool loadParkedVehicles = true;
        


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
			if (RootDivision == null)
			{
				RootDivision = Division.Create(transform);
				RootDivision.SetBounds(
					new Vector2(-3000f, -3000f),
					new Vector2(+3000f, +3000f));
			}

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

		internal void AddMapObjectsToDivisions ()
		{
			var enumerable = m_insts.Values.Cast<MapObject> ();

			if (m_cars != null)
				enumerable = enumerable.Concat (m_cars);

			RootDivision.AddRange (enumerable);

		}

		internal void LoadWater ()
		{
			if (Water != null)
			{
				Water.Initialize(new WaterFile(Config.GetPath("water_path")));
			}
		}

		internal void FinalizeLoad ()
		{
			// set layer recursively for all game objects
			//	this.gameObject.SetLayerRecursive( this.gameObject.layer );

			_timer = new Stopwatch();
			_leaves = RootDivision.ToList();

		}


        private void Update()
        {

			if (!Loader.HasLoaded)
				return;

			//this.Setup ();

            if (null == _leaves)
                return;

            _timer.Reset();
            _timer.Start();
            numLeavesLoadedThisFrame = 0;
            numObjectsLoadedThisFrame = 0;

            this.focusPoints.RemoveDeadObjects();
            if (this.focusPoints.Count > 0)
            {
                // only update divisions loading if there are focus points - because otherwise, 
                // load order of divisions is not updated
                this.UpdateDivisionsLoading();
            }

            measuredTimes[2] = (float)_timer.Elapsed.TotalMilliseconds;

        }

        void UpdateDivisionsLoading()
        {
            foreach (var div in _leaves)
            {
                if (float.IsPositiveInfinity(div.LoadOrder))
                    break;

                numObjectsLoadedThisFrame += div.LoadWhile(() => _timer.Elapsed.TotalSeconds < 1d / 60d);

                if (_timer.Elapsed.TotalSeconds >= 1d / 60d)
                {
                    //	break;
                }
                else
                {
                    numLeavesLoadedThisFrame++;
                }
            }
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
			
            if (_leaves == null) return;

			this.focusPoints.RemoveAll (t => null == t);

			if (this.focusPoints.Count < 1)
				return;

            numDivisionsUpdatedLoadOrder = 0;
            numMapObjectsUpdatedLoadOrder = 0;
            containingDivision = null;

            _timer.Reset();
            _timer.Start();

			List<Vector3> positions = this.focusPoints.Select (f => f.position).ToList ();

			bool toLoad = false; // _leaves.Aggregate(false, (current, leaf) => current | leaf.RefreshLoadOrder(pos));
            
			UnityEngine.Profiling.Profiler.BeginSample ("Update divisions", this);

			foreach (Division leaf in _leaves)
            {
				Vector3 pos = leaf.GetClosestPosition (positions);

                int count = 0;
                toLoad |= leaf.RefreshLoadOrder(pos, out count);
                if (count > 0)
                {
                    numDivisionsUpdatedLoadOrder++;
                    numMapObjectsUpdatedLoadOrder += count;
                }

                if (null == containingDivision && leaf.Contains(pos))
                {
                    containingDivision = leaf;
                }
            }

			UnityEngine.Profiling.Profiler.EndSample ();

            measuredTimes[0] = (float)_timer.Elapsed.TotalMilliseconds;

            if (!toLoad) return;

            _timer.Reset();
            _timer.Start();
			UnityEngine.Profiling.Profiler.BeginSample ("Sort leaves", this);
            _leaves.Sort();
			UnityEngine.Profiling.Profiler.EndSample ();
            measuredTimes[1] = (float)_timer.Elapsed.TotalMilliseconds;
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
            GUILayout.Label("total num divisions " + (null == _leaves ? 0 : _leaves.Count));
            GUILayout.Label("total num objects " + totalNumObjects);
            GUILayout.Label("geometry parts loaded " + SanAndreasUnity.Importing.Conversion.Geometry.NumGeometryPartsLoaded);
            GUILayout.Label("num objects in current division " + (containingDivision != null ? containingDivision.NumObjects : 0));
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