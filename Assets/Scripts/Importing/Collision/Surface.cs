using System;
using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    [Flags]
    public enum SurfaceFlags : byte
    {
        None = 0
    }

    public struct Surface
    {
        public const int Size = 4 * sizeof(byte); //lol

        public readonly byte Material;
        public readonly SurfaceFlags Flags;
        public readonly byte Brightness;
        public readonly byte Light;

        public Surface(BinaryReader reader, bool simple = false)
        {
            if (!simple)
            {
                Material = reader.ReadByte();
                Flags = (SurfaceFlags)reader.ReadByte();
                Brightness = reader.ReadByte();
                Light = reader.ReadByte();
            }
            else
            {
                Material = reader.ReadByte();
                Flags = SurfaceFlags.None;
                Brightness = 0;
                Light = reader.ReadByte();
            }
        }
    }
}