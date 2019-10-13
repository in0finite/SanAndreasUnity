/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio audio clip data structure
    /// </summary>
    public struct GTAAudioAudioClipData
    {
        /// <summary>
        /// Sound buffer offset
        /// </summary>
        public readonly uint SoundBufferOffset;

        /// <summary>
        /// Loop offset
        /// </summary>
        public readonly uint LoopOffset;

        /// <summary>
        /// Sample rate
        /// </summary>
        public readonly ushort SampleRate;

        /// <summary>
        /// Sound headroom
        /// </summary>
        public readonly ushort SoundHeadroom;

        /// <summary>
        /// Length
        /// </summary>
        public readonly uint Length;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="soundBufferOffset">Sound buffer offset</param>
        /// <param name="loopOffset">Loop offset</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="soundHeadroom">Sound headroom</param>
        /// <param name="length">Length</param>
        internal GTAAudioAudioClipData(uint soundBufferOffset, uint loopOffset, ushort sampleRate, ushort soundHeadroom, uint length)
        {
            SoundBufferOffset = soundBufferOffset;
            LoopOffset = loopOffset;
            SampleRate = sampleRate;
            SoundHeadroom = soundHeadroom;
            Length = length;
        }
    }
}
