using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio files class
    /// </summary>
    public class GTAAudioFiles : IDisposable
    {
        /// <summary>
        /// SFX audio files
        /// </summary>
        private GTAAudioSFXFile[] sfxAudioFiles;

        /// <summary>
        /// Streams audio files
        /// </summary>
        private GTAAudioStreamsFile[] streamsAudioFiles;

        /// <summary>
        /// SFX files lookup
        /// </summary>
        private Dictionary<string, uint> sfxFilesLookup;

        /// <summary>
        /// Streams files lookup
        /// </summary>
        private Dictionary<string, uint> streamsFilesLookup;

        /// <summary>
        /// Script event volume
        /// </summary>
        private byte[] scriptEventVolume;

        /// <summary>
        /// SFX audio files
        /// </summary>
        public GTAAudioSFXFile[] SFXAudioFiles
        {
            get
            {
                if (sfxAudioFiles == null)
                {
                    sfxAudioFiles = new GTAAudioSFXFile[0];
                }
                return sfxAudioFiles.Clone() as GTAAudioSFXFile[];
            }
        }

        /// <summary>
        /// Streams audio files
        /// </summary>
        public GTAAudioStreamsFile[] StreamsAudioFiles
        {
            get
            {
                if (streamsAudioFiles == null)
                {
                    streamsAudioFiles = new GTAAudioStreamsFile[0];
                }
                return streamsAudioFiles as GTAAudioStreamsFile[];
            }
        }

        /// <summary>
        /// Number of events
        /// </summary>
        public int NumScriptEvents
        {
            get
            {
                return ((scriptEventVolume == null) ? 0 : scriptEventVolume.Length);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sfxAudioFiles">SFX audio banks files</param>
        /// <param name="streamsAudioFiles">Streams audio banks files</param>
        /// <param name="sfxFilesLookup">SFX banks files lookup</param>
        /// <param name="streamsFilesLookup">Streams banks files lookup</param>
        /// <param name="scriptEventVolume">Script event volume</param>
        internal GTAAudioFiles(GTAAudioSFXFile[] sfxAudioFiles, GTAAudioStreamsFile[] streamsAudioFiles, Dictionary<string, uint> sfxFilesLookup, Dictionary<string, uint> streamsFilesLookup, byte[] scriptEventVolume)
        {
            this.sfxAudioFiles = sfxAudioFiles;
            this.streamsAudioFiles = streamsAudioFiles;
            this.sfxFilesLookup = sfxFilesLookup;
            this.streamsFilesLookup = streamsFilesLookup;
            this.scriptEventVolume = scriptEventVolume;
        }

        /// <summary>
        /// Open SFX audio stream by index
        /// </summary>
        /// <param name="sfxIndex">SFX index</param>
        /// <param name="bankIndex">Bank Index</param>
        /// <param name="audioClipIndex">Audio clip index</param>
        /// <returns>GTA audio stream</returns>
        public GTAAudioStream OpenSFXAudioStreamByID(uint sfxIndex, uint bankIndex, uint audioClipIndex)
        {
            GTAAudioStream ret = null;
            if (sfxIndex < sfxAudioFiles.Length)
            {
                GTAAudioSFXFile sfx_file = sfxAudioFiles[sfxIndex];
                if (sfx_file != null)
                {
                    Stream stream = sfx_file.Open(bankIndex, audioClipIndex);
                    if (stream != null)
                    {
                        if (stream is GTAAudioStream)
                        {
                            ret = (GTAAudioStream)stream;
                        }
                        else
                        {
                            stream.Dispose();
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Open SFX audio stream by name
        /// </summary>
        /// <param name="sfxName">SFX name</param>
        /// <param name="bankIndex">Bank index</param>
        /// <param name="audioClipIndex">Audio clip index</param>
        /// <returns>GTA audio stream if successful, otherwise "null"</returns>
        public GTAAudioStream OpenSFXAudioStreamByName(string sfxName, uint bankIndex, uint audioClipIndex)
        {
            GTAAudioStream ret = null;
            if (sfxName != null)
            {
                string key = sfxName.Trim().ToLower();
                if (sfxFilesLookup.ContainsKey(key))
                {
                    ret = OpenSFXAudioStreamByID(sfxFilesLookup[key], bankIndex, audioClipIndex);
                }
            }
            return ret;
        }

        /// <summary>
        /// Open SFX audio stream by script event index
        /// </summary>
        /// <param name="eventIndex"></param>
        /// <returns>GTA audio stream if successful, otherwise "null"</returns>
        public GTAAudioStream OpenSFXAudioStreamByScriptEventIndex(uint eventIndex)
        {
            GTAAudioStream ret = null;
            if (eventIndex >= 2000U)
            {
                ret = OpenSFXAudioStreamByName("SCRIPT", (eventIndex - 2000U) / 200U, (eventIndex - 2000) % 200);
            }
            return ret;
        }

        /// <summary>
        /// Open streams audio stream by index
        /// </summary>
        /// <param name="streamsIndex">Streams index</param>
        /// <param name="bankIndex">Bank Index</param>
        /// <returns>GTA audio stream if successful, otherwise "null"</returns>
        public Stream OpenStreamsAudioStreamByID(uint streamsIndex, uint bankIndex)
        {
            Stream ret = null;
            if (streamsIndex < streamsAudioFiles.Length)
            {
                GTAAudioStreamsFile streams_file = streamsAudioFiles[streamsIndex];
                if (streams_file != null)
                {
                    ret = streams_file.Open(bankIndex);
                }
            }
            return ret;
        }

        /// <summary>
        /// Open streams audio stream by name
        /// </summary>
        /// <param name="streamsName">Streams name</param>
        /// <param name="bankIndex">Bank index</param>
        /// <returns>GTA audio stream</returns>
        public Stream OpenStreamsAudioStreamByName(string streamsName, uint bankIndex)
        {
            Stream ret = null;
            if (streamsName != null)
            {
                string key = streamsName.Trim().ToLower();
                if (streamsFilesLookup.ContainsKey(key))
                {
                    ret = OpenStreamsAudioStreamByID(streamsFilesLookup[key], bankIndex);
                }
            }
            return ret;
        }

        /// <summary>
        /// Get beats data by streams index
        /// </summary>
        /// <param name="streamsIndex">Streams index</param>
        /// <returns>Beats data</returns>
        public GTAAudioBeatData[] GetBeatsDataByStreamsIndex(uint streamsIndex)
        {
            GTAAudioBeatData[] ret = null;
            if (streamsIndex < streamsAudioFiles.Length)
            {
                GTAAudioStreamsFile streams_file = streamsAudioFiles[streamsIndex];
                if (streams_file != null)
                {
                    ret = streams_file.BeatsData.Clone() as GTAAudioBeatData[];
                }
            }
            if (ret == null)
            {
                ret = new GTAAudioBeatData[0];
            }
            return ret;
        }

        /// <summary>
        /// Get beats data by streams name
        /// </summary>
        /// <param name="streamsName">Streams name</param>
        /// <returns>Beats data</returns>
        public GTAAudioBeatData[] GetBeatsDataByStreamsName(string streamsName)
        {
            GTAAudioBeatData[] ret = null;
            if (streamsName != null)
            {
                string key = streamsName.Trim().ToLower();
                if (streamsFilesLookup.ContainsKey(key))
                {
                    ret = GetBeatsDataByStreamsIndex(streamsFilesLookup[streamsName]);
                }
            }
            if (ret == null)
            {
                ret = new GTAAudioBeatData[0];
            }
            return ret;
        }

        /// <summary>
        /// Get SFX audio file
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>SFX audio file if successful, otherwise "null"</returns>
        public GTAAudioSFXFile GetSFXAudioFile(uint index)
        {
            return ((sfxAudioFiles == null) ? null : ((index < sfxAudioFiles.Length) ? sfxAudioFiles[index] : null));
        }

        /// <summary>
        /// Get streams audio file
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Streams audio file if successful, otherwise "null"</returns>
        public GTAAudioStreamsFile GetStreamsAudioFile(uint index)
        {
            return ((streamsAudioFiles == null) ? null : ((index < streamsAudioFiles.Length) ? streamsAudioFiles[index] : null));
        }

        /// <summary>
        /// Get script event volume
        /// </summary>
        /// <param name="index">Script event index</param>
        /// <returns>Volume</returns>
        public sbyte GetScriptEventVolume(uint index)
        {
            return ((index < scriptEventVolume.Length) ? (sbyte)(scriptEventVolume[index]) : (sbyte)0);
        }

        /// <summary>
        /// Get SFX file index by name
        /// </summary>
        /// <param name="sfxName">SFX name</param>
        /// <returns>SFX index if successful, otherwise "-1"</returns>
        public int GetSFXFileIndexByName(string sfxName)
        {
            int ret = -1;
            string key = sfxName.Trim().ToLower();
            if (sfxFilesLookup.ContainsKey(key))
            {
                ret = (int)(sfxFilesLookup[key]);
            }
            return ret;
        }

        /// <summary>
        /// Get streams file index by name
        /// </summary>
        /// <param name="streamsName">Streams name</param>
        /// <returns>SFX index if successful, otherwise "-1"</returns>
        public int GetStreamsFileIndexByName(string streamsName)
        {
            int ret = -1;
            string key = streamsName.Trim().ToLower();
            if (streamsFilesLookup.ContainsKey(key))
            {
                ret = (int)(streamsFilesLookup[key]);
            }
            return ret;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (sfxAudioFiles != null)
            {
                foreach (GTAAudioSFXFile sfx_file in sfxAudioFiles)
                {
                    if (sfx_file != null)
                    {
                        sfx_file.Dispose();
                    }
                }
                sfxAudioFiles = null;
            }
            if (streamsAudioFiles != null)
            {
                foreach (GTAAudioStreamsFile streams_file in streamsAudioFiles)
                {
                    if (streams_file != null)
                    {
                        streams_file.Dispose();
                    }
                }
                streamsAudioFiles = null;
            }
        }
    }
}
