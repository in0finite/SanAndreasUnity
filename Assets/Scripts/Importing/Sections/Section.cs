using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Sections
{
    internal struct SectionHeader
    {
        public static SectionHeader Read(Stream stream)
        {
            return new SectionHeader(stream);
        }

        public readonly UInt32 Type;
        public readonly UInt32 Size;
        public readonly UInt16 Version;

        private SectionHeader(Stream stream)
        {
            var reader = new BinaryReader(stream);
            Type = reader.ReadUInt32();
            Size = reader.ReadUInt32();
            reader.ReadUInt16(); // Unknown
            Version = reader.ReadUInt16();
        }

        public override string ToString()
        {
            return string.Format("{0}, Size: {1}, Vers: {2}", Type, Size, Version);
        }
    }

    internal class SectionTypeAttribute : Attribute
    {
        public readonly UInt32 Value;

        public SectionTypeAttribute(UInt32 value)
        {
            Value = value;
        }
    }

    internal abstract class SectionData
    {
        private delegate SectionData CtorDelegate(SectionHeader header, Stream stream);

        private static readonly Dictionary<UInt32, CtorDelegate> _sDataCtors
            = new Dictionary<UInt32, CtorDelegate>();

        private static CtorDelegate CreateDelegate(Type type)
        {
            var ctor = type.GetConstructor(new [] { typeof(SectionHeader), typeof(Stream) });

            if (ctor == null) {
                throw new Exception(string.Format("Type {0} ") );
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

        public static SectionData Read(SectionHeader header, Stream stream)
        {
            return Read<SectionData>(header, stream);
        }

        public static T Read<T>(SectionHeader header, Stream stream)
            where T : SectionData
        {
            if (_sDataCtors.Count == 0) FindTypes();
            if (!_sDataCtors.ContainsKey(header.Type)) return null;
            return (T) _sDataCtors[header.Type](header, stream);
        }
    }

    internal struct Section<TData>
        where TData : SectionData
    {
        public static Section<TData> Read(Stream stream)
        {
            return new Section<TData>(stream);
        }

        public static TData ReadData(Stream stream)
        {
            return new Section<TData>(stream).Data;
        }

        public readonly SectionHeader Header;
        public readonly TData Data;

        public UInt32 Type { get { return Header.Type; } }

        private Section(Stream stream)
        {
            Header = SectionHeader.Read(stream);
            
            var end = stream.Position + Header.Size;

            Data = SectionData.Read<TData>(Header, new FrameStream(stream, stream.Position, Header.Size));
            
            stream.Seek(end, SeekOrigin.Begin);
        }

        public override string ToString()
        {
            return Header.ToString();
        }
    }
}
