using System;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Flags]
    public enum ObjectFlag : uint
    {
        None = 0,
        WetEffect = 1,
        RenderAtNight = 2,
        Alpha1 = 4,
        Alpha2 = 8,
        RenderAtDay = 16,
        Interior = 32,
        NoZBufferWrite = 64,
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

    public interface ISimpleObjectDefinition : IObjectDefinition
    {
        string ModelName { get; }
        string TextureDictionaryName { get; }
        float DrawDist { get; }
        ObjectFlag Flags { get; }
    }

    [Section("objs")]
    public class ObjectDef : Definition, ISimpleObjectDefinition
    {
        public int Id { get; }
        public string ModelName { get; }
        public string TextureDictionaryName { get; }
        public float DrawDist { get; }
        public ObjectFlag Flags { get; }

        public ObjectDef(string line) : base(line)
        {
            Id = GetInt(0);
            ModelName = GetString(1);
            TextureDictionaryName = GetString(2);
            DrawDist = GetSingle(3);
            Flags = (ObjectFlag)GetInt(4);
        }

        public bool HasFlag(ObjectFlag flag)
        {
            return (Flags & flag) == flag;
        }
    }
}