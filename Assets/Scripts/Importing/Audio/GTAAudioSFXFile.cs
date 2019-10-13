using System.IO;

/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio SFX file class
    /// </summary>
    public class GTAAudioSFXFile : AGTAAudioFile
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="bankData">Bank data</param>
        internal GTAAudioSFXFile(string name, FileStream fileStream, GTAAudioBankData[] bankData) : base(name, fileStream, bankData)
        {
            // ...
        }

        /// <summary>
        /// Open audio stream
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <param name="audioIndex">Audio index</param>
        /// <returns>GTA audio stream</returns>
        public override Stream Open(uint bankIndex, uint audioIndex)
        {
            GTAAudioStream ret = null;
            if (FileStream != null)
            {
                if (bankIndex < NumBanks)
                {
                    GTAAudioBankData bank_data = BankData[bankIndex];
                    if (audioIndex < bank_data.NumAudioClips)
                    {
                        GTAAudioAudioClipData audio_clip_data = bank_data.AudioClipData[audioIndex];
                        uint offset = bank_data.Offset + audio_clip_data.SoundBufferOffset + 0x12C4;
                        if (FileStream.Length >= (offset + audio_clip_data.Length))
                        {
                            byte[] data = new byte[audio_clip_data.Length];
                            FileStream.Seek(offset, SeekOrigin.Begin);
                            if (FileStream.Read(data, 0, data.Length) == data.Length)
                            {
                                ret = new GTAAudioStream(this, audio_clip_data.SampleRate, audio_clip_data.LoopOffset, audio_clip_data.SoundHeadroom, data);
                                ret.Seek(0L, SeekOrigin.Begin);
                            }
                        }
                    }
                }
            }
            return ret;
        }
    }
}
