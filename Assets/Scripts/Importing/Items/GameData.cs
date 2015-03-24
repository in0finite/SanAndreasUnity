using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Sections;

namespace SanAndreasUnity.Importing.Items
{
    internal class GameData
    {
        private readonly List<Zone> _zones;

        private readonly Dictionary<int, Object> _objects;
        private readonly Dictionary<string, List<Instance>> _instances;

        public GameData(string path)
        {
            _zones = new List<Zone>();
            _objects = new Dictionary<int, Object>();
            _instances = new Dictionary<string, List<Instance>>();

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

        private void ReadIde(string path)
        {
            var file = new ItemFile(ResourceManager.GetPath(path));
            foreach (var obj in file.GetSection<Object>("objs")) {
                _objects.Add(obj.Id, obj);
            }
        }

        private void ReadIpl(string path)
        {
            var file = new ItemFile(ResourceManager.GetPath(path));
            foreach (var zone in file.GetSection<Zone>("zone")) {
                _zones.Add(zone);
            }

            var insts = file.GetSection<Instance>("inst");
            if (!insts.Any()) return;

            var name = string.Format("{0}/{1}",
                Path.GetFileName(Path.GetDirectoryName(path)),
                Path.GetFileNameWithoutExtension(path)).ToLower();

            if (!_instances.ContainsKey(name)) {
                _instances.Add(name, new List<Instance>());
            }

            var list = new List<Instance>();
            list.AddRange(insts);

            var streamFormat = Path.GetFileNameWithoutExtension(path).ToLower() + "_stream{0}.ipl";
            var missed = 0;
            for (var i = 0;; ++i) {
                var streamPath = string.Format(streamFormat, i);
                if (!ResourceManager.FileExists(streamPath)) {
                    ++missed;

                    if (missed > 10) break;
                    continue;
                }

                file = new ItemFile(ResourceManager.ReadFile(streamPath));
                list.AddRange(file.GetSection<Instance>("inst"));
            }

            list.ResolveLod();
            
            _instances[name].AddRange(list.Where(x => !x.IsLod));
        }

        public Object GetObject(int id)
        {
            return !_objects.ContainsKey(id) ? null : _objects[id];
        }

        public IEnumerable<string> GetGroups()
        {
            return _instances.Keys;
        }

        public IEnumerable<string> GetGroups(string folder)
        {
            return _instances.Keys.Where(x => x.StartsWith(folder + "/"));
        }

        public IEnumerable<Instance> GetInstances(params string[] group)
        {
            return @group.Length == 0
                ? _instances.Values.SelectMany(x => x)
                : group.Select(x => x.ToLower()).SelectMany(x => _instances.ContainsKey(x)
                    ? _instances[x] : Enumerable.Empty<Instance>());
        }
    }
}
