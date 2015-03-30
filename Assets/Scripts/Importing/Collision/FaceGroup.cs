using System.IO;

namespace SanAndreasUnity.Importing.Collision
{
    public class FaceGroup
    {
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
