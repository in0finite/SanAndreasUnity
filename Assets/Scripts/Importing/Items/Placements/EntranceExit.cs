
namespace SanAndreasUnity.Importing.Items.Placements
{
    [System.Flags]
    public enum EnexFlags
    {
        
    }

    [Section("enex")]
    public class EntranceExit : Placement
    {
        
        public readonly UnityEngine.Vector3 EntrancePos; // position where enex is located
        public readonly float EntranceAngle; // rotation of ped when entering enex
        public readonly UnityEngine.Vector3 Size;
        public readonly UnityEngine.Vector3 ExitPos; // position of ped after teleporting to this enex
        public readonly float ExitAngle; // rotation of ped after teleporting to this enex
        public readonly int TargetInterior; // interior level where enex is located
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
            Name = Name.Substring(1, Name.Length - 2);  // remove quotes
            SkyColorType = GetInt(index++);
            NumPedsToSpawn = GetInt(index++);
            TimeOn = GetInt(index++);
            TimeOff = GetInt(index++);

        }
    }
}