using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public enum GeometryFlag : ushort
    {
        TriangleStrips = 1,
        VertexTranslation = 2,
        TexCoords = 4,
        Colors = 8,
        Normals = 16,
        DynamicVertexLighting = 32,
        ModulateMaterialColor = 64,
        TexCoords2 = 128,
    }

    public struct FaceInfo
    {
        public readonly GeometryFlag Flags;
        public readonly UInt16 Vertex0;
        public readonly UInt16 Vertex1;
        public readonly UInt16 Vertex2;

        public int[] Indices { get { return new int[] { Vertex0, Vertex1, Vertex2 }; } }

        public FaceInfo(BinaryReader reader)
        {
            Vertex1 = reader.ReadUInt16();
            Vertex0 = reader.ReadUInt16();
            Flags = (GeometryFlag)reader.ReadUInt16();
            Vertex2 = reader.ReadUInt16();
        }
    }

    public struct BoundingSphere
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
    public class Geometry : SectionData
    {
        public readonly GeometryFlag Flags;
        public readonly UInt32 FaceCount;
        public readonly UInt32 VertexCount;
        public readonly UInt32 FrameCount;

        public readonly float Ambient;
        public readonly float Diffuse;
        public readonly float Specular;

        public readonly UnityEngine.Color32[] Colours;
        public readonly UnityEngine.Vector2[][] TexCoords;
        public readonly FaceInfo[] Faces;

        public readonly BoundingSphere BoundingSphere;

        public readonly UInt32 HasPosition;
        public readonly UInt32 HasNormals;

        public readonly UnityEngine.Vector3[] Vertices;
        public readonly UnityEngine.Vector3[] Normals;

        public readonly Material[] Materials;
        public readonly MaterialSplit[] MaterialSplits;

        public readonly Skin Skinning;

        public readonly TwoDEffect TwoDEffect;

        public readonly ExtraVertColor ExtraVertColor;

        public Geometry(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var dataHeader = SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            Flags = (GeometryFlag)reader.ReadUInt16();
            var uvCount = reader.ReadByte(); // uv count
            reader.ReadByte(); // native flags
            FaceCount = reader.ReadUInt32();
            VertexCount = reader.ReadUInt32();
            FrameCount = reader.ReadUInt32();

            if (dataHeader.Version == 4099)
            {
                Ambient = reader.ReadSingle();
                Diffuse = reader.ReadSingle();
                Specular = reader.ReadSingle();
            }

            if ((Flags & GeometryFlag.Colors) != 0)
            {
                Colours = new UnityEngine.Color32[VertexCount];
                for (var i = 0; i < VertexCount; ++i)
                {
                    Colours[i] = Conversion.Types.Convert(new Color4(reader));
                }
            }

            if ((Flags & (GeometryFlag.TexCoords | GeometryFlag.TexCoords2)) != 0)
            {
                TexCoords = new UnityEngine.Vector2[uvCount][];
                for (var j = 0; j < uvCount; ++j)
                {
                    var uvs = TexCoords[j] = new UnityEngine.Vector2[VertexCount];
                    for (var i = 0; i < VertexCount; ++i)
                    {
                        uvs[i] = Conversion.Types.Convert(new Vector2(reader));
                    }
                }
            }

            Faces = new FaceInfo[FaceCount];
            for (var i = 0; i < FaceCount; ++i)
            {
                Faces[i] = new FaceInfo(reader);
            }

            BoundingSphere = new BoundingSphere(reader);

            HasPosition = reader.ReadUInt32();
            HasNormals = reader.ReadUInt32();

            if (HasPosition > 1 || HasNormals > 1)
            {
                throw new Exception("Well there you go");
            }

            if ((Flags & GeometryFlag.VertexTranslation) != 0)
            {
                Vertices = new UnityEngine.Vector3[VertexCount];
                for (var i = 0; i < VertexCount; ++i)
                {
                    Vertices[i] = Conversion.Types.Convert(new Vector3(reader));
                }
            }

            if ((Flags & GeometryFlag.Normals) != 0)
            {
                Normals = new UnityEngine.Vector3[VertexCount];
                for (var i = 0; i < VertexCount; ++i)
                {
                    Normals[i] = Conversion.Types.Convert(new Vector3(reader));
                }
            }

            Materials = ReadSection<MaterialList>().Materials;

            var extensions = ReadSection<Extension>();

            MaterialSplits = extensions.FirstOrDefault<MaterialSplitList>().MaterialSplits;
            Skinning = extensions.FirstOrDefault<Skin>();
            TwoDEffect = extensions.FirstOrDefault<TwoDEffect>();
            ExtraVertColor = extensions.FirstOrDefault<ExtraVertColor>();
        }
    }
}