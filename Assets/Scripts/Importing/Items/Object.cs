namespace SanAndreasUnity.Importing.Items
{
    internal enum ObjectFlag : uint
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
    internal class Object : Item
    {
        public readonly int Id;

        public readonly string Geometry;
        public readonly string TextureDictionary;

        public readonly float DrawDist;
        public readonly ObjectFlag Flags;

        public Object(string line) : base(line)
        {
            Id = GetInt(0);
            Geometry = GetString(1);
            TextureDictionary = GetString(2);
            DrawDist = GetSingle(3);
            Flags = (ObjectFlag) GetInt(4);
        }
    }
}
