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

        public readonly UInt32 Unknown1;
        public readonly UInt32 Unknown2;
        public readonly Single Unknown3;
        public readonly Single Unknown4;
        public readonly Single Unknown5;

        public Material(SectionHeader header, Stream stream)
        {
            SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            Unknown1 = reader.ReadUInt32(); // Unknown
            Colour = new Color4(reader);
            Unknown2 = reader.ReadUInt32(); // Unknown
            TextureCount = reader.ReadUInt32();
            Textures = new Texture[TextureCount];
            Unknown3 = reader.ReadSingle(); // Unknown
            Unknown4 = reader.ReadSingle(); // Unknown
            Unknown5 = reader.ReadSingle(); // Unknown

            for (var i = 0; i < TextureCount; ++i) {
                Textures[i] = Section<Texture>.ReadData(stream);
            }
        }
    }
}
