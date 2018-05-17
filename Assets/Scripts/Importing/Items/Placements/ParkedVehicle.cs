using System.IO;
using UnityEngine;

namespace SanAndreasUnity.Importing.Items.Placements
{
    [Section("cars")]
    public class ParkedVehicle : Placement
    {
        public readonly UnityEngine.Vector3 Position;
        public readonly float Angle;
        public readonly int CarId;
        public readonly int[] Colors;
        public readonly bool ForceSpawn;
        public readonly float AlarmProbability;
        public readonly float LockedProbability;

        public ParkedVehicle(UnityEngine.Vector3 pos, float ang, int carId,
            int primaryColor = -1, int secondaryColor = -1)
            : base("")
        {
            Position = pos;
            Angle = ang;
            CarId = carId;

            Colors = new int[4];

            Colors[0] = primaryColor;
            Colors[1] = secondaryColor;
            Colors[2] = -1;
            Colors[3] = -1;
        }

        public ParkedVehicle(string line)
            : base(line)
        {
            Position = new UnityEngine.Vector3(
                GetSingle(0),
                GetSingle(1),
                GetSingle(2));

            Angle = GetSingle(3) * Mathf.Rad2Deg;
            CarId = GetInt(4);

            Colors = new int[4];

            Colors[0] = GetInt(5);
            Colors[1] = GetInt(6);
            Colors[2] = -1;
            Colors[3] = -1;

            ForceSpawn = GetInt(7) > 0;
            AlarmProbability = GetInt(8) / 100f;
            LockedProbability = GetInt(9) / 100f;
        }

        public ParkedVehicle(BinaryReader reader)
            : base(reader)
        {
            var posX = reader.ReadSingle();
            var posZ = reader.ReadSingle();
            var posY = reader.ReadSingle();

            Position = new UnityEngine.Vector3(posX, posY, posZ);

            Angle = reader.ReadSingle() * Mathf.Rad2Deg;
            CarId = reader.ReadInt32();

            Colors = new int[4];

            Colors[0] = reader.ReadInt32();
            Colors[1] = reader.ReadInt32();

            ForceSpawn = reader.ReadInt32() > 0;
            AlarmProbability = reader.ReadInt32() / 100f;
            LockedProbability = reader.ReadInt32() / 100f;

            Colors[2] = reader.ReadInt32();
            Colors[3] = reader.ReadInt32();
        }
    }
}