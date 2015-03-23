using System;
using System.IO;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace SanAndreasUnity.Utilities
{
    internal static class Tools
    {
        public static bool DoesExtend(this Type self, Type type)
        {
            return self.BaseType == type || (self.BaseType != null && self.BaseType.DoesExtend(type));
        }

        public static byte[] ReadBytes(this Stream self, int count)
        {
            var data = new byte[count];
            for (var i = 0; i < count; ++i) {
                var bt = self.ReadByte();
                if (bt == -1) throw new EndOfStreamException();

                data[i] = (byte) bt;
            }

            return data;
        }

        #region Clamps
        public static Byte Clamp(Byte value, Byte min, Byte max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static UInt16 Clamp(UInt16 value, UInt16 min, UInt16 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static UInt32 Clamp(UInt32 value, UInt32 min, UInt32 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static UInt64 Clamp(UInt64 value, UInt64 min, UInt64 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static SByte Clamp(SByte value, SByte min, SByte max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Int16 Clamp(Int16 value, Int16 min, Int16 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Int32 Clamp(Int32 value, Int32 min, Int32 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Int64 Clamp(Int64 value, Int64 min, Int64 max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Single Clamp(Single value, Single min, Single max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }

        public static Double Clamp(Double value, Double min, Double max)
        {
            return
                (value < min) ? min :
                (value > max) ? max :
                value;
        }
        #endregion Clamps

        public static int FloorDiv(int numer, int denom)
        {
            return (numer / denom) - (numer < 0 && (numer % denom) != 0 ? 1 : 0);
        }

        public static float WrapAngle(float ang)
        {
            return ang - (float) Math.Floor(ang / (Mathf.PI * 2f) + 0.5f) * Mathf.PI * 2f;
        }

        public static float WrapAngle(float ang, float basis)
        {
            return WrapAngle(ang - basis) + basis;
        }

        public static float AngleDif(float angA, float angB)
        {
            return WrapAngle(angA - angB);
        }

        public static String NextTexture(this Random rand, String prefix, int max)
        {
            if (!prefix.EndsWith("_"))
                prefix += "_";

            return prefix + rand.Next(max).ToString("X").ToLower();
        }

        public static String NextTexture(this Random rand, String prefix, int min, int max)
        {
            if (!prefix.EndsWith("_"))
                prefix += "_";

            return prefix + rand.Next(min, max).ToString("X").ToLower();
        }

        internal static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2 {
                x = reader.ReadSingle(),
                y = reader.ReadSingle()
            };
        }

        internal static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3 {
                x = -reader.ReadSingle(),
                z = reader.ReadSingle(),
                y = reader.ReadSingle()
            };
        }

        internal static String ReadString(this BinaryReader reader, int length)
        {
            var bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes).TrimNullChars();
        }

        internal static String TrimNullChars(this String str)
        {
            for (var i = 0; i < str.Length; ++i) if (str[i] == '\0') return str.Substring(0, i);

            return str;
        }
    }
}
