using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    public class Vertex
    {
        public const int SizeV1 = Vector3.Size;
        public const int Size = Vector3.SizeCompressed;

        public readonly Vector3 Position;

        public Vertex(BinaryReader reader, Version version)
        {
            Position = new Vector3(reader, (version != Version.COLL) ? VectorCompression.Collision : VectorCompression.None);
        }
    }
}