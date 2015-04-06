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

    public class AnimationPackage : Section
    {
        public AnimationPackage(BinaryReader reader)
            : base(reader)
        {
            string identifier = reader.ReadString(4);
            Int32 fileSize = reader.ReadInt32();
            string internalFileName = reader.ReadString(24);
            Int32 animationCount = reader.ReadInt32();

        }
    }
}
