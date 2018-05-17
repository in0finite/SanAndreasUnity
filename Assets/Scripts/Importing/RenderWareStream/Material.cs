using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public struct Color4
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public Color4(BinaryReader reader)
        {
            R = reader.ReadByte();
            G = reader.ReadByte();
            B = reader.ReadByte();
            A = reader.ReadByte();
        }
    }

    [SectionType(7)]
    public class Material : SectionData
    {
        public readonly Color4 Colour;
        public readonly UInt32 TextureCount;
        public readonly Texture[] Textures;

        public readonly UInt32 Flags;
        public readonly Single Ambient;
        public readonly Single Specular;
        public readonly Single Smoothness;

        public Material(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            Flags = reader.ReadUInt32();
            Colour = new Color4(reader);
            reader.ReadUInt32();
            TextureCount = reader.ReadUInt32();
            Textures = new Texture[TextureCount];
            Ambient = reader.ReadSingle();
            Smoothness = reader.ReadSingle();
            Specular = 1f - reader.ReadSingle();

            for (var i = 0; i < TextureCount; ++i)
            {
                Textures[i] = ReadSection<Texture>();
            }

            var extensions = ReadSection<Extension>();

            var smoothness = Smoothness;
            var specular = Specular;

            extensions.ForEach<ReflectionMaterial>(x => specular = x.Intensity);
            extensions.ForEach<SpecularMaterial>(x => smoothness = x.SpecularLevel);

            Smoothness = smoothness;
            Specular = specular;
        }
    }

    [SectionType(0x0253F2FC)]
    public class ReflectionMaterial : SectionData
    {
        public readonly Vector2 Scale;
        public readonly Vector2 Translation;

        public readonly float Intensity;

        public ReflectionMaterial(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            Scale = new Vector2(reader);
            Translation = new Vector2(reader);

            Intensity = reader.ReadSingle();
        }
    }

    [SectionType(0x0253F2F6)]
    public class SpecularMaterial : SectionData
    {
        public readonly float SpecularLevel;

        public SpecularMaterial(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            SpecularLevel = reader.ReadSingle();
        }
    }
}