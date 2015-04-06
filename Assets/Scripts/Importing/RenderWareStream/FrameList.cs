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

        public String Name;

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
        {
            var data = Section<Data>.ReadData(stream);
            var reader = new BinaryReader(new MemoryStream(data.Value));

            FrameCount = reader.ReadUInt32();

            Frames = new Frame[FrameCount];

            for (var i = 0; i < FrameCount; ++i) {
                Frames[i] = new Frame(i, reader);
            }

            for (var i = 0; i < FrameCount; ++i) {
                var extension = Section<Extension>.ReadData(stream);
                var frameName = extension.FirstOrDefault<FrameName>();
                if (frameName != null) Frames[i].Name = frameName.Name;

                var hanim = extension.FirstOrDefault<HierarchyAnimation>();

                if (hanim != null)
                {

                }
            }
        }
    }
}
