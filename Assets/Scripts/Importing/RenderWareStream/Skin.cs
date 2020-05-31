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
        public readonly SkinBoneIndices[] VertexBoneIndices;
        public readonly SkinBoneWeights[] VertexBoneWeights;

        public readonly Matrix4x4[] SkinToBoneMatrices;

        public readonly byte[] MeshBoneRemapIndices;

        public Skin(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            Int32 boneCount = (Int32)reader.ReadByte();
            Int32 boneIdCount = (Int32)reader.ReadByte();
            UInt16 weightsPerVertex = reader.ReadUInt16();

            byte[] boneIds = reader.ReadBytes(boneIdCount);

            var vertexCount = header.GetParent<Geometry>().VertexCount;

            VertexBoneIndices = new SkinBoneIndices[vertexCount];
            VertexBoneWeights = new SkinBoneWeights[vertexCount];

            for (int i = 0; i < vertexCount; ++i)
            {
                VertexBoneIndices[i] = new SkinBoneIndices(reader);
            }

            for (int i = 0; i < vertexCount; ++i)
            {
                VertexBoneWeights[i] = new SkinBoneWeights(reader);
            }

            SkinToBoneMatrices = new Matrix4x4[boneCount];

            for (int i = 0; i < boneCount; ++i)
            {
                if (boneIdCount == 0)
                {
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                }

                SkinToBoneMatrices[i] = new Matrix4x4(reader);
            }

            UInt32 boneLimit = reader.ReadUInt32();
            UInt32 meshCount = reader.ReadUInt32();
            UInt32 RLE = reader.ReadUInt32();

            if (meshCount > 0)
            {
                MeshBoneRemapIndices = reader.ReadBytes((Int32)(boneCount + 2 * (RLE + meshCount)));
            }
        }
    }
}