using System;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Flags]
    public enum ObjectFlag : uint
    {
        WetEffect = 1,
        RenderAtNight = 2,
        Alpha1 = 4,
        Alpha2 = 8,
        RenderAtDay = 16,
        Interior = 32,
        DisableShadowMesh = 64,
        NoCull = 128,
        DisableDrawDist = 256,
        Breakable = 512,
        BreakableCrack = 1024,
        GarageDoor = 2048,
        MultiClumpCollide = 4096,
        WeatherBrightness = 32768,
        ExplodeHit = 65536,
        MultiClumpSpray = 1048576,
        NoBackCull = 2097152
    }

    [Section("objs")]
    public class Object : Item
    {
        public readonly int Id;

        public readonly string ModelName;
        public readonly string TextureDictionaryName;

        public readonly float DrawDist;
        public readonly ObjectFlag Flags;

        public Object(string line) : base(line)
        {
            Id = GetInt(0);
            ModelName = GetString(1);
            TextureDictionaryName = GetString(2);
            DrawDist = GetSingle(3);
            Flags = (ObjectFlag) GetInt(4);
        }

        public bool HasFlag(ObjectFlag flag)
        {
            return (Flags & flag) == flag;
        }
    }
}
