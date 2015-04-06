using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Items.Placements;
using SanAndreasUnity.Importing.Items.Definitions;
using UnityEngine;

namespace SanAndreasUnity.Importing.Items
{
    public static class GameData
    {
        private static readonly List<Zone> _zones = new List<Zone>();

        private static readonly Dictionary<int, IObjectDefinition> _definitions
            = new Dictionary<int,IObjectDefinition>();
        private static readonly Dictionary<int, List<Placement>> _placements
            = new Dictionary<int,List<Placement>>();

        public static bool HasLoaded { get; private set; }

        public static void Load(string path)
        {
            if (HasLoaded) {
                _zones.Clear();
                _definitions.Clear();
                _placements.Clear();
            }

            HasLoaded = true;

            var ws = new[] {' ', '\t'};

            using (var reader = File.OpenText(path)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    line = line.Trim();

                    if (line.Length == 0) continue;
                    if (line.StartsWith("#")) continue;

                    var index = line.IndexOfAny(ws);
                    if (index == -1) continue;

                    var type = line.Substring(0, index);
                    var args = line.Substring(index).TrimStart();

                    switch (type.ToLower()) {
                        case "ide":
                            ReadIde(args);
                            break;
                        case "ipl":
                            ReadIpl(args);
                            break;
                    }
                }
            }
        }

        public static void ReadIde(string path)
        {
            var file = new ItemFile<Definition>(ArchiveManager.GetPath(path));
            foreach (var obj in file.GetItems<Definition>().OfType<IObjectDefinition>()) {
                _definitions.Add(obj.Id, obj);
            }
        }

        public static void ReadIpl(string path)
        {
            var file = new ItemFile<Placement>(ArchiveManager.GetPath(path));
            foreach (var zone in file.GetSection<Zone>("zone")) {
                _zones.Add(zone);
            }

            var insts = file.GetSection<Instance>("inst");

            var list = new List<Instance>();
            list.AddRange(insts);

            var cars = new List<ParkedVehicle>(file.GetSection<ParkedVehicle>("cars"));

            var streamFormat = Path.GetFileNameWithoutExtension(path).ToLower() + "_stream{0}.ipl";
            var missed = 0;
            for (var i = 0;; ++i) {
                var streamPath = string.Format(streamFormat, i);
                if (!ArchiveManager.FileExists(streamPath)) {
                    ++missed;

                    if (missed > 10) break;
                    continue;
                }

                file = new ItemFile<Placement>(ArchiveManager.ReadFile(streamPath));
                list.AddRange(file.GetSection<Instance>("inst"));
                cars.AddRange(file.GetSection<ParkedVehicle>("cars"));
            }

            list.ResolveLod();

            var lastCell = -1;
            foreach (var inst in list) {
                var cell = inst.CellId & 0xff;
                if (lastCell != cell && !_placements.ContainsKey(lastCell = cell)) {
                    _placements.Add(cell, new List<Placement>());
                }

                _placements[cell].Add(inst);
            }

            if (!_placements.ContainsKey(0)) {
                _placements.Add(0, new List<Placement>());
            }

            _placements[0].AddRange(cars.Cast<Placement>());
        }

        public static TDefinition GetDefinition<TDefinition>(int id)
            where TDefinition : Definition, IObjectDefinition
        {
            return !_definitions.ContainsKey(id) ? null : (TDefinition) _definitions[id];
        }

        public static IEnumerable<TPlacement> GetPlacements<TPlacement>(params int[] cellIds)
            where TPlacement : Placement
        {
            return cellIds.SelectMany(x => _placements.ContainsKey(x)
                ? _placements[x].OfType<TPlacement>() : Enumerable.Empty<TPlacement>());
        }
    }
}
