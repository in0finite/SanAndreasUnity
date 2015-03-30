using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    public class Face
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;
        public readonly Surface Surface;

        public Face(BinaryReader reader, Version version)
        {
            switch (version) {
                case Version.COLL:
                    A = reader.ReadInt32();
                    B = reader.ReadInt32();
                    C = reader.ReadInt32();
                    Surface = new Surface(reader);
                    break;
                default:
                    A = reader.ReadUInt16();
                    B = reader.ReadUInt16();
                    C = reader.ReadUInt16();
                    Surface = new Surface(reader, true);
                    break;
            }
        }
    }
}
