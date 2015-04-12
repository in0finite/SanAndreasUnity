using System;
using System.IO;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace SanAndreasUnity.Utilities
{
    public static class Tools
    {
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

        public static String ReadString(this BinaryReader reader, int length)
        {
            var bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes).TrimNullChars();
        }

        public static String TrimNullChars(this String str)
        {
            for (var i = 0; i < str.Length; ++i) if (str[i] == '\0') return str.Substring(0, i);

            return str;
        }
    }
}
