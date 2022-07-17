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
            private readonly int value;

            public int nExtraA_comp1 { get { return (nExtraA & 0x000F) >> 0; } }
            public int nExtraA_comp2 { get { return (nExtraA & 0x00F0) >> 4; } }
            public int nExtraA_comp3 { get { return (nExtraA & 0x0F00) >> 8; } }

            public int nExtraB_comp1 { get { return (nExtraB & 0x000F) >> 0; } }
            public int nExtraB_comp2 { get { return (nExtraB & 0x00F0) >> 4; } }
            public int nExtraB_comp3 { get { return (nExtraB & 0x0F00) >> 8; } }

            public int nExtraAComp { get { return (nExtraA & 0x0FFF) >> 0; } }
            public int nExtraARule { get { return (nExtraA & 0xF000) >> 12; } }
            public int nExtraBComp { get { return (nExtraB & 0x0FFF) >> 0; } }
            public int nExtraBRule { get { return (nExtraB & 0xF000) >> 12; } }

            public int nExtraA { get { return (value & 0xFFFF) >> 0; } }
            public int nExtraB { get { return (int)(value & 0xFFFF0000) >> 16; } }

            public CompRulesUnion(int value)
            {
                this.value = value;
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