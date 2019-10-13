using System;
using System.IO;

/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio file abstract class
    /// </summary>
    public abstract class AGTAAudioFile : IDisposable
    {
        /// <summary>
        /// Name
        /// </summary>
        private string name;

        /// <summary>
        /// File stream
        /// </summary>
        private FileStream fileStream;

        /// <summary>
        /// Bank data
        /// </summary>
        private GTAAudioBankData[] bankData;

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                if (name == null)
                {
                    name = "";
                }
                return name;
            }
        }

        /// <summary>
        /// File stream
        /// </summary>
        internal FileStream FileStream
        {
            get
            {
                return fileStream;
            }
        }

        /// <summary>
        /// Bank data
        /// </summary>
        internal GTAAudioBankData[] BankData
        {
            get
            {
                if (bankData == null)
                {
                    bankData = new GTAAudioBankData[0];
                }
                return bankData;
            }
        }

        /// <summary>
        /// Number of banks
        /// </summary>
        public int NumBanks
        {
            get
            {
                return BankData.Length;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="bankData">Bank data</param>
        internal AGTAAudioFile(string name, FileStream fileStream, GTAAudioBankData[] bankData)
        {
            this.name = name;
            this.fileStream = fileStream;
            this.bankData = bankData;
        }

        /// <summary>
        /// Is bank available
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <returns>"true" if bank is available, otherwise "false"</returns>
        public bool IsBankAvailable(uint bankIndex)
        {
            return (bankIndex < NumBanks);
        }

        /// <summary>
        /// Is audio clip available
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <param name="audioClipIndex">Bank index</param>
        /// <returns>"true" if audio clip is available, otherwise "false"</returns>
        public bool IsAudioClipAvailableFromBank(uint bankIndex, uint audioClipIndex)
        {
            return GetBankData(bankIndex).IsAudioClipAvailable(audioClipIndex);
        }

        /// <summary>
        /// Get bank data
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <returns>Bank data</returns>
        public GTAAudioBankData GetBankData(uint bankIndex)
        {
            if (!(IsBankAvailable(bankIndex)))
            {
                throw new IndexOutOfRangeException("Bank index: " + bankIndex + "; Number of banks: " + NumBanks);
            }
            return BankData[bankIndex];
        }

        /// <summary>
        /// Get number of audio clips from bank
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <returns>Number of audio clips</returns>
        public int GetNumAudioClipsFromBank(uint bankIndex)
        {
            return GetBankData(bankIndex).NumAudioClips;
        }

        /// <summary>
        /// Get audio clip data
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <param name="audioClipIndex">Audio clip index</param>
        /// <param name="result">Result</param>
        /// <returns>Audio clip data</returns>
        public GTAAudioAudioClipData GetAudioClipData(uint bankIndex, uint audioClipIndex)
        {
            return GetBankData(bankIndex).GetAudioClipData(audioClipIndex);
        }

        /// <summary>
        /// Open audio stream
        /// </summary>
        /// <param name="bankIndex">Bank index</param>
        /// <param name="audioIndex">Audio index</param>
        /// <param name="bankSlot">Bank slot</param>
        /// <returns>Audio stream</returns>
        public abstract Stream Open(uint bankIndex, uint audioIndex);

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            if (fileStream != null)
            {
                fileStream.Dispose();
                fileStream = null;
            }
        }
    }
}
