using System;
using System.IO;

/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio stream file class
    /// </summary>
    public class GTAAudioStreamsFile : AGTAAudioFile
    {
        /// <summary>
        /// Beats data
        /// </summary>
        private GTAAudioBeatData[] beatsData;

        /// <summary>
        /// Beats data
        /// </summary>
        internal GTAAudioBeatData[] BeatsData
        {
            get
            {
                if (beatsData == null)
                {
                    beatsData = new GTAAudioBeatData[0];
                }
                return beatsData;
            }
        }

        /// <summary>
        /// Number of beats
        /// </summary>
        public int NumBeats
        {
            get
            {
                return BeatsData.Length;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="bankData">Bank data</param>
        /// <param name="beatsData">Beats data</param>
        internal GTAAudioStreamsFile(string name, FileStream fileStream, GTAAudioBankData[] bankData, GTAAudioBeatData[] beatsData) : base(name, fileStream, bankData)
        {
            this.beatsData = beatsData;
        }

        /// <summary>
        /// Is beat data available
        /// </summary>
        /// <param name="beatIndex">Beat index</param>
        /// <returns>"true" if beat data is available, otherwise "false"</returns>
        public bool IsBeatDataAvailable(uint beatIndex)
        {
            return (beatIndex < BeatsData.Length);
        }

        /// <summary>
        /// Get beat data
        /// </summary>
        /// <param name="beatIndex">Beat index</param>
        /// <param name="result">Result</param>
        /// <returns>Beat data</returns>
        public GTAAudioBeatData GetBeatData(uint beatIndex)
        {
            if (!(IsBeatDataAvailable(beatIndex)))
            {
                throw new IndexOutOfRangeException("Beat index: " + beatIndex + "; Number of beats: " + NumBeats);
            }
            return BeatsData[beatIndex];
        }

        /// <summary>
        /// Open audio stream
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <param name="audioIndex">Audio index (unused)</param>
        /// <returns>Audio stream</returns>
        public override Stream Open(uint bankIndex, uint audioIndex)
        {
            Stream ret = null;
            if (FileStream != null)
            {
                if (bankIndex < BankData.Length)
                {
                    GTAAudioBankData bank_data = BankData[bankIndex];
                    uint offset = bank_data.Offset + 0x1F84;
                    if (FileStream.Length >= (offset + bank_data.Length))
                    {
                        DecodingBinaryReader reader = new DecodingBinaryReader(FileStream);
                        FileStream.Seek(offset, SeekOrigin.Begin);
                        byte[] data = reader.ReadDecodeBytes((int)(bank_data.Length));
                        if (data != null)
                        {
                            if (data.Length == bank_data.Length)
                            {
                                ret = new CommitableMemoryStream(this, data);
                                ret.Seek(0L, SeekOrigin.Begin);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Open audio stream
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <returns>Audio stream</returns>
        public Stream Open(uint bankIndex)
        {
            return Open(bankIndex, 0U);
        }
    }
}
