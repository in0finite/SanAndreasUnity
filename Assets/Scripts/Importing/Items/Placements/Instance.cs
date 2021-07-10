using System.Collections.Generic;
using System.IO;

namespace SanAndreasUnity.Importing.Items.Placements
{
    public static class InstanceExtensions
    {
        public static void ResolveLod(this IList<Instance> insts)
        {
            foreach (var inst in insts)
            {
                if (inst.LodIndex != -1)
                {
                    var lod = inst.LodInstance = insts[inst.LodIndex];
                    lod.IsLod = true;
                }
            }
        }
    }

    [Section("inst")]
    public class Instance : Placement
    {
        public readonly int ObjectId;
        public readonly string LodGeometry;
        public readonly int CellId;
        public int InteriorLevel => this.CellId & 0xff;
        public readonly UnityEngine.Vector3 Position;
        public readonly UnityEngine.Quaternion Rotation;
        public readonly int LodIndex;

        public Instance LodInstance { get; internal set; }

        public bool IsLod { get; internal set; }

        //private string DebugX, DebugY, DebugZ;

        public Instance(string line) : base(line)
        {
            /*string strX = string.Format("X => Value: {0} - Parsed: {1}", GetString(3), GetSingle(3)),
                   strY = string.Format("Y => Value: {0} - Parsed: {1}", GetString(5), GetSingle(5)),
                   strZ = string.Format("Z => Value: {0} - Parsed: {1}", GetString(4), GetSingle(4));

            DebugX = strX;
            DebugY = strY;
            DebugZ = strZ;*/

            ObjectId = GetInt(0);
            LodGeometry = GetString(1);
            CellId = GetInt(2);
            Position = new UnityEngine.Vector3(GetSingle(3), GetSingle(5), GetSingle(4));
            Rotation = new UnityEngine.Quaternion(GetSingle(6), GetSingle(8), GetSingle(7), GetSingle(9));
            LodIndex = GetInt(10);
        }

        public Instance(BinaryReader reader) : base(reader)
        {
            var posX = reader.ReadSingle();
            var posZ = reader.ReadSingle();
            var posY = reader.ReadSingle();

            Position = new UnityEngine.Vector3(posX, posY, posZ);

            var rotX = reader.ReadSingle();
            var rotZ = reader.ReadSingle();
            var rotY = reader.ReadSingle();
            var rotW = reader.ReadSingle();

            Rotation = new UnityEngine.Quaternion(rotX, rotY, rotZ, rotW);

            ObjectId = reader.ReadInt32();
            CellId = reader.ReadInt32();
            LodIndex = reader.ReadInt32();
        }
    }
}