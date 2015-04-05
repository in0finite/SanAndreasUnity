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
        {
            var data = Section<Data>.ReadData(stream); // Struct
            if (data == null) return;

            var reader = new BinaryReader(new MemoryStream(data.Value));

            AtomicCount = reader.ReadUInt32();
            LightCount = reader.ReadUInt32();
            CameraCount = reader.ReadUInt32();

            FrameList = Section<FrameList>.ReadData(stream); // Frame List
            GeometryList = Section<GeometryList>.ReadData(stream); // Geometry List

            Atomics = new Atomic[AtomicCount];

            for (int i = 0; i < AtomicCount; ++i)
            {
                Atomics[i] = Section<Atomic>.ReadData(stream); // Atomic
            }

            var section = Section<SectionData>.ReadData(stream);
            var extension = section as Extension;

            if (extension != null) {
                var collision = extension.FirstOrDefault<CollisionModel>();
                if (collision != null) {
                    Collision = collision.Collision;
                }
            }
        }
    }
}
