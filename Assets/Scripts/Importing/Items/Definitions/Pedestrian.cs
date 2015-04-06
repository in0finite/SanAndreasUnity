using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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

    [Section("peds")]
    public class Pedestrian : Definition, IObjectDefinition
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

        public Pedestrian(string line)
            : base(line)
        {
            Id = GetInt(0);
            ModelName = GetString(1);
            TextureDictionaryName = GetString(2);
            DefaultType = (PedestrianType) Enum.Parse(typeof(PedestrianType), GetString(3), true);
            BehaviourName = GetString(4);
            AnimGroupName = GetString(5);
            CanDriveMask = (uint) GetInt(6, NumberStyles.HexNumber);
            Flags = (uint) GetInt(7);
            AnimFileName = GetString(8);
            Radio1 = GetInt(9);
            Radio2 = GetInt(10);
        }
    }
}
