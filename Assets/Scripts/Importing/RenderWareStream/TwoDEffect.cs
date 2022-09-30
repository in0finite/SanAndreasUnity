using System.Collections.Generic;
using System.IO;
using SanAndreasUnity.Importing.Conversion;
using UGameCore.Utilities;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(0x0253F2F8)]
    public class TwoDEffect : SectionData
    {
        public readonly uint NumEntries;

        public readonly List<Light> Lights;

        public TwoDEffect(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            NumEntries = reader.ReadUInt32();

            for (int i = 0; i < NumEntries; i++)
            {
                Vector3 position = new Vector3(reader);
                uint dataType = reader.ReadUInt32();
                uint dataSize = reader.ReadUInt32();

                long nextEntryPosition = stream.Position + dataSize;

                if (0 == dataType)
                {
                    // light

                    if (null == Lights)
                        Lights = new List<Light>(1);

                    Lights.Add(new Light(position, dataSize, reader));
                }

                stream.Position = nextEntryPosition;
            }

        }

        public readonly struct Light
        {
            public readonly UnityEngine.Vector3 Position;
            public readonly UnityEngine.Color Color;
            public readonly float CoronaFarClip;
            public readonly float PointlightRange;
            public readonly float CoronaSize;
            public readonly float ShadowSize;
            public readonly CoronaShowMode CoronaShowModeFlags;
            public readonly byte CoronaEnableReflection;
            public readonly byte CoronaFlareType;
            public readonly byte ShadowColorMultiplier;
            public readonly Flags1 Flags_1;
            public readonly string CoronaTexName;
            public readonly string ShadowTexName;
            public readonly byte ShadowZDistance;
            public readonly Flags2 Flags_2;
            public readonly byte LookDirectionX;
            public readonly byte LookDirectionY;
            public readonly byte LookDirectionZ;

            public enum CoronaShowMode : byte
            {
                DEFAULT = 0,
                RANDOM_FLASHING,
                RANDOM_FLASHIN_ALWAYS_AT_WET_WEATHER,
                LIGHTS_ANIM_SPEED_4X,
                LIGHTS_ANIM_SPEED_2X,
                LIGHTS_ANIM_SPEED_1X,
                TRAFFICLIGHT = 7,
                TRAINCROSSLIGHT,
                ALWAYS_DISABLED,
                AT_RAIN_ONLY,
                ON_5S_OFF_5S,
                ON_6S_OFF_4S,
                ON_6S_OFF_4S_2,
            }

            [System.Flags]
            public enum Flags1 : byte
            {
                CORONA_CHECK_OBSTACLES = 1,
                FOG_TYPE = 2,
                FOG_TYPE_2 = 4,
                WITHOUT_CORONA = 8,
                CORONA_ONLY_AT_LONG_DISTANCE = 16,
                AT_DAY = 32,
                AT_NIGHT = 64,
                BLINKING1 = 128,
            }

            [System.Flags]
            public enum Flags2 : byte
            {
                CORONA_ONLY_FROM_BELOW = 1,
                BLINKING2 = 2,
                UDPDATE_HEIGHT_ABOVE_GROUND = 4,
                CHECK_DIRECTION = 8,
                BLINKING3 = 16,
            }

            public Light(
                Vector3 position,
                uint dataSize,
                BinaryReader reader)
            {
                if (dataSize != 76 && dataSize != 80)
                    throw new System.Exception($"Size of data for light 2d effect must be 76 or 80, found {dataSize}");

                Position = Types.Convert(position);
                Color = Types.Convert(new Color4(reader));
                CoronaFarClip = reader.ReadSingle();
                PointlightRange = reader.ReadSingle();
                CoronaSize = reader.ReadSingle();
                ShadowSize = reader.ReadSingle();
                CoronaShowModeFlags = (CoronaShowMode) reader.ReadByte();
                CoronaEnableReflection = reader.ReadByte();
                CoronaFlareType = reader.ReadByte();
                ShadowColorMultiplier = reader.ReadByte();
                Flags_1 = (Flags1) reader.ReadByte();
                CoronaTexName = reader.ReadString(24);
                ShadowTexName = reader.ReadString(24);
                ShadowZDistance = reader.ReadByte();
                Flags_2 = (Flags2) reader.ReadByte();

                if (dataSize == 76)
                {
                    LookDirectionX = 0;
                    LookDirectionY = 0;
                    LookDirectionZ = 0;
                }
                else
                {
                    LookDirectionX = reader.ReadByte();
                    LookDirectionY = reader.ReadByte();
                    LookDirectionZ = reader.ReadByte();
                }

            }
        }
    }
}
