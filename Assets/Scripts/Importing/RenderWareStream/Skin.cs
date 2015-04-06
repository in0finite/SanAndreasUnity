using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(0x0116)]
    public class Skin : SectionData
    {
        public Skin(SectionHeader header, Stream stream)
        {
            var reader = new BinaryReader(stream);

            Int32 boneCount = (Int32)reader.ReadByte();
            Int32 boneIdCount = (Int32)reader.ReadByte();
            UInt16 weightsPerVertex = reader.ReadUInt16();

            byte[] boneIds = reader.ReadBytes(boneIdCount);
        }
    }
}
