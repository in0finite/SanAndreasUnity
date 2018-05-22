using System;
using System.IO;

namespace SanAndreasUnity.Importing
{
    public enum VectorCompression
    {
        None,
        Collision,
        Animation,
    }

    public enum QuaternionCompression
    {
        None,
        Animation,
    }

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
        public const int Size = 3 * sizeof(float);
        public const int SizeCompressed = 3 * sizeof(short);

        public readonly Single X;
        public readonly Single Y;
        public readonly Single Z;

        public Vector3(BinaryReader reader, VectorCompression compression = VectorCompression.None)
        {
            if (compression == VectorCompression.None)
            {
                X = reader.ReadSingle();
                Y = reader.ReadSingle();
                Z = reader.ReadSingle();
            }
            else
            {
                float compressionScale;

                switch (compression)
                {
                    case VectorCompression.Collision:
                        compressionScale = 128.0f;
                        break;

                    case VectorCompression.Animation:
                        compressionScale = 1024.0f;
                        break;

                    default:
                        compressionScale = 1.0f;
                        break;
                }

                X = reader.ReadInt16() / compressionScale;
                Y = reader.ReadInt16() / compressionScale;
                Z = reader.ReadInt16() / compressionScale;
            }
        }
    }

    public struct Vector4
    {
        public readonly Single X;
        public readonly Single Y;
        public readonly Single Z;
        public readonly Single W;

        public Vector4(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            W = reader.ReadSingle();
        }
    }

    public struct Quaternion
    {
        public readonly Single X;
        public readonly Single Y;
        public readonly Single Z;
        public readonly Single W;

        public Quaternion(BinaryReader reader, QuaternionCompression compression = QuaternionCompression.None)
        {
            if (compression == QuaternionCompression.None)
            {
                X = reader.ReadSingle();
                Y = reader.ReadSingle();
                Z = reader.ReadSingle();
                W = reader.ReadSingle();
            }
            else
            {
                float compressionScale = 4096.0f;

                X = reader.ReadInt16() / compressionScale;
                Y = reader.ReadInt16() / compressionScale;
                Z = reader.ReadInt16() / compressionScale;
                W = reader.ReadInt16() / compressionScale;
            }
        }
    }

    public struct Matrix4x4
    {
        public readonly Vector4 V0;
        public readonly Vector4 V1;
        public readonly Vector4 V2;
        public readonly Vector4 V3;

        public Matrix4x4(BinaryReader reader)
        {
            V0 = new Vector4(reader);
            V1 = new Vector4(reader);
            V2 = new Vector4(reader);
            V3 = new Vector4(reader);
        }
    }
}