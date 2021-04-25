using System.IO;
using Types = SanAndreasUnity.Importing.Conversion.Types;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(0x0253F2F9)]
    public class ExtraVertColor : SectionData
    {
        public readonly UnityEngine.Color[] Colors;

        public ExtraVertColor(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            uint magicNumber = reader.ReadUInt32();

            if (0 == magicNumber)
                return;

            var geometry = header.GetParent<Geometry>();

            Colors = new UnityEngine.Color[geometry.VertexCount];

            for (int i = 0; i < geometry.VertexCount; i++)
            {
                Colors[i] = Types.Convert(new Color4(reader));
            }
        }
    }
}
