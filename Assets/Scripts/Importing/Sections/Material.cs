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
    internal class Material : SectionData
    {
        public readonly Color4 Colour;
        public readonly UInt32 TextureCount;
        public readonly Texture[] Textures;

        public Material(SectionHeader header, Stream stream)
        {
            var dataHeader = SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            reader.ReadUInt32(); // Unknown
            Colour = new Color4(reader);
            reader.ReadUInt32(); // Unknown
            TextureCount = reader.ReadUInt32();
            Textures = new Texture[TextureCount]; 
            reader.ReadSingle(); // Unknown
            reader.ReadSingle(); // Unknown
            reader.ReadSingle(); // Unknown

            for (var i = 0; i < TextureCount; ++i) {
                Textures[i] = Section<Texture>.ReadData(stream);
            }
        }
    }
}
