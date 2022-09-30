using SanAndreasUnity.Importing.Items.Placements;
using UGameCore.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
#if !ENABLE_IL2CPP
using System.Linq.Expressions;
#endif
using System.Reflection;
using UnityEngine;

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

    public abstract class ItemBase
    {
        private readonly string[] _parts;

        public int Parts { get { return _parts.Length; } }

        protected ItemBase(string line, bool commaSeparated = true)
        {
            var ws = new[] { ' ', '\t' };

            if (commaSeparated)
            {
                _parts = line.Split(',')
                    .SelectMany(x => x.Split(ws, StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Trim())
                    .ToArray();
            }
            else
            {
                _parts = line.Split(ws, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToArray();
            }
        }

        protected ItemBase(BinaryReader reader)
        {
        }

        public string GetString(int index)
        {
            return _parts[index];
        }

        public byte GetByte(int index)
        {
            return byte.Parse(_parts[index], CultureInfo.InvariantCulture);
        }

        public int GetInt(int index)
        {
            return int.Parse(_parts[index], CultureInfo.InvariantCulture);
        }

        public int GetInt(int index, NumberStyles numberStyles)
        {
            return int.Parse(_parts[index], numberStyles);
        }

        public float GetSingle(int index)
        {
            return float.Parse(_parts[index], CultureInfo.InvariantCulture);
        }

        public double GetDouble(int index)
        {
            return double.Parse(_parts[index], CultureInfo.InvariantCulture);
        }

        public UnityEngine.Vector3 GetUnityVec3(ref int index, bool invertYAndZ)
        {
            float x = GetSingle(index++);
            float y = GetSingle(index++);
            float z = GetSingle(index++);
            if (invertYAndZ)
                return new UnityEngine.Vector3(x, z, y);
            else
                return new UnityEngine.Vector3(x, y, z);
        }
    }

    public abstract class Definition : ItemBase
    {
        protected Definition(string line, bool commaSeparated = true)
            : base(line, commaSeparated) { }
    }

    public interface IObjectDefinition
    {
        int Id { get; }
    }

    public abstract class Placement : ItemBase
    {
        protected Placement(string line, bool commaSeparated = true)
            : base(line, commaSeparated) { }

        protected Placement(BinaryReader reader)
            : base(reader) { }
    }

    public class ItemFile<TType>
        where TType : ItemBase
    {
        private delegate TType ItemCtor(string line);

        private static readonly Dictionary<string, ItemCtor> _sCtors;

        static ItemFile()
        {
            _sCtors = new Dictionary<string, ItemCtor>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attrib = (SectionAttribute)type.GetCustomAttributes(typeof(SectionAttribute), false).FirstOrDefault();
                if (attrib == null) continue;

                if (!typeof(TType).IsAssignableFrom(type)) continue;

                var ctor = type.GetConstructor(new[] { typeof(string) });

#if !ENABLE_IL2CPP
                var line = Expression.Parameter(typeof(string), "line");
                var call = Expression.New(ctor, line);
                var cast = Expression.Convert(call, typeof(TType));
                var lamb = Expression.Lambda<ItemCtor>(cast, line);

                _sCtors.Add(attrib.Section, lamb.Compile());
#else
                _sCtors.Add(attrib.Section, line => (TType) ctor.Invoke(new object[] { line }));
#endif
            }
        }

        private readonly Dictionary<string, List<TType>> _sections
            = new Dictionary<string, List<TType>>();

        public ItemFile(string path)
        {
            string fileName = Path.GetFileName(path);

            if (!Archive.ArchiveManager.FileExists(fileName))
            {
                Debug.LogError($"Item file not found: {path}");
                return;
            }

            using (var stream = Archive.ArchiveManager.ReadFile(fileName))
            {
                using (var reader = new StreamReader(stream))
                {
                    Load(reader);
                }
            }
        }

        public ItemFile(StreamReader reader)
        {
            Load(reader);
        }

        void Load(StreamReader reader)
        {
            
            List<TType> curSection = null;
            ItemCtor curCtor = null;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var hashIndex = line.IndexOf('#');
                if (hashIndex != -1)
                {
                    line = line.Substring(0, hashIndex);
                }

                line = line.Trim();

                if (line.Length == 0) continue;

                if (curSection == null)
                {
                    line = line.ToLower();

                    if (_sections.ContainsKey(line))
                    {
                        curSection = _sections[line];
                    }
                    else
                    {
                        curSection = new List<TType>();
                        _sections.Add(line, curSection);
                    }

                    if (_sCtors.ContainsKey(line))
                    {
                        curCtor = _sCtors[line];
                    }

                    continue;
                }

                if (line.Equals("end"))
                {
                    curSection = null;
                    curCtor = null;
                    continue;
                }

                if (curCtor == null) continue;

                curSection.Add(curCtor(line));
            }
            
        }

        public ItemFile(Stream stream)
        {
            var reader = new BinaryReader(stream);

            if (reader.ReadString(4) != "bnry") throw new Exception("Not a binary IPL file.");

            var instCount = reader.ReadInt32();
            stream.Seek(12, SeekOrigin.Current);
            var carsCount = reader.ReadInt32();
            stream.Seek(4, SeekOrigin.Current);
            var instOffset = reader.ReadInt32();
            stream.Seek(28, SeekOrigin.Current);
            var carsOffset = reader.ReadInt32();

            var insts = new List<TType>();
            _sections.Add("inst", insts);

            stream.Seek(instOffset, SeekOrigin.Begin);
            for (var j = 0; j < instCount; ++j)
            {
                insts.Add((TType)(ItemBase)new Instance(reader));
            }

            var cars = new List<TType>();
            _sections.Add("cars", cars);

            stream.Seek(carsOffset, SeekOrigin.Begin);
            for (var j = 0; j < carsCount; ++j)
            {
                cars.Add((TType)(ItemBase)new ParkedVehicle(reader));
            }
        }

        public IEnumerable<TItem> GetSection<TItem>(string name)
            where TItem : TType
        {
            name = name.ToLower();

            return !_sections.ContainsKey(name)
                ? Enumerable.Empty<TItem>()
                : _sections[name].Cast<TItem>();
        }

        public IEnumerable<TItem> GetItems<TItem>()
            where TItem : TType
        {
            return _sections.SelectMany(x => x.Value.OfType<TItem>());
        }
    }
}