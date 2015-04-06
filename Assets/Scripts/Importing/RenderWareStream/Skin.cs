using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public struct SkinBoneWeights
    {
        public readonly Single[] Weights;

        public SkinBoneWeights(BinaryReader reader)
        {
            Weights = new Single[4];

            for (int i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = reader.ReadSingle();
            }
        }
    }

    public struct SkinBoneIndices
    {
        public readonly byte[] Indices;

        public SkinBoneIndices(BinaryReader reader)
        {
            Indices = reader.ReadBytes(4);
        }
    }

    [SectionType(0x0116)]
    public class Skin : SectionData
    {
        public Skin(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            Int32 boneCount = (Int32)reader.ReadByte();
            Int32 boneIdCount = (Int32)reader.ReadByte();
            UInt16 weightsPerVertex = reader.ReadUInt16();

            byte[] boneIds = reader.ReadBytes(boneIdCount);

            var vertexCount = header.GetParent<Geometry>().VertexCount;

            SkinBoneIndices[] vertexBoneIndices = new SkinBoneIndices[vertexCount];
            SkinBoneWeights[] vertexBoneWeights = new SkinBoneWeights[vertexCount];

            for (int i = 0; i < vertexCount; ++i)
            {
                vertexBoneIndices[i] = new SkinBoneIndices(reader);
            }

            for (int i = 0; i < vertexCount; ++i)
            {
                vertexBoneWeights[i] = new SkinBoneWeights(reader);
            }

            Matrix4x4[] skinToBoneMatrices = new Matrix4x4[boneCount];

            for (int i = 0; i < boneCount; ++i)
            {
                skinToBoneMatrices[i] = new Matrix4x4(reader);
            }

            UInt32 boneLimit = reader.ReadUInt32();
            UInt32 meshCount = reader.ReadUInt32();
            UInt32 RLE = reader.ReadUInt32();

            if (meshCount > 0)
            {

            }
        }
    }
}
