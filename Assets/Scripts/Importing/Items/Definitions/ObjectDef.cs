using System;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Flags]
    public enum ObjectFlag : uint
    {
        None = 0,
        WetEffect = 1, // IS_ROAD
        RenderAtNight = 2, // ?
        Alpha1 = 4, // DRAW_LAST
        Alpha2 = 8, // ADDITIVE
        RenderAtDay = 16, // ?
        Interior = 32, // ? doors
        NoZBufferWrite = 64,
        NoCull = 128, // DONT_RECEIVE_SHADOWS
        DisableDrawDist = 256, // ?
        Breakable = 512, // IS_GLASS_TYPE_1
        BreakableCrack = 1024, // IS_GLASS_TYPE_2
        GarageDoor = 2048,
        MultiClumpCollide = 4096, // IS_DAMAGABLE
        IsTree = 8192,
        IsPalm = 16384,
        WeatherBrightness = 32768, // DOES_NOT_COLLIDE_WITH_FLYER
        ExplodeHit = 65536,
        MultiClumpSpray = 1048576, // IS_TAG
        NoBackCull = 2097152,
        IsBreakableStatue = 4194304,
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