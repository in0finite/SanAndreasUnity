using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Sections
{
    internal enum SectionType
    {
        Null = 0,
        Data = 1,
        String = 2,
        Extension = 3,
        Texture = 6,
        Material = 7,
        MaterialList = 8,
        FrameList = 14,
        Geometry = 15,
        Clump = 16,
        Atomic = 20,
        TextureNative = 21,
        TextureDictionary = 22,
        GeometryList = 26,
        MaterialSplit = 1294,
        Frame = 39056126
    }

    internal struct SectionHeader
    {
        public readonly SectionType Type;
        public readonly UInt32 Size;
        public readonly UInt16 Version;

        public SectionHeader(Stream stream)
        {
            var reader = new BinaryReader(stream);
            Type = (SectionType) reader.ReadUInt32();
            Size = reader.ReadUInt32();
            reader.ReadUInt16(); // Unknown
            Version = reader.ReadUInt16();
        }

        public override string ToString()
        {
            return String.Format("{0}, Size: {1}, Vers: {2}", Type, Size, Version);
        }
    }

    internal class SectionTypeAttribute : Attribute
    {
        public readonly SectionType Value;

        public SectionTypeAttribute(SectionType value)
        {
            Value = value;
        }
    }

    internal abstract class SectionData
    {
        private delegate SectionData CtorDelegate(SectionHeader header, Stream stream);

        private static readonly Dictionary<SectionType, CtorDelegate> _sDataCtors
            = new Dictionary<SectionType, CtorDelegate>();

        private static CtorDelegate CreateDelegate(Type type)
        {
            var ctor = type.GetConstructor(new [] { typeof(SectionHeader), typeof(Stream) });

            if (ctor == null) {
                throw new Exception(String.Format("Type {0} ") );
            }

            var header = Expression.Parameter(typeof(SectionHeader), "header");
            var stream = Expression.Parameter(typeof(Stream), "stream");

            var call = Expression.New(ctor, header, stream);
            var cast = Expression.Convert(call, typeof(SectionData));

            return Expression.Lambda<CtorDelegate>(cast, header, stream).Compile();
        }

        private static void FindTypes()
        {
            _sDataCtors.Clear();

            foreach (var t in Assembly.GetExecutingAssembly().GetTypes()) {
                if (t.BaseType != typeof (SectionData)) continue;

                var attrib = (SectionTypeAttribute) t.GetCustomAttributes(typeof(SectionTypeAttribute), false).FirstOrDefault();

                if (attrib != null) {
                    _sDataCtors.Add(attrib.Value, CreateDelegate(t));
                }
            }
        }

        public static SectionData FromStream(SectionHeader header, Stream stream)
        {
            return FromStream<SectionData>(header, stream);
        }

        public static T FromStream<T>(SectionHeader header, Stream stream)
            where T : SectionData
        {
            if (_sDataCtors.Count == 0) FindTypes();
            if (!_sDataCtors.ContainsKey(header.Type)) return null;
            return (T) _sDataCtors[header.Type](header, stream);
        }
    }

    internal struct Section
    {
        public readonly SectionHeader Header;
        public readonly SectionData Data;

        public SectionType Type { get { return Header.Type; } }

        public Section(Stream stream)
        {
            Header = new SectionHeader(stream);
            Data = SectionData.FromStream(Header, new FrameStream(stream, stream.Position, Header.Size));
        }

        public override string ToString()
        {
            return Header.ToString();
        }
    }
}
