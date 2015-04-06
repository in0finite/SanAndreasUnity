using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Items;
using System.Linq;
using System.Diagnostics;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Items.Placements;
using System.IO;
using SanAndreasUnity.Behaviours.Player;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Vehicles;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.World
{
    public class Cell : MonoBehaviour
    {
        private Stopwatch _timer;
        private List<Division> _leaves;

        public Division RootDivision { get; private set; }

        public List<int> CellIds = new List<int> { 0, 13 };

        public PlayerController Player;
        public Water Water;

        void Update()
        {
            if (RootDivision == null && Loader.HasLoaded) {
                RootDivision = Division.Create(transform);
                RootDivision.SetBounds(
                    new Vector2(-3000f, -3000f),
                    new Vector2(+3000f, +3000f));

                using (Utilities.Profiler.Start("Cell partitioning time")) {
                    var insts = Item.GetPlacements<Instance>(CellIds.ToArray())
                        .ToDictionary(x => x, x => StaticGeometry.Create());

                    foreach (var inst in insts) {
                        inst.Value.Initialize(inst.Key, insts);
                    }

                    var cars = Item.GetPlacements<ParkedVehicle>(CellIds.ToArray())
                        .Select(x => VehicleSpawner.Create(x))
                        .Cast<MapObject>()
                        .ToArray();

                    RootDivision.AddRange(insts.Values.Cast<MapObject>().Concat(cars));
                }

                if (Water != null) {
                    using (Utilities.Profiler.Start("Water load time")) {
                        Water.Initialize(new WaterFile(Config.GetPath("water_path")));
                    }
                }

                _timer = new Stopwatch();
                _leaves = RootDivision.ToList();
            }

            if (_leaves == null) return;

            var pos = Player.transform.position;
            var toLoad = _leaves.Aggregate(false, (current, leaf) => current | leaf.RefreshLoadOrder(pos));

            if (!toLoad) return;

            _leaves.Sort();

            _timer.Reset();
            _timer.Start();

            foreach (var div in _leaves) {
                if (float.IsPositiveInfinity(div.LoadOrder)) break;
                if (!div.LoadWhile(() => _timer.Elapsed.TotalSeconds < 1d / 60d)) break;
            }
        }
    }
}
