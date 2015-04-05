using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(0x253F2FE)]
    public class Frame : SectionData
    {
        public readonly String Name;

        public Frame(SectionHeader header, Stream stream)
        {
            Name = new String(header, stream);
        }
    }
}
