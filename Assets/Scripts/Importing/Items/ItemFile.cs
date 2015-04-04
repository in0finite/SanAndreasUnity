using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SanAndreasUnity.Importing.Items.Placements;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Items
{
    public class SectionAttribute : Attribute
    {
        public readonly string Section;

        public SectionAttribute(string section)
        {
            Section = section;
        }
    }

    public abstract class Item
    {
        private readonly string[] _parts;

        public int Parts { get { return _parts.Length; } }

        protected Item(string line, bool commaSeparated = true)
        {
            var ws = new[] { ' ', '\t' };

            if (commaSeparated) {
                _parts = line.Split(',')
                    .SelectMany(x => x.Split(ws, StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Trim())
                    .ToArray();
            } else {
                _parts = line.Split(ws, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToArray();
            }
        }

        protected Item(BinaryReader reader) { }

        public string GetString(int index)
        {
            return _parts[index];
        }

        public int GetInt(int index)
        {
            return int.Parse(_parts[index]);
        }

        public int GetInt(int index, NumberStyles numberStyles)
        {
            return int.Parse(_parts[index], numberStyles);
        }

        public float GetSingle(int index)
        {
            return float.Parse(_parts[index]);
        }

        public double GetDouble(int index)
        {
            return double.Parse(_parts[index]);
        }
    }

    public abstract class Definition : Item
    {
        protected Definition(string line, bool commaSeparated = true)
            : base(line, commaSeparated) { }
    }

    public abstract class Placement : Item
    {
        protected Placement(string line, bool commaSeparated = true)
            : base(line, commaSeparated) { }

        protected Placement(BinaryReader reader)
            : base(reader) { }
    }

    public class ItemFile<TType>
        where TType : Item
    {
        private delegate TType ItemCtor(string line);

        private static readonly Dictionary<string, ItemCtor> _sCtors;

        static ItemFile()
        {
            _sCtors = new Dictionary<string, ItemCtor>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
                var attrib = (SectionAttribute) type.GetCustomAttributes(typeof(SectionAttribute), false).FirstOrDefault();
                if (attrib == null) continue;

                if (!typeof(TType).IsAssignableFrom(type)) continue;

                var ctor = type.GetConstructor(new[] {typeof (string)});

                var line = Expression.Parameter(typeof (string), "line");
                var call = Expression.New(ctor, line);
                var cast = Expression.Convert(call, typeof(TType));
                var lamb = Expression.Lambda<ItemCtor>(cast, line);

                _sCtors.Add(attrib.Section, lamb.Compile());
            }
        }

        private readonly Dictionary<string, List<TType>> _sections
            = new Dictionary<string, List<TType>>();

        public ItemFile(string path)
        {
            List<TType> curSection = null;
            ItemCtor curCtor = null;

            using (var reader = File.OpenText(path)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    line = line.Trim();

                    if (line.Length == 0) continue;
                    if (line.StartsWith("#")) continue;

                    if (curSection == null) {
                        line = line.ToLower();

                        if (_sections.ContainsKey(line)) {
                            curSection = _sections[line];
                        } else {
                            curSection = new List<TType>();
                            _sections.Add(line, curSection);
                        }

                        if (_sCtors.ContainsKey(line)) {
                            curCtor = _sCtors[line];
                        }

                        continue;
                    }

                    if (line.Equals("end")) {
                        curSection = null;
                        curCtor = null;
                        continue;
                    }

                    if (curCtor == null) continue;

                    curSection.Add(curCtor(line));
                }
            }
        }

        public ItemFile(Stream stream)
        {
            var reader = new BinaryReader(stream);

            if (reader.ReadString(4) != "bnry") throw new Exception("Not a binary IPL file.");

            var instCount = reader.ReadInt32();
            stream.Seek(12, SeekOrigin.Current);
            reader.ReadInt32(); // cars count
            stream.Seek(4, SeekOrigin.Current);
            var instOffset = reader.ReadInt32();
            stream.Seek(28, SeekOrigin.Current);
            reader.ReadInt32(); // cars offset

            var insts = new List<TType>();
            _sections.Add("inst", insts);

            stream.Seek(instOffset, SeekOrigin.Begin);
            for (var j = 0; j < instCount; ++j) {
                insts.Add((TType) (Item) new Instance(reader));
            }
        }

        public IEnumerable<TItem> GetSection<TItem>(string name)
            where TItem : TType
        {
            name = name.ToLower();

            return !_sections.ContainsKey(name)
                ? Enumerable.Empty<TItem>()
                : _sections[name].OfType<TItem>();
        }

        public IEnumerable<TItem> GetItems<TItem>()
            where TItem : TType
        {
            return _sections.SelectMany(x => x.Value.OfType<TItem>());
        }
    }
}
