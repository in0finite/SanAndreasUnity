using System;
using System.IO;

namespace SanAndreasUnity.Importing.Sections
{
    internal enum GeometryFlag : ushort
    {
        TexCoords = 4,
        Colors = 8,
        Normals = 16
    }

    internal struct Vector2
    {
        public readonly Single X;
        public readonly Single Y;

        public Vector2(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
        }
    }

    internal struct Vector3
    {
        public readonly Single X;
        public readonly Single Y;
        public readonly Single Z;

        public Vector3(BinaryReader reader)
        {
            X = -reader.ReadSingle();
            Z = reader.ReadSingle();
            Y = reader.ReadSingle();
        }
    }

    internal struct FaceInfo
    {
        public readonly GeometryFlag Flags;
        public readonly UInt16 Vertex0;
        public readonly UInt16 Vertex1;
        public readonly UInt16 Vertex2;

        public FaceInfo(BinaryReader reader)
        {
            Vertex1 = reader.ReadUInt16();
            Vertex0 = reader.ReadUInt16();
            Flags = (GeometryFlag) reader.ReadUInt16();
            Vertex2 = reader.ReadUInt16();
        }
    }

    internal struct BoundingSphere
    {
        public readonly Vector3 Offset;
        public readonly float Radius;

        public BoundingSphere(BinaryReader reader)
        {
            Offset = new Vector3(reader);
            Radius = reader.ReadSingle();
        }
    }

    [SectionType(15)]
    internal class Geometry : SectionData
    {
        public readonly GeometryFlag Flags;
        public readonly UInt32 FaceCount;
        public readonly UInt32 VertexCount;
        public readonly UInt32 FrameCount;

        public readonly float Ambient;
        public readonly float Diffuse;
        public readonly float Specular;

        public readonly Color4[] Colours;
        public readonly Vector2[] TexCoords;
        public readonly FaceInfo[] Faces;

        public readonly BoundingSphere BoundingSphere;

        public readonly UInt32 HasPosition;
        public readonly UInt32 HasNormals;

        public readonly Vector3[] Vertices;
        public readonly Vector3[] Normals;

        public readonly Material[] Materials;
        public readonly MaterialSplit[] MaterialSplits;
        public readonly UInt16 IndexCount;

        public Geometry(SectionHeader header, Stream stream)
        {
            var dataHeader = SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);
            
            Flags = (GeometryFlag) reader.ReadUInt16();
            reader.ReadUInt16(); // Unknown
            FaceCount = reader.ReadUInt32();
            VertexCount = reader.ReadUInt32();
            FrameCount = reader.ReadUInt32();

            if (dataHeader.Version == 4099) {
                Ambient = reader.ReadSingle();
                Diffuse = reader.ReadSingle();
                Specular = reader.ReadSingle();
            }

            if ((Flags & GeometryFlag.Colors) != 0) {
                Colours = new Color4[VertexCount];
                for (var i = 0; i < VertexCount; ++i) {
                    Colours[i] = new Color4(reader);
                }
            }

            if ((Flags & GeometryFlag.TexCoords) != 0) {
                TexCoords = new Vector2[VertexCount];
                for (var i = 0; i < VertexCount; ++i) {
                    TexCoords[ i ] = new Vector2(reader);
                }
            }

            Faces = new FaceInfo[FaceCount];
            for (var i = 0; i < FaceCount; ++i) {
                Faces[ i ] = new FaceInfo(reader);
            }

            BoundingSphere = new BoundingSphere(reader);

            HasPosition = reader.ReadUInt32();
            HasNormals = reader.ReadUInt32();

            if (HasPosition > 1 || HasNormals > 1) {
                throw new Exception("Well there you go");
            }

            Vertices = new Vector3[VertexCount];
            for (var i = 0; i < VertexCount; ++i) {
                Vertices[i] = new Vector3(reader);
            }

            if ((Flags & GeometryFlag.Normals) != 0) {
                Normals = new Vector3[VertexCount];
                for (var i = 0; i < VertexCount; ++i) {
                    Normals[ i ] = new Vector3(reader);
                }
            }

            Materials = Section<MaterialList>.ReadData(stream).Materials;

            var extHeader = SectionHeader.Read(stream);
            var msplits = Section<MaterialSplitList>.ReadData(stream);

            MaterialSplits = msplits.MaterialSplits;
            FaceCount = msplits.FaceCount;
            IndexCount = msplits.IndexCount;

            foreach (var mat in MaterialSplits) {
                mat.Material = Materials[mat.MaterialIndex];
            }
        }
    }
}
