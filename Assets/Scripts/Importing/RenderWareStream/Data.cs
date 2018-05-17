using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(1)]
    public class Data : SectionData
    {
        public readonly byte[] Value;

        public Data(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            Value = new byte[header.Size];
            stream.Read(Value, 0, (int)header.Size);
        }
    }
}