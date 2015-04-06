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
        public Object[] Objects;

        public Animation(BinaryReader reader)
        {
            string animationName = reader.ReadString(24);
            Int32 objectCount = reader.ReadInt32();
            Int32 frameLength = reader.ReadInt32();
            Int32 unknown = reader.ReadInt32();

            Objects = new Object[objectCount];

            for (int i = 0; i < objectCount; ++i)
            {
                Objects[i] = new Object(reader);
            }
        }
    }

    public class Object
    {
        public readonly Frame[] Frames;

        public Object(BinaryReader reader)
        {
            string objectName = reader.ReadString(24);
            Int32 frameType = reader.ReadInt32();
            Int32 frameCount = reader.ReadInt32();
            Int32 boneId = reader.ReadInt32();

            Frames = new Frame[frameCount];

            for (int i = 0; i < frameCount; ++i)
            {
                Frames[i] = new Frame(reader);
            }
        }
    }


    public class Frame
    {
        public readonly Int16 QuaternionX;
        public readonly Int16 QuaternionY;
        public readonly Int16 QuaternionZ;
        public readonly Int16 QuaternionW;

        public readonly Int16 Time;

        public readonly Int16 TranslationX;
        public readonly Int16 TranslationY;
        public readonly Int16 TranslationZ;

        public Frame(BinaryReader reader)
        {
            QuaternionX = reader.ReadInt16();
            QuaternionY = reader.ReadInt16();
            QuaternionZ = reader.ReadInt16();
            QuaternionW = reader.ReadInt16();

            Time = reader.ReadInt16();

            TranslationX = reader.ReadInt16();
            TranslationY = reader.ReadInt16();
            TranslationZ = reader.ReadInt16();
        }
    }

    public class AnimationPackage : Section
    {
        public readonly Animation[] Animations;

        public AnimationPackage(BinaryReader reader)
            : base(reader)
        {
            string internalFileName = reader.ReadString(24);
            Int32 animationCount = reader.ReadInt32();

            Animations = new Animation[animationCount];

            for (int i = 0; i < animationCount; ++i)
            {
                Animations[i] = new Animation(reader);
            }
        }
    }
}
