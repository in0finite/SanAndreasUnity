using UGameCore.Utilities;
using System.IO;
using System.Text;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(2)]
    public class String : SectionData
    {
        public readonly string Value;

        public String(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            Value = Encoding.UTF8.GetString(stream.ReadBytes((int)header.Size)).TrimNullChars();
        }
    }
}