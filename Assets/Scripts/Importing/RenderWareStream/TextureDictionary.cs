using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(22)]
    public class TextureDictionary : SectionData
    {
        public UInt16 TextureCount;
        public TextureNative[] Textures;

        public TextureDictionary(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            TextureCount = reader.ReadUInt16();
            Textures = new TextureNative[TextureCount];
            reader.ReadUInt16(); // Unknown

            for (var i = 0; i < TextureCount; ++i)
            {
                Textures[i] = ReadSection<TextureNative>();
            }
        }
    }
}