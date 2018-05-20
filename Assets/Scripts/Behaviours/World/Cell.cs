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

        public Division RootDivision { get; private set; }

        public List<int> CellIds = new List<int> { 0, 13 };

        public Camera PreviewCamera;

        public Transform Focus;
        public Water Water;

        public static Cell Instance = null;

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

        // variable used by MapObject class
        public float maxDrawDistance = 500;

        public bool loadParkedVehicles = true;
        private PlayerController _playerController;

        private void Awake()
        {
            Instance = this;
            _playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        }

        private void Start()
        {
            InvokeRepeating("UpdateDivisions", 0f, 0.1f);
        }

        private void Update()
        {
            if (RootDivision == null && Loader.HasLoaded)
            {
                RootDivision = Division.Create(transform);
                RootDivision.SetBounds(
                    new Vector2(-3000f, -3000f),
                    new Vector2(+3000f, +3000f));

                using (Utilities.Profiler.Start("Cell partitioning time"))
                {
                    var placements = Item.GetPlacements<Instance>(CellIds.ToArray());
                    var insts = placements.ToDictionary(x => x, x => StaticGeometry.Create());

                    UnityEngine.Debug.Log("Num static geometries " + placements.Count() + ".");
                    totalNumObjects = placements.Count();

                    foreach (var inst in insts)
                    {
                        inst.Value.Initialize(inst.Key, insts);
                    }

                    //    if (NetConfig.IsServer) {
                    if (loadParkedVehicles)
                    {
                        var parkedVehicles = Item.GetPlacements<ParkedVehicle>(CellIds.ToArray());
                        var cars = parkedVehicles.Select(x => VehicleSpawner.Create(x))
                            .Cast<MapObject>()
                            .ToArray();

                        UnityEngine.Debug.Log("Num parked vehicles " + parkedVehicles.Count() + ".");

                        RootDivision.AddRange(insts.Values.Cast<MapObject>().Concat(cars));
                    }
                    else
                    {
                        RootDivision.AddRange(insts.Values.Cast<MapObject>());
                    }
                }

                if (Water != null)
                {
                    using (Utilities.Profiler.Start("Water load time"))
                    {
                        Water.Initialize(new WaterFile(Config.GetPath("water_path")));
                    }
                }

                _timer = new Stopwatch();
                _leaves = RootDivision.ToList();
            }

            if (null == _leaves)
                return;

            _timer.Reset();
            _timer.Start();
            numLeavesLoadedThisFrame = 0;
            numObjectsLoadedThisFrame = 0;
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
            measuredTimes[2] = (float)_timer.Elapsed.TotalMilliseconds;
        }

        private void UpdateDivisions()
        {
            if (_leaves == null) return;

            numDivisionsUpdatedLoadOrder = 0;
            numMapObjectsUpdatedLoadOrder = 0;
            containingDivision = null;

            _timer.Reset();
            _timer.Start();
            var pos = Focus.position;
            var toLoad = false; // _leaves.Aggregate(false, (current, leaf) => current | leaf.RefreshLoadOrder(pos));
            foreach (Division leaf in _leaves)
            {
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
            measuredTimes[0] = (float)_timer.Elapsed.TotalMilliseconds;

            if (!toLoad) return;

            _timer.Reset();
            _timer.Start();
            _leaves.Sort();
            measuredTimes[1] = (float)_timer.Elapsed.TotalMilliseconds;
        }

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

        private void showWindow(int windowID)
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