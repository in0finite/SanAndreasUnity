using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public class Frame
    {
        public Frame(int index, BinaryReader reader)
        {
            Index = index;
            MatrixRight = new Vector3(reader);
            MatrixForward = new Vector3(reader);
            MatrixUp = new Vector3(reader);
            Position = new Vector3(reader);
            ParentIndex = reader.ReadInt32();
            MatrixFlags = reader.ReadUInt32();
        }

        public String Name { get; internal set; }
        public HierarchyAnimation HAnim { get; internal set; }

        public readonly Int32 Index;
        public readonly Int32 ParentIndex;

        public readonly Vector3 Position;

        public readonly Vector3 MatrixRight;
        public readonly Vector3 MatrixUp;
        public readonly Vector3 MatrixForward;

        public readonly UInt32 MatrixFlags;
    }

    [SectionType(14)]
    public class FrameList : SectionData
    {
        public readonly UInt32 FrameCount;

        public readonly Frame[] Frames;

        public FrameList(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var data = ReadSection<Data>();
            var reader = new BinaryReader(new MemoryStream(data.Value));

            FrameCount = reader.ReadUInt32();

            Frames = new Frame[FrameCount];

            for (var i = 0; i < FrameCount; ++i)
            {
                Frames[i] = new Frame(i, reader);
            }

            for (var i = 0; i < FrameCount; ++i)
            {
                var extension = ReadSection<Extension>();

                var frameName = extension.FirstOrDefault<FrameName>();
                var hierarchyAnimation = extension.FirstOrDefault<HierarchyAnimation>();

                if (frameName != null)
                {
                    Frames[i].Name = frameName.Name;
                }

                if (hierarchyAnimation != null)
                {
                    Frames[i].HAnim = hierarchyAnimation;
                }
            }
        }
    }
}