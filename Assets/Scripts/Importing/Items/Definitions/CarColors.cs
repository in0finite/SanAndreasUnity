namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Section("col")]
    public class ColorDef : Definition
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public ColorDef(string line)
            : base(line.Replace('.', ','))
        {
            R = GetByte(0);
            G = GetByte(1);
            B = GetByte(2);
        }
    }

    public abstract class CarColorDef : Definition
    {
        public struct ColorIndices
        {
            public readonly int A;
            public readonly int B;
            public readonly int C;
            public readonly int D;

            public ColorIndices(int a, int b, int c = -1, int d = -1)
            {
                A = a;
                B = b;
                C = c;
                D = d;
            }
        }

        public readonly string Name;
        public readonly ColorIndices[] Colors;

        public readonly bool Is4Color;

        protected CarColorDef(string line, int indicesPerEntry)
            : base(line)
        {
            Name = GetString(0);
            Colors = new ColorIndices[(Parts - 1) / indicesPerEntry];
            Is4Color = indicesPerEntry >= 4;
        }
    }

    [Section("car")]
    public class CarColor2Def : CarColorDef
    {
        public CarColor2Def(string line)
            : base(line, 2)
        {
            for (var i = 0; i < Colors.Length; ++i)
            {
                Colors[i] = new ColorIndices(GetInt(i * 2 + 1), GetInt(i * 2 + 2));
            }
        }
    }

    [Section("car4")]
    public class CarColor4Def : CarColorDef
    {
        public CarColor4Def(string line)
            : base(line, 4)
        {
            for (var i = 0; i < Colors.Length; ++i)
            {
                Colors[i] = new ColorIndices(GetInt(i * 4 + 1), GetInt(i * 4 + 2), GetInt(i * 4 + 3), GetInt(i * 4 + 4));
            }
        }
    }
}