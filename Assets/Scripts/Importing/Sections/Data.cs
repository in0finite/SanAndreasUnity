using System.IO;

namespace SanAndreasUnity.Importing.Sections
{
    [SectionType(1)]
    internal class Data : SectionData
    {
        public readonly byte[] Value;

        public Data(SectionHeader header, Stream stream)
        {
            Value = new byte[header.Size];
            stream.Read(Value, 0, (int) header.Size);
        }
    }
}
