using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public class MaterialSplit
    {
        public readonly UInt32 VertexCount;
        public readonly UInt32 MaterialIndex;
        public readonly Int32[] FaceIndices;

        public MaterialSplit(Stream stream)
        {
            var reader = new BinaryReader(stream);
            VertexCount = reader.ReadUInt32();
            FaceIndices = new Int32[VertexCount];
            MaterialIndex = reader.ReadUInt32();

            for (var i = 0; i < VertexCount; ++i)
            {
                FaceIndices[i] = (Int32)reader.ReadUInt32();
            }
        }
    }

    [SectionType(1294)]
    public class MaterialSplitList : SectionData
    {
        public readonly bool TriangleStrip;
        public readonly UInt32 SplitCount;
        public readonly UInt32 FaceCount;
        public readonly MaterialSplit[] MaterialSplits;

        public MaterialSplitList(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            TriangleStrip = reader.ReadUInt32() == 1;
            SplitCount = reader.ReadUInt32();
            MaterialSplits = new MaterialSplit[SplitCount];
            FaceCount = reader.ReadUInt32();

            for (var i = 0; i < SplitCount; ++i)
            {
                MaterialSplits[i] = new MaterialSplit(stream);
            }
        }
    }
}