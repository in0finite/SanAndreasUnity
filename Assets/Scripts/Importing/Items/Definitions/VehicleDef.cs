using System;
using System.Globalization;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    public enum VehicleType
    {
        Trailer,
        Bmx,
        Bike,
        Train,
        Boat,
        Plane,
        Heli,
        Quad,
        MTruck,
        Car
    }

    [Section("cars")]
    public class VehicleDef : Definition, IObjectDefinition
    {
        public struct CompRulesUnion
        {
            public int nExtraA_comp1;
            public int nExtraA_comp2;
            public int nExtraA_comp3;

            public int nExtraB_comp1;
            public int nExtraB_comp2;
            public int nExtraB_comp3;

            public int nExtraAComp;
            public int nExtraARule;
            public int nExtraBComp;
            public int nExtraBRule;

            public int nExtraA;
            public int nExtraB;

            public CompRulesUnion(int value)
            {
                nExtraA = (value & 0x0000FFFF) >> 0;
                nExtraB = (int)(value & 0xFFFF0000) >> 16;

                nExtraAComp = (nExtraA & 0x0FFF) >> 0;
                nExtraARule = (nExtraA & 0xF000) >> 12;

                nExtraBComp = (nExtraB & 0x0FFF) >> 0;
                nExtraBRule = (nExtraB & 0xF000) >> 12;

                nExtraA_comp1 = (nExtraA & 0x000F) >> 0;
                nExtraA_comp2 = (nExtraA & 0x00F0) >> 4;
                nExtraA_comp3 = (nExtraA & 0x0F00) >> 8;

                nExtraB_comp1 = (nExtraB & 0x000F) >> 0;
                nExtraB_comp2 = (nExtraB & 0x00F0) >> 4;
                nExtraB_comp3 = (nExtraB & 0x0F00) >> 8;
            }

            public Boolean HasExtraOne()
            {
                return nExtraA != 0;
            }

            public Boolean HasExtraTwo()
            {
                return nExtraB != 0;
            }
        }

        public readonly int Id;

        int IObjectDefinition.Id
        {
            get { return Id; }
        }

        public readonly string ModelName;
        public readonly string TextureDictionaryName;

        public readonly VehicleType VehicleType;

        public readonly string HandlingName;
        public readonly string GameName;
        public readonly string AnimsName;
        public readonly string ClassName;

        public readonly int Frequency;
        public readonly int Flags;
        public readonly CompRulesUnion CompRules;

        public readonly bool HasWheels;

        public readonly int WheelId;
        public readonly float WheelScaleFront;
        public readonly float WheelScaleRear;

        public readonly int UpgradeId;

        public VehicleDef(string line) : base(line)
        {
            Id = GetInt(0);

            ModelName = GetString(1);
            TextureDictionaryName = GetString(2);

            VehicleType = (VehicleType)Enum.Parse(typeof(VehicleType), GetString(3), true);

            HandlingName = GetString(4);
            GameName = GetString(5);
            AnimsName = GetString(6);
            ClassName = GetString(7);

            Frequency = GetInt(8);
            Flags = GetInt(9);
            CompRules = new CompRulesUnion(GetInt(10, NumberStyles.HexNumber));

            HasWheels = Parts >= 15;

            if (HasWheels)
            {
                WheelId = GetInt(11);
                WheelScaleFront = GetSingle(12);
                WheelScaleRear = GetSingle(13);
                UpgradeId = GetInt(14);
            }
        }
    }
}