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

    public class Clip
    {
        public readonly string Name;
        public readonly Int32 BoneCount;
        public readonly Int32 FrameLength;
        public readonly Int32 Unknown;
        public readonly Bone[] Bones;

        public Clip(BinaryReader reader)
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
        private readonly Dictionary<string, Clip> _namedClips
            = new Dictionary<string,Clip>(StringComparer.InvariantCultureIgnoreCase);

        public readonly string FileName;
        public readonly Int32 ClipCount;
        public readonly Clip[] Clips;

        public AnimationPackage(BinaryReader reader)
            : base(reader)
        {
            FileName = reader.ReadString(24);
            ClipCount = reader.ReadInt32();

            Clips = new Clip[ClipCount];

            for (int i = 0; i < ClipCount; ++i)
            {
                var clip = Clips[i] = new Clip(reader);
                _namedClips.Add(clip.Name, clip);
            }

            //DebugPrint();
        }

        public Clip this[string name]
        {
            get { return _namedClips[name]; }
        }

        public void DebugPrint()
        {
            string s = "";

            for (int i = 0; i < ClipCount; ++i)
            {
                var anim = Clips[i];

                s += string.Format("(#{0}) {1}\n", i, anim.Name);

                for (int j = 0; j < anim.BoneCount; ++j)
                {
                    var bone = anim.Bones[j];

                    s += string.Format("{0} (id: {1})\n", bone.Name, bone.BoneId);
                }
            }

            File.WriteAllText("anim.txt", s);
        }
    }
}
