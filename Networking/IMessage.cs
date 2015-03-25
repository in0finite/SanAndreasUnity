using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Debug = UnityEngine.Debug;

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
    }

    /// <summary>
    /// Root message types that may be sent over the network must implement this
    /// interface.
    /// </summary>
    public interface INetworkMessage : IMessage { }

    /// <summary>
    /// Root message types that may be saved or loaded must implement this interface.
    /// </summary>
    public interface IPersistenceMessage : IMessage { }

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

        class Entry
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
            if (assemblies.Length == 0) {
                assemblies = new [] { Assembly.GetCallingAssembly() };
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

            if (!BitConverter.IsLittleEndian) {
                throw new NotImplementedException("Not implemented for Big-Endian systems.");
            }
        }

        private void UpdateIdentDict()
        {
            _idents.Clear();

            foreach (var entry in _entries) {
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
            foreach (var entry in valid) {
                bool nextIdentConflict;

                if (entry.Ident > 0) {
                    if (_entries.ContainsKey(entry.Ident)) {
                        throw new Exception(String.Format("Message identifier conflict: "
                            + "'{0}' and '{1}' share the same hard-coded identifier.",
                            entry.Type, _entries[entry.Ident].Type));
                    }

                    nextIdentConflict = entry.Ident == nextIdent;
                } else {
                    entry.Ident = nextIdent;
                    nextIdentConflict = true;
                }

                if (entry.Ident > MaxIdent) {
                    throw new Exception(String.Format("Message identifier is too large: '{0}'.", entry.Ident));
                }

                _entries.Add(entry.Ident, entry);

                while (nextIdentConflict) {
                    nextIdentConflict = _entries.ContainsKey(++nextIdent);
                }
            }

            foreach (var entry in _entries) {
                Debug.LogFormat("{0}: {1}", entry.Key, entry.Value.Type.Name);
            }

            UpdateIdentDict();
        }

        // HACK
        private String TranslateTypeName(String name)
        {
            return name.StartsWith("Arcade.") ? name.Substring(7) : name;
        }

        /// <summary>
        /// Load identifier assignments from a schema object.
        /// </summary>
        internal void FromSchema(MessageTableSchema schema)
        {
            var asm = Assembly.GetExecutingAssembly();

            var entries = schema.Entries
                .Select(x => new { type = asm.GetType(TranslateTypeName(x.TypeName)), ident = x.Ident })
                .Select(x => new Entry(x.type, x.ident))
                .ToArray();

            _entries.Clear();

            foreach (var entry in entries) {
                _entries.Add(entry.Ident, entry);
            }

            UpdateIdentDict();
        }

        private static void WriteIdent(Stream stream, uint ident)
        {
            for (var i = 0; i < 4; ++i) {
                var val = (byte) ((ident >> (7 * i)) & 0x7f);
                var flag = ident >> (7 * i + 7) > 0;

                stream.WriteByte((byte) (val | (flag ? 0x80 : 0x00)));

                if (!flag) break;
            }
        }

        private static uint ReadIdent(Stream stream)
        {
            uint ident = 0;

            for (var i = 0; i < 4; ++i) {
                var encoded = stream.ReadByte();
                var val = encoded & 0x7f;
                var flag = (encoded & 0x80) == 0x80;

                ident |= (uint) (val << (7 * i));

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
            using (var stream = new MemoryStream()) {
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

        public TBaseInterface Deserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes)) {
                return Deserialize(stream);
            }
        }

        public TMessage Deserialize<TMessage>(byte[] bytes)
            where TMessage : TBaseInterface
        {
            using (var stream = new MemoryStream(bytes)) {
                return (TMessage) Deserialize(stream);
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
            return (TMessage) Deserialize(stream);
        }

        /// <summary>
        /// Construct a schema object that can be used to load identifier assignments.
        /// </summary>
        public MessageTableSchema GetSchema()
        {
            if (_schema != null) return _schema;

            _schema = new MessageTableSchema {
                Entries = _entries.Values
                    .OrderBy(x => x.Ident)
                    .Select(x => new MessageTableEntry { Ident = x.Ident, TypeName = x.Type.FullName })
                    .ToList()
            };

            return _schema;
        }
    }
}
