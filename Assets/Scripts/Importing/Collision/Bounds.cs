using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    public class Bounds
    {
        public const int Size = sizeof(float) + 3 * Vector3.Size;

        public readonly float Radius;
        public readonly Vector3 Center;
        public readonly Vector3 Min;
        public readonly Vector3 Max;

        public Bounds(BinaryReader reader, Version version)
        {
            switch (version)
            {
                case Version.COLL:
                    Radius = reader.ReadSingle();
                    Center = new Vector3(reader);
                    Min = new Vector3(reader);
                    Max = new Vector3(reader);
                    break;

                default:
                    Min = new Vector3(reader);
                    Max = new Vector3(reader);
                    Center = new Vector3(reader);
                    Radius = reader.ReadSingle();
                    break;
            }
        }
    }
}