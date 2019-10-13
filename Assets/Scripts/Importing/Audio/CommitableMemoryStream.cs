using System.IO;

/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// Commitable memory stream class
    /// </summary>
    public class CommitableMemoryStream : MemoryStream
    {
        /// <summary>
        /// GTA audio file
        /// </summary>
        private AGTAAudioFile gtaAudioFile;

        /// <summary>
        /// On close
        /// </summary>
        internal OnCloseGTAAudioFileEventHandler OnClose;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        internal CommitableMemoryStream(AGTAAudioFile gtaAudioFile) : base()
        {
            this.gtaAudioFile = gtaAudioFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="buffer">Buffer</param>
        internal CommitableMemoryStream(AGTAAudioFile gtaAudioFile, byte[] buffer) : base(buffer)
        {
            this.gtaAudioFile = gtaAudioFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="capacity">Capacity</param>
        internal CommitableMemoryStream(AGTAAudioFile gtaAudioFile, int capacity) : base(capacity)
        {
            this.gtaAudioFile = gtaAudioFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="writable">Writable</param>
        internal CommitableMemoryStream(AGTAAudioFile gtaAudioFile, byte[] buffer, bool writable) : base(buffer, writable)
        {
            this.gtaAudioFile = gtaAudioFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        internal CommitableMemoryStream(AGTAAudioFile gtaAudioFile, byte[] buffer, int index, int count) : base(buffer, index, count)
        {
            this.gtaAudioFile = gtaAudioFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <param name="writable">Writable</param>
        internal CommitableMemoryStream(AGTAAudioFile gtaAudioFile, byte[] buffer, int index, int count, bool writable) : base(buffer, index, count, writable)
        {
            this.gtaAudioFile = gtaAudioFile;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gtaAudioFile">GTA audio file</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <param name="writable">Writable</param>
        /// <param name="publiclyVisible">Publicly visible</param>
        internal CommitableMemoryStream(AGTAAudioFile gtaAudioFile, byte[] buffer, int index, int count, bool writable, bool publiclyVisible) : base(buffer, index, count, writable, publiclyVisible)
        {
            this.gtaAudioFile = gtaAudioFile;
        }

        /// <summary>
        /// Close();
        /// </summary>
        public override void Close()
        {
            if (OnClose != null)
            {
                OnClose(gtaAudioFile, this);
                OnClose = null;
            }
            base.Close();
        }
    }
}
