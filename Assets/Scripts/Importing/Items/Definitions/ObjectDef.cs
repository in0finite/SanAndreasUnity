using System;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Flags]
    public enum ObjectFlag : uint
    {
        None = 0,
        IsRoad = 1,
        RenderAtNight = 2, // ?
        DrawLast = 4,
        Additive = 8,
        RenderAtDay = 16, // ?
        Interior = 32, // ? doors
        NoZBufferWrite = 64,
        DontReceiveShadows = 128,
        DisableDrawDist = 256, // ?
        Breakable = 512, // IS_GLASS_TYPE_1
        BreakableCrack = 1024, // IS_GLASS_TYPE_2
        GarageDoor = 2048,
        IsDamagable = 4096,
        IsTree = 8192,
        IsPalm = 16384,
        DoesNotCollideWithFlyer = 32768,
        ExplodeHit = 65536,
        IsTag = 1048576,
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