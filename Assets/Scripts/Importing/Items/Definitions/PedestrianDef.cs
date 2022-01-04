using System;
using System.Globalization;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    [Flags]
    public enum PedestrianType
    {
        Player1 = 0,

        Civilian = 8,
        CivMale = Civilian | 0,
        CivFemale = Civilian | 1,

        Criminal = 16,
        Prostitute = Criminal | 1,

        EmergencyServices = 32,
        Cop = EmergencyServices | 0,
        Medic = EmergencyServices | 1,
        FireMan = EmergencyServices | 2,

        GangMember = 64,
        Gang1 = GangMember | 0,
        Gang2 = GangMember | 1,
        Gang3 = GangMember | 2,
        Gang4 = GangMember | 3,
        Gang5 = GangMember | 4,
        Gang6 = GangMember | 5,
        Gang7 = GangMember | 6,
        Gang8 = GangMember | 7
    }

    public static class PedestrianTypeExtensions
    {
        public static bool IsGangMember(this PedestrianType pedestrianType)
        {
            return (pedestrianType & PedestrianType.GangMember) != 0;
        }

        public static bool IsCriminal(this PedestrianType pedestrianType)
        {
            return pedestrianType == PedestrianType.Criminal;
        }

        public static bool IsCop(this PedestrianType pedestrianType)
        {
            return pedestrianType == PedestrianType.Cop;
        }

        public static bool IsFemale(this PedestrianType pedestrianType)
        {
            return pedestrianType == PedestrianType.CivFemale || pedestrianType == PedestrianType.Prostitute;
        }

        public static bool IsMale(this PedestrianType pedestrianType)
        {
            return !pedestrianType.IsFemale();
        }
    }

    [Section("peds")]
    public class PedestrianDef : Definition, IObjectDefinition
    {
        public readonly int Id;

        int IObjectDefinition.Id
        {
            get { return Id; }
        }

        public readonly string ModelName;
        public readonly string TextureDictionaryName;
        public readonly PedestrianType DefaultType;
        public readonly string BehaviourName;
        public readonly string AnimGroupName;
        public readonly uint CanDriveMask;
        public readonly uint Flags;
        public readonly string AnimFileName;
        public readonly int Radio1;
        public readonly int Radio2;

        public PedestrianDef(string line)
            : base(line)
        {
            Id = GetInt(0);
            ModelName = GetString(1);
            TextureDictionaryName = GetString(2);
            Enum.TryParse<PedestrianType>(GetString(3), true, out DefaultType);
            BehaviourName = GetString(4);
            AnimGroupName = GetString(5);
            CanDriveMask = (uint)GetInt(6, NumberStyles.HexNumber);
            Flags = (uint)GetInt(7);
            AnimFileName = GetString(8);
            Radio1 = GetInt(9);
            Radio2 = GetInt(10);
        }
    }
}