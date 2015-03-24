using System.IO;
using UnityEngine;

namespace SanAndreasUnity.Importing.Items
{
    [Section("inst")]
    internal class Instance : Item
    {
        public readonly int ObjectId;
        public readonly string LodGeometry;
        public readonly int CellId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly int LodIndex;

        public Instance(string line) : base(line)
        {
            ObjectId = GetInt(0);
            LodGeometry = GetString(1);
            CellId = GetInt(2);
            Position = new Vector3(GetSingle(3), GetSingle(5), GetSingle(4));
            Rotation = new Quaternion(GetSingle(6), GetSingle(8), GetSingle(7), GetSingle(9));
            LodIndex = GetInt(10);
        }

        public Instance(BinaryReader reader) : base(reader)
        {
            var posX = reader.ReadSingle();
            var posZ = reader.ReadSingle();
            var posY = reader.ReadSingle();

            Position = new Vector3(posX, posY, posZ);

            var rotX = -reader.ReadSingle();
            var rotZ = reader.ReadSingle();
            var rotY = reader.ReadSingle();
            var rotW = reader.ReadSingle();

            Rotation = new Quaternion(rotX, rotY, rotZ, rotW);

            ObjectId = reader.ReadInt32();
            CellId = reader.ReadInt32();
            LodIndex = reader.ReadInt32();
        }
    }
}
