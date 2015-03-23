using System;
using System.IO;

namespace SanAndreasUnity.Importing.Sections
{
    internal class MaterialSplit
    {
        public readonly UInt16 Offset;
        public readonly UInt16 VertexCount;
        public readonly UInt16 MaterialIndex;
        public readonly UInt16[] FaceIndices;

        public Material Material { get; internal set; }

        public MaterialSplit(UInt16 offset, Stream stream)
        {
            Offset = offset;
            var reader = new BinaryReader(stream);
            VertexCount = (UInt16) reader.ReadUInt32();
            FaceIndices = new UInt16[VertexCount + 1];
            MaterialIndex = (UInt16) reader.ReadUInt32();

            for (var i = 0; i < VertexCount; ++i) {
                FaceIndices[i] = (UInt16) reader.ReadUInt32();
            }

            FaceIndices[VertexCount++] = 0xffff;
        }
    }

    [SectionType(1294)]
    internal class MaterialSplitList : SectionData
    {
        public readonly bool TriangleStrip;
        public readonly UInt32 SplitCount;
        public readonly UInt32 FaceCount;
        public readonly UInt16 IndexCount;
        public readonly MaterialSplit[] MaterialSplits;

        public MaterialSplitList(SectionHeader header, Stream stream)
        {
            var reader = new BinaryReader(stream);

            TriangleStrip = reader.ReadUInt32() == 1;
            SplitCount = reader.ReadUInt32();
            MaterialSplits = new MaterialSplit[SplitCount];
            FaceCount = reader.ReadUInt32();

            IndexCount = 0;
            for (var i = 0; i < SplitCount; ++i) {
                MaterialSplits[i] = new MaterialSplit(IndexCount, stream);
                IndexCount += MaterialSplits[i].VertexCount;
            }

            if (FaceCount + SplitCount != IndexCount) {
                throw new Exception("Bad model format");
            }
        }
    }
}
