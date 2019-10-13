/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio stream class
    /// </summary>
    public class GTAAudioStream : CommitableMemoryStream
    {
        /// <summary>
        /// Sample rate
        /// </summary>
        public readonly ushort SampleRate;

        /// <summary>
        /// Loop offset
        /// </summary>
        public readonly uint LoopOffset;

        /// <summary>
        /// Sound headroom
        /// </summary>
        public readonly uint SoundHeadroom;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="loopOffset">Loop offset</param>
        /// <param name="soundHeadroom">Loop offset</param>
        internal GTAAudioStream(AGTAAudioFile gtaAudioFile, ushort sampleRate, uint loopOffset, uint soundHeadroom) : base(gtaAudioFile)
        {
            SampleRate = sampleRate;
            LoopOffset = loopOffset;
            SoundHeadroom = soundHeadroom;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="loopOffset">Loop offset</param>
        /// <param name="buffer">Buffer</param>
        internal GTAAudioStream(AGTAAudioFile gtaAudioFile, ushort sampleRate, uint loopOffset, uint soundHeadroom, byte[] buffer) : base(gtaAudioFile, buffer)
        {
            SampleRate = sampleRate;
            LoopOffset = loopOffset;
            SoundHeadroom = soundHeadroom;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="loopOffset">Loop offset</param>
        /// <param name="capacity">Capacity</param>
        internal GTAAudioStream(AGTAAudioFile gtaAudioFile, ushort sampleRate, uint loopOffset, uint soundHeadroom, int capacity) : base(gtaAudioFile, capacity)
        {
            SampleRate = sampleRate;
            LoopOffset = loopOffset;
            SoundHeadroom = soundHeadroom;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="loopOffset">Loop offset</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="writable">Writable</param>
        internal GTAAudioStream(AGTAAudioFile gtaAudioFile, ushort sampleRate, uint loopOffset, uint soundHeadroom, byte[] buffer, bool writable) : base(gtaAudioFile, buffer, writable)
        {
            SampleRate = sampleRate;
            LoopOffset = loopOffset;
            SoundHeadroom = soundHeadroom;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="loopOffset">Loop offset</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        internal GTAAudioStream(AGTAAudioFile gtaAudioFile, ushort sampleRate, uint loopOffset, uint soundHeadroom, byte[] buffer, int index, int count) : base(gtaAudioFile, buffer, index, count)
        {
            SampleRate = sampleRate;
            LoopOffset = loopOffset;
            SoundHeadroom = soundHeadroom;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="loopOffset">Loop offset</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <param name="writable">Writable</param>
        internal GTAAudioStream(AGTAAudioFile gtaAudioFile, ushort sampleRate, uint loopOffset, uint soundHeadroom, byte[] buffer, int index, int count, bool writable) : base(gtaAudioFile, buffer, index, count, writable)
        {
            SampleRate = sampleRate;
            LoopOffset = loopOffset;
            SoundHeadroom = soundHeadroom;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="loopOffset">Loop offset</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <param name="writable">Writable</param>
        /// <param name="publiclyVisible">Publicly visible</param>
        internal GTAAudioStream(AGTAAudioFile gtaAudioFile, ushort sampleRate, uint loopOffset, uint soundHeadroom, byte[] buffer, int index, int count, bool writable, bool publiclyVisible) : base(gtaAudioFile, buffer, index, count, writable, publiclyVisible)
        {
            SampleRate = sampleRate;
            LoopOffset = loopOffset;
            SoundHeadroom = soundHeadroom;
        }
    }
}
