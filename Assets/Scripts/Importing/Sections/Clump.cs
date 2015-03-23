using System;
using System.IO;

namespace SanAndreasUnity.Importing.Sections
{
    [SectionType(16)]
    internal class Clump : SectionData
    {
        public readonly UInt32 ObjectCount;
        public readonly GeometryList GeometryList;

        public Clump(SectionHeader header, Stream stream)
        {
            var dat = Section<Data>.ReadData(stream);
            if (dat == null) return;

            ObjectCount = BitConverter.ToUInt32(dat.Value, 0);
            var frameList = Section<SectionData>.Read(stream);
            GeometryList = Section<GeometryList>.ReadData(stream);
        }
    }
}
