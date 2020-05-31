using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(TypeId)]
    public class Clump : SectionData
    {
        public const Int32 TypeId = 16;

        public readonly UInt32 AtomicCount;
        public readonly UInt32 LightCount;
        public readonly UInt32 CameraCount;

        public readonly FrameList FrameList;
        public readonly GeometryList GeometryList;
        public readonly Atomic[] Atomics;

        public readonly Collision.CollisionFile Collision;

        public Clump(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var data = ReadSection<Data>(); // Struct
            if (data == null) return;

            var reader = new BinaryReader(new MemoryStream(data.Value));

            AtomicCount = reader.ReadUInt32();
            LightCount = reader.ReadUInt32();
            CameraCount = reader.ReadUInt32();

            FrameList = ReadSection<FrameList>(); // Frame List
            GeometryList = ReadSection<GeometryList>(); // Geometry List

            Atomics = new Atomic[AtomicCount];

            for (int i = 0; i < AtomicCount; ++i)
            {
                Atomics[i] = ReadSection<Atomic>(); // Atomic
            }

            var section = ReadSection<SectionData>();
            var extension = section as Extension;

            if (extension != null)
            {
                var collision = extension.FirstOrDefault<CollisionModel>();
                if (collision != null)
                {
                    Collision = collision.Collision;
                }
            }
        }
    }
}