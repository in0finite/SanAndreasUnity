using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
    public class Vehicle : Definition, IObjectDefinition
    {
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
        public readonly int CompRules;

        public readonly bool HasWheels;

        public readonly int WheelId;
        public readonly float WheelScaleFront;
        public readonly float WheelScaleRear;

        public readonly int UpgradeId;

        public Vehicle(string line) : base(line)
        {
            Id = GetInt(0);

            ModelName = GetString(1);
            TextureDictionaryName = GetString(2);

            VehicleType = (VehicleType) Enum.Parse(typeof(VehicleType), GetString(3), true);
        
            HandlingName = GetString(4);
            GameName = GetString(5);
            AnimsName = GetString(6);
            ClassName = GetString(7);

            Frequency = GetInt(8);
            Flags = GetInt(9);
            CompRules = GetInt(10, NumberStyles.HexNumber);

            HasWheels = Parts >= 15;

            if (HasWheels) {
                WheelId = GetInt(11);
                WheelScaleFront = GetSingle(12);
                WheelScaleRear = GetSingle(13);
                UpgradeId = GetInt(14);
            }
        }
    }
}
