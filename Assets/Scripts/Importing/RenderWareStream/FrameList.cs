using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(14)]
    public class FrameList : SectionData
    {
        public readonly UInt32 FrameCount;

        public struct Frame
        {
            public Vector3 Right;
            public Vector3 Up;
            public Vector3 Forward;

            public Vector3 Position;

            public Int32 Index;
            public Int32 ParentIndex;
            public UInt32 MatrixFlags;

            public String Name;
        }

        public readonly Frame[] Frames;

        public FrameList(SectionHeader header, Stream stream)
        {
            var data = Section<Data>.ReadData(stream);
            var reader = new BinaryReader(new MemoryStream(data.Value));

            FrameCount = reader.ReadUInt32();

            Frames = new Frame[FrameCount];

            for (var i = 0; i < FrameCount; ++i)
            {
                Frames[i] = new Frame()
                {
                    Index = i,
                    Right = new Vector3(reader),
                    Up = new Vector3(reader),
                    Forward = new Vector3(reader),
                    Position = new Vector3(reader),
                    ParentIndex = reader.ReadInt32(),
                    MatrixFlags = reader.ReadUInt32(),
                };
            }

            for (var i = 0; i < FrameCount; ++i)
            {
                SectionHeader.Read(stream); // Extension
                Frames[i].Name = Section<FrameName>.ReadData(stream).Name;
            }
        }
    }
}
