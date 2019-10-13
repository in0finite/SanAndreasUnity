using System;

/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio bank data structure
    /// </summary>
    public struct GTAAudioBankData
    {
        /// <summary>
        /// Offset
        /// </summary>
        public readonly uint Offset;

        /// <summary>
        /// Length
        /// </summary>
        public readonly uint Length;

        /// <summary>
        /// Audio clip data
        /// </summary>
        private GTAAudioAudioClipData[] audioClipData;

        /// <summary>
        /// Audio clip data
        /// </summary>
        internal GTAAudioAudioClipData[] AudioClipData
        {
            get
            {
                if (audioClipData == null)
                {
                    audioClipData = new GTAAudioAudioClipData[0];
                }
                return audioClipData;
            }
        }

        /// <summary>
        /// Number of audio clips
        /// </summary>
        public int NumAudioClips
        {
            get
            {
                return AudioClipData.Length;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        internal GTAAudioBankData(uint offset, uint length)
        {
            Offset = offset;
            Length = length;
            audioClipData = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="audioClipData">Audio clip data</param>
        internal GTAAudioBankData(uint offset, uint length, GTAAudioAudioClipData[] audioClipData)
        {
            Offset = offset;
            Length = length;
            this.audioClipData = audioClipData;
        }

        /// <summary>
        /// Is audio clip available
        /// </summary>
        /// <param name="audioClipIndex">Audio clip index</param>
        /// <returns>"true" if audio clip is available, otherwise "false"</returns>
        public bool IsAudioClipAvailable(uint audioClipIndex)
        {
            return (audioClipIndex < NumAudioClips);
        }

        /// <summary>
        /// Get audio clip data
        /// </summary>
        /// <param name="audioClipIndex">Audio clip index</param>
        /// <returns>Audio clip data</returns>
        public GTAAudioAudioClipData GetAudioClipData(uint audioClipIndex)
        {
            if (!(IsAudioClipAvailable(audioClipIndex)))
            {
                throw new IndexOutOfRangeException("Audio clip index: " + audioClipIndex + "; Number of audio clips: " + NumAudioClips);
            }
            return AudioClipData[audioClipIndex];
        }
    }
}
