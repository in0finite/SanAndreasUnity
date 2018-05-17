using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(8)]
    public class MaterialList : SectionData
    {
        public readonly UInt32 MaterialCount;
        public readonly Material[] Materials;

        public MaterialList(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var data = ReadSection<Data>();
            MaterialCount = BitConverter.ToUInt32(data.Value, 0);

            Materials = new Material[MaterialCount];

            for (var i = 0; i < MaterialCount; ++i)
            {
                Materials[i] = ReadSection<Material>();
            }
        }
    }
}