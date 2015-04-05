using System;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(14)]
    public class FrameList : SectionData
    {
        public readonly UInt32 FrameCount;
        public readonly Frame[] Frames;

        public FrameList(SectionHeader header, Stream stream)
        {
            var data = Section<Data>.ReadData(stream);

            FrameCount = BitConverter.ToUInt32(data.Value, 0);
            Frames = new Frame[FrameCount];

            for (var i = 0; i < FrameCount; ++i)
            {
                SectionHeader.Read(stream); // extension
                Frames[i] = Section<Frame>.ReadData(stream);
            }
        }
    }
}
