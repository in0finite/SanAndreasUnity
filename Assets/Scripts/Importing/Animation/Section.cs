using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Animation
{
    public abstract class Section
    {
        public readonly string Identifier;
        public readonly long Length;

        protected Section(BinaryReader reader)
        {
            Identifier = reader.ReadString(4);
            Length = reader.ReadUInt32();
        }
    }

    public class Animation
    {
        public readonly string Name;
        public readonly Int32 BoneCount;
        public readonly Int32 FrameLength;
        public readonly Int32 Unknown;
        public readonly Bone[] Bones;

        public Animation(BinaryReader reader)
        {
            Name = reader.ReadString(24);
            BoneCount = reader.ReadInt32();
            FrameLength = reader.ReadInt32();
            Unknown = reader.ReadInt32();

            Bones = new Bone[BoneCount];

            for (int i = 0; i < BoneCount; ++i)
            {
                Bones[i] = new Bone(reader);
            }
        }
    }

    public class Bone
    {
        public readonly string Name;
        public readonly Int32 FrameType;
        public readonly Int32 FrameCount;
        public readonly Int32 BoneId;
        public readonly Frame[] Frames;

        public Bone(BinaryReader reader)
        {
            Name = reader.ReadString(24);
            FrameType = reader.ReadInt32();
            FrameCount = reader.ReadInt32();
            BoneId = reader.ReadInt32();

            Frames = new Frame[FrameCount];

            for (int i = 0; i < FrameCount; ++i)
            {
                Frames[i] = new Frame(reader, FrameType == 4);
            }
        }
    }

    public class Frame
    {
        public readonly Vector3 Translation;
        public readonly Quaternion Rotation;

        public readonly Int16 Time;

        public Frame(BinaryReader reader, bool root)
        {
            Rotation = new Quaternion(reader, QuaternionCompression.Animation);

            Time = reader.ReadInt16();

            if (root)
            {
                Translation = new Vector3(reader, VectorCompression.Animation);
            }
        }
    }

    public class AnimationPackage : Section
    {
        public readonly string FileName;
        public readonly Int32 AnimationCount;
        public readonly Animation[] Animations;

        public AnimationPackage(BinaryReader reader)
            : base(reader)
        {
            FileName = reader.ReadString(24);
            AnimationCount = reader.ReadInt32();

            Animations = new Animation[AnimationCount];

            for (int i = 0; i < AnimationCount; ++i)
            {
                Animations[i] = new Animation(reader);
            }
        }
    }
}
