
namespace SanAndreasUnity.Importing.Items.Placements
{
    [System.Flags]
    public enum EnexFlags
    {
        
    }

    [Section("enex")]
    public class EntranceExit : Placement
    {
        
        public readonly UnityEngine.Vector3 EntrancePos;
        public readonly float EntranceAngle;
        public readonly UnityEngine.Vector3 Size;
        public readonly UnityEngine.Vector3 ExitPos;
        public readonly float ExitAngle;
        public readonly int TargetInterior;
        public readonly int Flags;
        public readonly string Name;
        public readonly int SkyColorType;
        public readonly int NumPedsToSpawn;
        public readonly int TimeOn;
        public readonly int TimeOff;


        public EntranceExit(string line) : base(line)
        {
            int index = 0;

            EntrancePos = GetUnityVec3(ref index, true);
            EntranceAngle = GetSingle(index++);
            Size = GetUnityVec3(ref index, false);
            ExitPos = GetUnityVec3(ref index, true);
            ExitAngle = GetSingle(index++);
            TargetInterior = GetInt(index++);
            Flags = GetInt(index++);
            Name = GetString(index++);
            SkyColorType = GetInt(index++);
            NumPedsToSpawn = GetInt(index++);
            TimeOn = GetInt(index++);
            TimeOff = GetInt(index++);

        }
    }
}