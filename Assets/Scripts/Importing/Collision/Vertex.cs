using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    public class Vertex
    {
        public readonly Vector3 Position;

        public Vertex(BinaryReader reader, Version version)
        {
            Position = new Vector3(reader, version != Version.COLL);
        }
    }
}
