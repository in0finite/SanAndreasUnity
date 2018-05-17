using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    public class FaceGroup
    {
        public const int Size = 2 * Vector3.Size + 2 * sizeof(ushort);

        public readonly Vector3 Min;
        public readonly Vector3 Max;
        public readonly int StartFace;
        public readonly int EndFace;

        public FaceGroup(BinaryReader reader)
        {
            Min = new Vector3(reader);
            Max = new Vector3(reader);
            StartFace = reader.ReadUInt16();
            EndFace = reader.ReadUInt16();
        }
    }
}