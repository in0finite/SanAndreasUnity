using System.IO;
using System.Text;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Sections
{
    [SectionType(2)]
    public class String : SectionData
    {
        public readonly string Value;

        public String(SectionHeader header, Stream stream)
        {
            Value = Encoding.UTF8.GetString(stream.ReadBytes((int) header.Size)).TrimNullChars();
        }
    }
}
