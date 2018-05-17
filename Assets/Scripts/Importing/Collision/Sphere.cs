using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    public class Sphere
    {
        public const int Size = sizeof(float) + Vector3.Size + Surface.Size;

        public readonly float Radius;
        public readonly Vector3 Center;
        public readonly Surface Surface;

        public Sphere(BinaryReader reader, Version version)
        {
            switch (version)
            {
                case Version.COLL:
                    Radius = reader.ReadSingle();
                    Center = new Vector3(reader);
                    break;

                default:
                    Center = new Vector3(reader);
                    Radius = reader.ReadSingle();
                    break;
            }

            Surface = new Surface(reader);
        }
    }
}