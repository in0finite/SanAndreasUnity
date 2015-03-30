using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    public struct Surface
    {
        public readonly byte Material;
        public readonly Flags Flags;
        public readonly byte Brightness;
        public readonly byte Light;

        public Surface(BinaryReader reader, bool simple = false)
        {
            if (!simple) {
                Material = reader.ReadByte();
                Flags = (Flags) reader.ReadByte();
                Brightness = reader.ReadByte();
                Light = reader.ReadByte();
            } else {
                Material = reader.ReadByte();
                Flags = Flags.None;
                Brightness = 0;
                Light = reader.ReadByte();
            }
        }
    }
}
