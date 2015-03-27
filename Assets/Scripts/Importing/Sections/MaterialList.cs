using System;
using System.IO;

namespace SanAndreasUnity.Importing.Sections
{
    [SectionType(8)]
    public class MaterialList : SectionData
    {
        public readonly UInt32 MaterialCount;
        public readonly Material[] Materials;

        public MaterialList(SectionHeader header, Stream stream)
        {
            var data = Section<Data>.ReadData(stream);
            MaterialCount = BitConverter.ToUInt32(data.Value, 0);

            Materials = new Material[MaterialCount];

            for (var i = 0; i < MaterialCount; ++i) {
                Materials[i] = Section<Material>.ReadData(stream);
            }
        }
    }
}
