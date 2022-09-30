using UGameCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !ENABLE_IL2CPP
using System.Linq.Expressions;
#endif
using System.Reflection;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public struct SectionHeader
    {
        public static SectionHeader Read(Stream stream, SectionData parent = null)
        {
            return new SectionHeader(stream, parent);
        }

        private readonly SectionData _parent;

        public readonly UInt32 Type;
        public readonly UInt32 Size;
        public readonly UInt16 Version;

        private SectionHeader(Stream stream, SectionData parent)
        {
            _parent = parent;

            var reader = new BinaryReader(stream);
            Type = reader.ReadUInt32();
            Size = reader.ReadUInt32();
            reader.ReadUInt16(); // Unknown
            Version = reader.ReadUInt16();
        }

        public SectionData GetParent()
        {
            return _parent;
        }

        public TSection GetParent<TSection>()
            where TSection : SectionData
        {
            return (TSection)_parent;
        }

        public override string ToString()
        {
            return string.Format("{0}, Size: {1}, Vers: {2}", Type, Size, Version);
        }
    }

    public class SectionTypeAttribute : Attribute
    {
        public readonly UInt32 Value;

        public SectionTypeAttribute(UInt32 value)
        {
            Value = value;
        }
    }

    public abstract class SectionData
    {
        private delegate SectionData CtorDelegate(SectionHeader header, Stream stream);

        private static readonly Dictionary<UInt32, CtorDelegate> _sDataCtors
            = new Dictionary<UInt32, CtorDelegate>();

        private static CtorDelegate CreateDelegate(Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(SectionHeader), typeof(Stream) });

            if (ctor == null)
            {
                throw new Exception(string.Format("Type {0} "));
            }

#if !ENABLE_IL2CPP
            var header = Expression.Parameter(typeof(SectionHeader), "header");
            var stream = Expression.Parameter(typeof(Stream), "stream");

            var call = Expression.New(ctor, header, stream);
            var cast = Expression.Convert(call, typeof(SectionData));

            return Expression.Lambda<CtorDelegate>(cast, header, stream).Compile();
#else
            return (header, stream) => (SectionData) ctor.Invoke(new object[] {header, stream});
#endif
        }

        private static void FindTypes()
        {
            _sDataCtors.Clear();

            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.BaseType != typeof(SectionData)) continue;

                var attrib = (SectionTypeAttribute)t.GetCustomAttributes(typeof(SectionTypeAttribute), false).FirstOrDefault();

                if (attrib != null)
                {
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
            if (!_sDataCtors.ContainsKey(header.Type))
            {
                if (typeof(T) == typeof(SectionData)) return null;
                throw new Exception(string.Format("Unexpected section header {0}.", header.Type));
            }
            return (T)_sDataCtors[header.Type](header, stream);
        }

        private readonly SectionHeader _header;
        private readonly Stream _stream;

        protected SectionData(SectionHeader header, Stream stream)
        {
            _header = header;
            _stream = stream;
        }

        protected TSection ReadSection<TSection>(SectionData parent = null)
            where TSection : SectionData
        {
            return Section<TSection>.ReadData(_stream, parent ?? this);
        }
    }

    public struct Section<TData>
        where TData : SectionData
    {
        public static Section<TData> Read(Stream stream, SectionData parent = null)
        {
            return new Section<TData>(stream, parent);
        }

        public static TData ReadData(Stream stream, SectionData parent = null)
        {
            return new Section<TData>(stream, parent).Data;
        }

        public readonly SectionHeader Header;
        public readonly TData Data;

        public UInt32 Type { get { return Header.Type; } }

        private Section(Stream stream, SectionData parent)
        {
            Header = SectionHeader.Read(stream, parent);

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