using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public enum AtomicFlag
    {
        CollisionTest = 0x01,
        Render = 0x04,
    }

    [SectionType(0x14)]
    public class Atomic : SectionData
    {
        public readonly UInt32 FrameIndex;
        public readonly UInt32 GeometryIndex;
        public readonly AtomicFlag Flags;
        public readonly UInt32 Unused;

        public Atomic(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var data = ReadSection<Data>(); // Struct
            var reader = new BinaryReader(new MemoryStream(data.Value));

            FrameIndex = reader.ReadUInt32();
            GeometryIndex = reader.ReadUInt32();
            Flags = (AtomicFlag)reader.ReadUInt32();
            Unused = reader.ReadUInt32();
        }
    }
}