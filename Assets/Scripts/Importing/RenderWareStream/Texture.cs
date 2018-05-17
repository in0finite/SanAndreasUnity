using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public enum Filter : ushort
    {
        None = 0x0,
        Nearest = 0x1,
        Linear = 0x2,
        MipNearest = 0x3,
        MipLinear = 0x4,
        LinearMipNearest = 0x5,
        LinearMipLinear = 0x6,
        Unknown = 0x1101
    }

    public enum WrapMode : byte
    {
        None = 0,
        Wrap = 1,
        Mirror = 2,
        Clamp = 3
    }

    public enum CompressionMode : byte
    {
        None = 0,
        DXT1 = 1,
        DXT3 = 3
    }

    [Flags]
    public enum RasterFormat : uint
    {
        Default = 0x0000,
        A1R5G5B5 = 0x0100,
        R5G6B5 = 0x0200,
        R4G4B4A4 = 0x0300,
        LUM8 = 0x0400,
        BGRA8 = 0x0500,
        BGR8 = 0x0600,
        R5G5B5 = 0x0a00,

        NoExt = 0x0fff,

        ExtAutoMipMap = 0x1000,
        ExtPal8 = 0x2000,
        ExtPal4 = 0x4000,
        ExtMipMap = 0x8000
    }

    [SectionType(6)]
    public class Texture : SectionData
    {
        public readonly Filter FilterMode;
        public readonly string TextureName;
        public readonly string MaskName;

        public Texture(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            FilterMode = (Filter)reader.ReadUInt16();
            reader.ReadUInt16(); // Unknown

            TextureName = ReadSection<String>().Value;
            MaskName = ReadSection<String>().Value;
        }
    }
}