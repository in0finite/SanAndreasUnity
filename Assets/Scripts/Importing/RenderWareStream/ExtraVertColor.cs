using System.IO;
using Types = SanAndreasUnity.Importing.Conversion.Types;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(0x0253F2F9)]
    public class ExtraVertColor : SectionData
    {
        public readonly UnityEngine.Vector2[] Colors;
        public readonly UnityEngine.Vector2[] Colors2;

        public ExtraVertColor(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            uint magicNumber = reader.ReadUInt32();

            if (0 == magicNumber)
                return;

            var geometry = header.GetParent<Geometry>();

            Colors = new UnityEngine.Vector2[geometry.VertexCount];
            Colors2 = new UnityEngine.Vector2[geometry.VertexCount];

            for (int i = 0; i < geometry.VertexCount; i++)
            {
                var color = Types.Convert(new Color4(reader));
                Colors[i] = new UnityEngine.Vector2(color.r, color.g);
                Colors2[i] = new UnityEngine.Vector2(color.b, color.a);
            }
        }
    }
}
