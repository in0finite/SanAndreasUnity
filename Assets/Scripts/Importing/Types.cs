using System;
using System.IO;

namespace SanAndreasUnity.Importing
{
    public struct Vector2
    {
        public readonly Single X;
        public readonly Single Y;

        public Vector2(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
        }
    }

    public struct Vector3
    {
        public readonly Single X;
        public readonly Single Y;
        public readonly Single Z;

        public Vector3(BinaryReader reader, bool compressed = false)
        {
            if (!compressed) {
                X = reader.ReadSingle();
                Y = reader.ReadSingle();
                Z = reader.ReadSingle();
            } else {
                X = reader.ReadInt16() / 128f;
                Y = reader.ReadInt16() / 128f;
                Z = reader.ReadInt16() / 128f;
            }
        }
    }
}
