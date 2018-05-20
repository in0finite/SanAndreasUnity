#region MessageTable

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Debug = UnityEngine.Debug;

#if NET_4_0
using System.Collections.Concurrent;
#endif

namespace ProtoBuf
{
    /// <summary>
    /// Root message types (ones that may not be containted within another message)
    /// must implement this interface.
    /// </summary>
    public interface IMessage
    {
        void FromProto(Stream stream);

        void ToProto(Stream stream);

        byte[] ToProtoBytes();

        void FromJson(JObject obj);

        string ToJson();
    }

    /// <summary>
    /// Used to hard-code a message type identifier for message types used in a MessageTable.
    /// If this attribute is omitted the type will be provided an identifier automatically.
    /// </summary>
    public class MessageIdentAttribute : Attribute
    {
        public uint Ident { get; private set; }

        public MessageIdentAttribute(uint ident)
        {
            if (ident == 0) throw new ArgumentException("Identifier must be larger than 0.", "ident");

            Ident = ident;
        }
    }

#if PROTOBUF

    package ProtoBuf;

    message MessageTableSchema
    {
        repeated MessageTableEntry Entries = 1;
    }

    message MessageTableEntry
    {
        required uint32 Ident = 1;
        required string TypeName = 2;
    }

#endif

    /// <summary>
    /// Assigns unique identifiers to messages implementing a specified
    /// interface, and provides methods for serializing and deserializing
    /// messages while preserving their types.
    /// </summary>
    /// <typeparam name="TBaseInterface">
    /// Interface messages types in this table must implement.
    /// </typeparam>
    public class MessageTable<TBaseInterface>
        where TBaseInterface : IMessage
    {
        private const uint MaxIdent = (1 << (7 * 4)) - 1;

        private class Entry
        {
            public Type Type { get; private set; }

            public Func<TBaseInterface> Constructor { get; private set; }

            public uint Ident { get; internal set; }

            public Entry(Type type, uint id = 0, ConstructorInfo ctor = null)
            {
                Type = type;
                Ident = id;

                ctor = ctor ?? type.GetConstructor(new Type[0]);

                var call = Expression.New(ctor);
                Constructor = Expression.Lambda<Func<TBaseInterface>>(call).Compile();
            }
        }

        /// <summary>
        /// Constructs an empty message table.
        /// </summary>
        public static MessageTable<TBaseInterface> CreateEmpty()
        {
            return new MessageTable<TBaseInterface>();
        }

        /// <summary>
        /// Creates a message table populated with assigned identifiers for message types
        /// in the specified assemblies.
        /// </summary>
        /// <param name="assemblies">
        /// Assemblies to look for message types in.
        /// If omitted, the calling assembly is used.
        /// </param>
        public static MessageTable<TBaseInterface> CreatePopulated(params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                assemblies = new[] { Assembly.GetCallingAssembly() };
            }

            var table = CreateEmpty();
            table.Generate(assemblies, false);

            return table;
        }

        private readonly Dictionary<uint, Entry> _entries;
        private readonly Dictionary<Type, uint> _idents;

        private MessageTableSchema _schema;

        /// <summary>
        /// True if this table has assigned identifiers.
        /// </summary>
        public bool IsPopulated { get { return _entries != null; } }

        private MessageTable()
        {
            _entries = new Dictionary<uint, Entry>();
            _idents = new Dictionary<Type, uint>();

            if (!BitConverter.IsLittleEndian)
            {
                throw new NotImplementedException("Not implemented for Big-Endian systems.");
            }
        }

        private void UpdateIdentDict()
        {
            _idents.Clear();

            foreach (var entry in _entries)
            {
                _idents.Add(entry.Value.Type, entry.Key);
            }
        }

        /// <summary>
        /// Populate the message table.
        /// </summary>
        /// <param name="minimal">
        /// Only populate with message types that have hard-coded identifiers,
        /// this should be performed by clients before receiving a schema from
        /// the server.
        /// </param>
        internal void Generate(IEnumerable<Assembly> asms, bool minimal)
        {
            var iMessage = typeof(TBaseInterface);
            var ctorParams = new Type[0];

            var valid = asms.SelectMany(x => x.GetTypes())
                .Where(x => !x.IsAbstract)
                .Where(x => x.GetInterfaces().Contains(iMessage))
                .Select(x => new { type = x, ctor = x.GetConstructor(ctorParams), attr = x.GetAttribute<MessageIdentAttribute>(false) })
                .Where(x => x.ctor != null && (!minimal || x.attr != null))
                .OrderBy(x => x.attr == null ? uint.MaxValue : x.attr.Ident)
                .Select(x => new Entry(x.type, x.attr != null ? x.attr.Ident : 0, x.ctor))
                .ToArray();

            _entries.Clear();

            uint nextIdent = 1;
            foreach (var entry in valid)
            {
                bool nextIdentConflict;

                if (entry.Ident > 0)
                {
                    if (_entries.ContainsKey(entry.Ident))
                    {
                        throw new Exception(String.Format("Message identifier conflict: "
                            + "'{0}' and '{1}' share the same hard-coded identifier.",
                            entry.Type, _entries[entry.Ident].Type));
                    }

                    nextIdentConflict = entry.Ident == nextIdent;
                }
                else
                {
                    entry.Ident = nextIdent;
                    nextIdentConflict = true;
                }

                if (entry.Ident > MaxIdent)
                {
                    throw new Exception(String.Format("Message identifier is too large: '{0}'.", entry.Ident));
                }

                _entries.Add(entry.Ident, entry);

                while (nextIdentConflict)
                {
                    nextIdentConflict = _entries.ContainsKey(++nextIdent);
                }
            }

            foreach (var entry in _entries)
            {
                Debug.LogFormat("{0}: {1}", entry.Key, entry.Value.Type.Name);
            }

            UpdateIdentDict();
        }

        // HACK
        private String TranslateTypeName(String name)
        {
            return name.StartsWith("Arcade.") ? name.Substring(7) : name;
        }

        // HACK
        private String InjectPackage(String name, String package)
        {
            var dot = name.LastIndexOf('.');
            return String.Format("{0}.{1}{2}", name.Substring(0, dot), package, name.Substring(dot));
        }

        /// <summary>
        /// Load identifier assignments from a schema object.
        /// </summary>
        internal void FromSchema(MessageTableSchema schema)
        {
            var asm = Assembly.GetExecutingAssembly();

            var entries = schema.Entries
                .Select(x =>
                {
                    var name = TranslateTypeName(x.TypeName);
                    var type = asm.GetType(TranslateTypeName(x.TypeName))
                        ?? asm.GetType(InjectPackage(name, "Cabinet"))
                        ?? asm.GetType(InjectPackage(name, "Player"));
                    return new { type = type, ident = x.Ident };
                })
                .Select(x => new Entry(x.type, x.ident))
                .ToArray();

            _entries.Clear();

            foreach (var entry in entries)
            {
                _entries.Add(entry.Ident, entry);
            }

            UpdateIdentDict();
        }

        private static void WriteIdent(Stream stream, uint ident)
        {
            for (var i = 0; i < 4; ++i)
            {
                var val = (byte)((ident >> (7 * i)) & 0x7f);
                var flag = ident >> (7 * i + 7) > 0;

                stream.WriteByte((byte)(val | (flag ? 0x80 : 0x00)));

                if (!flag) break;
            }
        }

        private static uint ReadIdent(Stream stream)
        {
            uint ident = 0;

            for (var i = 0; i < 4; ++i)
            {
                var encoded = stream.ReadByte();
                var val = encoded & 0x7f;
                var flag = (encoded & 0x80) == 0x80;

                ident |= (uint)(val << (7 * i));

                if (!flag) break;
            }

            return ident;
        }

        public uint GetIdent(TBaseInterface message)
        {
            var type = message != null ? message.GetType() : null;
            return message == null || !_idents.ContainsKey(type) ? 0 : _idents[type];
        }

        private Entry GetEntry(uint id)
        {
            return id == 0 || !_entries.ContainsKey(id) ? null : _entries[id];
        }

        public byte[] Serialize(TBaseInterface message)
        {
            using (var stream = new MemoryStream())
            {
                Serialize(stream, message);
                return stream.ToArray();
            }
        }

        public void Serialize(Stream stream, TBaseInterface message)
        {
            var id = GetIdent(message);

            WriteIdent(stream, id);

            if (id > 0) message.ToProto(stream);
        }

        public string SerializeJson(TBaseInterface message)
        {
            var id = GetIdent(message);
            return "{\"id\":" + id + (id > 0 ? ",\"value\":" + message.ToJson() : "") + "}";
        }

        public TBaseInterface Deserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Deserialize(stream);
            }
        }

        public TMessage Deserialize<TMessage>(byte[] bytes)
            where TMessage : TBaseInterface
        {
            using (var stream = new MemoryStream(bytes))
            {
                return (TMessage)Deserialize(stream);
            }
        }

        public TBaseInterface Deserialize(Stream stream)
        {
            var id = ReadIdent(stream);

            var entry = GetEntry(id);
            if (entry == null) return default(TBaseInterface);

            var msg = entry.Constructor();
            msg.FromProto(stream);

            return msg;
        }

        public TMessage Deserialize<TMessage>(Stream stream)
            where TMessage : TBaseInterface
        {
            return (TMessage)Deserialize(stream);
        }

        public TBaseInterface Deserialize(JObject obj)
        {
            var id = (uint)obj["id"];

            var entry = GetEntry(id);
            if (entry == null) return default(TBaseInterface);

            var msg = entry.Constructor();
            msg.FromJson((JObject)obj["value"]);

            return msg;
        }

        /// <summary>
        /// Construct a schema object that can be used to load identifier assignments.
        /// </summary>
        public MessageTableSchema GetSchema()
        {
            if (_schema != null) return _schema;

            _schema = new MessageTableSchema
            {
                Entries = _entries.Values
                    .OrderBy(x => x.Ident)
                    .Select(x => new MessageTableEntry { Ident = x.Ident, TypeName = x.Type.FullName })
                    .ToList()
            };

            return _schema;
        }
    }
}

#endregion MessageTable

#region ProtocolParser

#if NET_4_0
#endif

//
//  Read/Write string and byte arrays
//
namespace SilentOrbit.ProtocolBuffers
{
    public static partial class ProtocolParser
    {
        public static string ReadString(Stream stream)
        {
            return Encoding.UTF8.GetString(ReadBytes(stream));
        }

        /// <summary>
        /// Reads a length delimited byte array
        /// </summary>
        public static byte[] ReadBytes(Stream stream)
        {
            //VarInt length
            int length = (int)ReadUInt32(stream);

            //Bytes
            byte[] buffer = new byte[length];
            int read = 0;
            while (read < length)
            {
                int r = stream.Read(buffer, read, length - read);
                if (r == 0)
                    throw new ProtocolBufferException("Expected " + (length - read) + " got " + read);
                read += r;
            }
            return buffer;
        }

        /// <summary>
        /// Skip the next varint length prefixed bytes.
        /// Alternative to ReadBytes when the data is not of interest.
        /// </summary>
        public static void SkipBytes(Stream stream)
        {
            int length = (int)ReadUInt32(stream);
            if (stream.CanSeek)
                stream.Seek(length, SeekOrigin.Current);
            else
                ReadBytes(stream);
        }

        public static void WriteString(Stream stream, string val)
        {
            WriteBytes(stream, Encoding.UTF8.GetBytes(val));
        }

        /// <summary>
        /// Writes length delimited byte array
        /// </summary>
        public static void WriteBytes(Stream stream, byte[] val)
        {
            WriteUInt32(stream, (uint)val.Length);
            stream.Write(val, 0, val.Length);
        }
    }

    [Obsolete("Renamed to PositionStream")]
    public class StreamRead : PositionStream
    {
        public StreamRead(Stream baseStream) : base(baseStream)
        {
        }
    }

    /// <summary>
    /// Wrapper for streams that does not support the Position property.
    /// Adds support for the Position property.
    /// </summary>
    public class PositionStream : Stream
    {
        private Stream stream;

        /// <summary>
        /// Bytes left to read
        /// </summary>
        public int BytesRead { get; private set; }

        /// <summary>
        /// Define how many bytes are allowed to read
        /// </summary>
        /// <param name='baseStream'>
        /// Base stream.
        /// </param>
        /// <param name='maxLength'>
        /// Max length allowed to read from the stream.
        /// </param>
        public PositionStream(Stream baseStream)
        {
            stream = baseStream;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = stream.Read(buffer, offset, count);
            BytesRead += read;
            return read;
        }

        public override int ReadByte()
        {
            int b = stream.ReadByte();
            BytesRead += 1;
            return b;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return BytesRead;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Close()
        {
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            stream.Dispose();
            base.Dispose(disposing);
        }
    }
}

#endregion ProtocolParser

#region ProtocolParserExceptions

//
// Exception used in the generated code
//

namespace SilentOrbit.ProtocolBuffers
{
    ///<summary>>
    /// This exception is thrown when badly formatted protocol buffer data is read.
    ///</summary>
    public class ProtocolBufferException : Exception
    {
        public ProtocolBufferException(string message) : base(message)
        {
        }
    }
}

#endregion ProtocolParserExceptions

#region ProtocolParserFixed

//
//  This file contain references on how to write and read
//  fixed integers and float/double.
//

namespace SilentOrbit.ProtocolBuffers
{
    public static partial class ProtocolParser
    {
        #region Fixed Int, Only for reference

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static ulong ReadFixed64(BinaryReader reader)
        {
            return reader.ReadUInt64();
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static long ReadSFixed64(BinaryReader reader)
        {
            return reader.ReadInt64();
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static uint ReadFixed32(BinaryReader reader)
        {
            return reader.ReadUInt32();
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static int ReadSFixed32(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static void WriteFixed64(BinaryWriter writer, ulong val)
        {
            writer.Write(val);
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static void WriteSFixed64(BinaryWriter writer, long val)
        {
            writer.Write(val);
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static void WriteFixed32(BinaryWriter writer, uint val)
        {
            writer.Write(val);
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static void WriteSFixed32(BinaryWriter writer, int val)
        {
            writer.Write(val);
        }

        #endregion Fixed Int, Only for reference

        #region Fixed: float, double. Only for reference

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static float ReadFloat(BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static double ReadDouble(BinaryReader reader)
        {
            return reader.ReadDouble();
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static void WriteFloat(BinaryWriter writer, float val)
        {
            writer.Write(val);
        }

        /// <summary>
        /// Only for reference
        /// </summary>
        [Obsolete("Only for reference")]
        public static void WriteDouble(BinaryWriter writer, double val)
        {
            writer.Write(val);
        }

        #endregion Fixed: float, double. Only for reference
    }
}

#endregion ProtocolParserFixed

#region ProtocolParserKey

//
//  Reader/Writer for field key
//

namespace SilentOrbit.ProtocolBuffers
{
    public enum Wire
    {
        Varint = 0,          //int32, int64, UInt32, UInt64, SInt32, SInt64, bool, enum
        Fixed64 = 1,         //fixed64, sfixed64, double
        LengthDelimited = 2, //string, bytes, embedded messages, packed repeated fields

        //Start = 3,         //  groups (deprecated)
        //End = 4,           //  groups (deprecated)
        Fixed32 = 5,         //32-bit    fixed32, SFixed32, float
    }

    public class Key
    {
        public uint Field { get; set; }

        public Wire WireType { get; set; }

        public Key(uint field, Wire wireType)
        {
            Field = field;
            WireType = wireType;
        }

        public override string ToString()
        {
            return string.Format("[Key: {0}, {1}]", Field, WireType);
        }
    }

    /// <summary>
    /// Storage of unknown fields
    /// </summary>
    public class KeyValue
    {
        public Key Key { get; set; }

        public byte[] Value { get; set; }

        public KeyValue(Key key, byte[] value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("[KeyValue: {0}, {1}, {2} bytes]", Key.Field, Key.WireType, Value.Length);
        }
    }

    public static partial class ProtocolParser
    {
        public static Key ReadKey(Stream stream)
        {
            uint n = ReadUInt32(stream);
            return new Key(n >> 3, (Wire)(n & 0x07));
        }

        public static Key ReadKey(byte firstByte, Stream stream)
        {
            if (firstByte < 128)
                return new Key((uint)(firstByte >> 3), (Wire)(firstByte & 0x07));
            uint fieldID = ((uint)ReadUInt32(stream) << 4) | ((uint)(firstByte >> 3) & 0x0F);
            return new Key(fieldID, (Wire)(firstByte & 0x07));
        }

        public static void WriteKey(Stream stream, Key key)
        {
            uint n = (key.Field << 3) | ((uint)key.WireType);
            WriteUInt32(stream, n);
        }

        /// <summary>
        /// Seek past the value for the previously read key.
        /// </summary>
        public static void SkipKey(Stream stream, Key key)
        {
            switch (key.WireType)
            {
                case Wire.Fixed32:
                    stream.Seek(4, SeekOrigin.Current);
                    return;

                case Wire.Fixed64:
                    stream.Seek(8, SeekOrigin.Current);
                    return;

                case Wire.LengthDelimited:
                    stream.Seek(ProtocolParser.ReadUInt32(stream), SeekOrigin.Current);
                    return;

                case Wire.Varint:
                    ProtocolParser.ReadSkipVarInt(stream);
                    return;

                default:
                    throw new NotImplementedException("Unknown wire type: " + key.WireType);
            }
        }

        /// <summary>
        /// Read the value for an unknown key as bytes.
        /// Used to preserve unknown keys during deserialization.
        /// Requires the message option preserveunknown=true.
        /// </summary>
        public static byte[] ReadValueBytes(Stream stream, Key key)
        {
            byte[] b;
            int offset = 0;

            switch (key.WireType)
            {
                case Wire.Fixed32:
                    b = new byte[4];
                    while (offset < 4)
                        offset += stream.Read(b, offset, 4 - offset);
                    return b;

                case Wire.Fixed64:
                    b = new byte[8];
                    while (offset < 8)
                        offset += stream.Read(b, offset, 8 - offset);
                    return b;

                case Wire.LengthDelimited:
                    //Read and include length in value buffer
                    uint length = ProtocolParser.ReadUInt32(stream);
                    using (var ms = new MemoryStream())
                    {
                        //TODO: pass b directly to MemoryStream constructor or skip usage of it completely
                        ProtocolParser.WriteUInt32(ms, length);
                        b = new byte[length + ms.Length];
                        ms.ToArray().CopyTo(b, 0);
                        offset = (int)ms.Length;
                    }

                    //Read data into buffer
                    while (offset < b.Length)
                        offset += stream.Read(b, offset, b.Length - offset);
                    return b;

                case Wire.Varint:
                    return ProtocolParser.ReadVarIntBytes(stream);

                default:
                    throw new NotImplementedException("Unknown wire type: " + key.WireType);
            }
        }
    }
}

#endregion ProtocolParserKey

#region ProtocolParserMemory

#if NET_4_0
#endif

/// <summary>
/// MemoryStream management
/// </summary>
namespace SilentOrbit.ProtocolBuffers
{
    public interface MemoryStreamStack : IDisposable
    {
        MemoryStream Pop();

        void Push(MemoryStream stream);
    }

    /// <summary>
    /// Thread safe stack of memory streams
    /// </summary>
    public class ThreadSafeStack : MemoryStreamStack
    {
        private Stack<MemoryStream> stack = new Stack<MemoryStream>();

        /// <summary>
        /// The returned stream is not reset.
        /// You must call .SetLength(0) before using it.
        /// This is done in the generated code.
        /// </summary>
        public MemoryStream Pop()
        {
            lock (stack)
            {
                if (stack.Count == 0)
                    return new MemoryStream();
                else
                    return stack.Pop();
            }
        }

        public void Push(MemoryStream stream)
        {
            lock (stack)
            {
                stack.Push(stream);
            }
        }

        public void Dispose()
        {
            lock (stack)
            {
                stack.Clear();
            }
        }
    }

    /// <summary>
    /// Non-thread safe stack of memory streams
    /// Safe as long as only one thread is Serializing
    /// </summary>
    public class ThreadUnsafeStack : MemoryStreamStack
    {
        private Stack<MemoryStream> stack = new Stack<MemoryStream>();

        /// <summary>
        /// The returned stream is not reset.
        /// You must call .SetLength(0) before using it.
        /// This is done in the generated code.
        /// </summary>
        public MemoryStream Pop()
        {
            if (stack.Count == 0)
                return new MemoryStream();
            else
                return stack.Pop();
        }

        public void Push(MemoryStream stream)
        {
            stack.Push(stream);
        }

        public void Dispose()
        {
            stack.Clear();
        }
    }

    /// <summary>
    /// Unoptimized stack, allocates a new MemoryStream for every request.
    /// </summary>
    public class AllocationStack : MemoryStreamStack
    {
        /// <summary>
        /// The returned stream is not reset.
        /// You must call .SetLength(0) before using it.
        /// This is done in the generated code.
        /// </summary>
        public MemoryStream Pop()
        {
            return new MemoryStream();
        }

        public void Push(MemoryStream stream)
        {
            //No need to Dispose MemoryStream
        }

        public void Dispose()
        {
        }
    }

#if NET_4_0
    public class ConcurrentBagStack : MemoryStreamStack
    {
        ConcurrentBag<MemoryStream> bag = new ConcurrentBag<MemoryStream>();

        /// <summary>
        /// The returned stream is not reset.
        /// You must call .SetLength(0) before using it.
        /// This is done in the generated code.
        /// </summary>
        public MemoryStream Pop()
        {
            MemoryStream result;

            if (bag.TryTake(out result))
                return result;
            else
                return new MemoryStream();
        }

        public void Push(MemoryStream stream)
        {
            bag.Add(stream);
        }

        public void Dispose()
        {
            throw new ApplicationException("ConcurrentBagStack.Dispose() should not be called.");
        }
    }
#endif

    public static partial class ProtocolParser
    {
        /// <summary>
        /// Experimental stack of MemoryStream
        /// </summary>
        public static MemoryStreamStack Stack = new AllocationStack();
    }
}

#endregion ProtocolParserMemory

#region ProtocolParserVarInt

namespace SilentOrbit.ProtocolBuffers
{
    public static partial class ProtocolParser
    {
        /// <summary>
        /// Reads past a varint for an unknown field.
        /// </summary>
        public static void ReadSkipVarInt(Stream stream)
        {
            while (true)
            {
                int b = stream.ReadByte();
                if (b < 0)
                    throw new IOException("Stream ended too early");

                if ((b & 0x80) == 0)
                    return; //end of varint
            }
        }

        public static byte[] ReadVarIntBytes(Stream stream)
        {
            byte[] buffer = new byte[10];
            int offset = 0;
            while (true)
            {
                int b = stream.ReadByte();
                if (b < 0)
                    throw new IOException("Stream ended too early");
                buffer[offset] = (byte)b;
                offset += 1;
                if ((b & 0x80) == 0)
                    break; //end of varint
                if (offset >= buffer.Length)
                    throw new ProtocolBufferException("VarInt too long, more than 10 bytes");
            }
            byte[] ret = new byte[offset];
            Array.Copy(buffer, ret, ret.Length);
            return ret;
        }

        #region VarInt: int32, uint32, sint32

        [Obsolete("Use (int)ReadUInt64(stream); //yes 64")]
        /// <summary>
        /// Since the int32 format is inefficient for negative numbers we have avoided to implement it.
        /// The same functionality can be achieved using: (int)ReadUInt64(stream);
        /// </summary>
        public static int ReadInt32(Stream stream)
        {
            return (int)ReadUInt64(stream);
        }

        [Obsolete("Use WriteUInt64(stream, (ulong)val); //yes 64, negative numbers are encoded that way")]
        /// <summary>
        /// Since the int32 format is inefficient for negative numbers we have avoided to imlplement.
        /// The same functionality can be achieved using: WriteUInt64(stream, (uint)val);
        /// Note that 64 must always be used for int32 to generate the ten byte wire format.
        /// </summary>
        public static void WriteInt32(Stream stream, int val)
        {
            //signed varint is always encoded as 64 but values!
            WriteUInt64(stream, (ulong)val);
        }

        /// <summary>
        /// Zig-zag signed VarInt format
        /// </summary>
        public static int ReadZInt32(Stream stream)
        {
            uint val = ReadUInt32(stream);
            return (int)(val >> 1) ^ ((int)(val << 31) >> 31);
        }

        /// <summary>
        /// Zig-zag signed VarInt format
        /// </summary>
        public static void WriteZInt32(Stream stream, int val)
        {
            WriteUInt32(stream, (uint)((val << 1) ^ (val >> 31)));
        }

        /// <summary>
        /// Unsigned VarInt format
        /// Do not use to read int32, use ReadUint64 for that.
        /// </summary>
        public static uint ReadUInt32(Stream stream)
        {
            int b;
            uint val = 0;

            for (int n = 0; n < 5; n++)
            {
                b = stream.ReadByte();
                if (b < 0)
                    throw new IOException("Stream ended too early");

                //Check that it fits in 32 bits
                if ((n == 4) && (b & 0xF0) != 0)
                    throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
                //End of check

                if ((b & 0x80) == 0)
                    return val | (uint)b << (7 * n);

                val |= (uint)(b & 0x7F) << (7 * n);
            }

            throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
        }

        /// <summary>
        /// Unsigned VarInt format
        /// </summary>
        public static void WriteUInt32(Stream stream, uint val)
        {
            byte b;
            while (true)
            {
                b = (byte)(val & 0x7F);
                val = val >> 7;
                if (val == 0)
                {
                    stream.WriteByte(b);
                    break;
                }
                else
                {
                    b |= 0x80;
                    stream.WriteByte(b);
                }
            }
        }

        #endregion VarInt: int32, uint32, sint32

        #region VarInt: int64, UInt64, SInt64

        [Obsolete("Use (long)ReadUInt64(stream); instead")]
        /// <summary>
        /// Since the int64 format is inefficient for negative numbers we have avoided to implement it.
        /// The same functionality can be achieved using: (long)ReadUInt64(stream);
        /// </summary>
        public static int ReadInt64(Stream stream)
        {
            return (int)ReadUInt64(stream);
        }

        [Obsolete("Use WriteUInt64 (stream, (ulong)val); instead")]
        /// <summary>
        /// Since the int64 format is inefficient for negative numbers we have avoided to implement.
        /// The same functionality can be achieved using: WriteUInt64 (stream, (ulong)val);
        /// </summary>
        public static void WriteInt64(Stream stream, int val)
        {
            WriteUInt64(stream, (ulong)val);
        }

        /// <summary>
        /// Zig-zag signed VarInt format
        /// </summary>
        public static long ReadZInt64(Stream stream)
        {
            ulong val = ReadUInt64(stream);
            return (long)(val >> 1) ^ ((long)(val << 63) >> 63);
        }

        /// <summary>
        /// Zig-zag signed VarInt format
        /// </summary>
        public static void WriteZInt64(Stream stream, long val)
        {
            WriteUInt64(stream, (ulong)((val << 1) ^ (val >> 63)));
        }

        /// <summary>
        /// Unsigned VarInt format
        /// </summary>
        public static ulong ReadUInt64(Stream stream)
        {
            int b;
            ulong val = 0;

            for (int n = 0; n < 10; n++)
            {
                b = stream.ReadByte();
                if (b < 0)
                    throw new IOException("Stream ended too early");

                //Check that it fits in 64 bits
                if ((n == 9) && (b & 0xFE) != 0)
                    throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
                //End of check

                if ((b & 0x80) == 0)
                    return val | (ulong)b << (7 * n);

                val |= (ulong)(b & 0x7F) << (7 * n);
            }

            throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
        }

        /// <summary>
        /// Unsigned VarInt format
        /// </summary>
        public static void WriteUInt64(Stream stream, ulong val)
        {
            byte b;
            while (true)
            {
                b = (byte)(val & 0x7F);
                val = val >> 7;
                if (val == 0)
                {
                    stream.WriteByte(b);
                    break;
                }
                else
                {
                    b |= 0x80;
                    stream.WriteByte(b);
                }
            }
        }

        #endregion VarInt: int64, UInt64, SInt64

        #region Varint: bool

        public static bool ReadBool(Stream stream)
        {
            int b = stream.ReadByte();
            if (b < 0)
                throw new IOException("Stream ended too early");
            if (b == 1)
                return true;
            if (b == 0)
                return false;
            throw new ProtocolBufferException("Invalid boolean value");
        }

        public static void WriteBool(Stream stream, bool val)
        {
            stream.WriteByte(val ? (byte)1 : (byte)0);
        }

        #endregion Varint: bool
    }
}

#endregion ProtocolParserVarInt