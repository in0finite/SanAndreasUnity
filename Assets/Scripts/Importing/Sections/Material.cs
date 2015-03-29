using System;
using System.IO;

namespace SanAndreasUnity.Importing.Sections
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
        public readonly Single Diffuse;

        public Material(SectionHeader header, Stream stream)
        {
            SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            Flags = reader.ReadUInt32();
            Colour = new Color4(reader);
            reader.ReadUInt32();
            TextureCount = reader.ReadUInt32();
            Textures = new Texture[TextureCount];
            Ambient = reader.ReadSingle();
            Specular = reader.ReadSingle();
            Diffuse = reader.ReadSingle();

            for (var i = 0; i < TextureCount; ++i) {
                Textures[i] = Section<Texture>.ReadData(stream);
            }
        }
    }
}
