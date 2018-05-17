using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(0x253F2FE)]
    public class FrameName : SectionData
    {
        public readonly String Name;

        public FrameName(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            Name = new String(header, stream);
        }
    }
}