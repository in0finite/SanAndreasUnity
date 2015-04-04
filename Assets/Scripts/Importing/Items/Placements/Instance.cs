using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SanAndreasUnity.Importing.Items.Placements
{
    public static class InstanceExtensions
    {
        public static void ResolveLod(this IList<Instance> insts)
        {
            foreach (var inst in insts) {
                if (inst.LodIndex != -1) {
                    var lod = inst.LodInstance = insts[inst.LodIndex];
                    lod.IsLod = true;
                }
            }
        }
    }

    [Section("inst")]
    public class Instance : Item
    {
        public readonly int ObjectId;
        public readonly string LodGeometry;
        public readonly int CellId;
        public readonly UnityEngine.Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly int LodIndex;

        public Instance LodInstance { get; internal set; }
        public Definitions.Object Object { get; internal set; }

        public bool IsLod { get; internal set; }

        public Instance(string line) : base(line)
        {
            ObjectId = GetInt(0);
            LodGeometry = GetString(1);
            CellId = GetInt(2);
            Position = new UnityEngine.Vector3(GetSingle(3), GetSingle(5), GetSingle(4));
            Rotation = new Quaternion(GetSingle(6), GetSingle(8), GetSingle(7), GetSingle(9));
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

            Rotation = new Quaternion(rotX, rotY, rotZ, rotW);

            ObjectId = reader.ReadInt32();
            CellId = reader.ReadInt32();
            LodIndex = reader.ReadInt32();
        }
    }
}
