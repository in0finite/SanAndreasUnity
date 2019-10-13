using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio class
    /// </summary>
    public static class GTAAudio
    {
        /// <summary>
        /// Maximal sound entries
        /// </summary>
        private static readonly ushort maxSoundEntries = 400;

        /// <summary>
        /// Streams encoding secret
        /// </summary>
        internal static readonly byte[] streamsEncodingSecret = new byte[] { 0xEA, 0x3A, 0xC4, 0xA1, 0x9A, 0xA8, 0x14, 0xF3, 0x48, 0xB0, 0xD7, 0x23, 0x9D, 0xE8, 0xFF, 0xF1 };

        /// <summary>
        /// Get null terminated byte string length
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Number of characters in byte string</returns>
        private static int GetNullTerminatedByteStringLength(byte[] bytes)
        {
            int ret = 0;
            if (bytes != null)
            {
                ret = bytes.Length;
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] == 0)
                    {
                        ret = i;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Open GTA audio files
        /// </summary>
        /// <param name="directory">GTA audio files directory</param>
        /// <param name="filesMode">Audio files mode</param>
        /// <returns>GTA audio files</returns>
        public static GTAAudioFiles Open(string directory, EGTAAudioFilesMode filesMode)
        {
            GTAAudioFiles ret = null;
            try
            {
                if (directory != null)
                {
                    /*if (filesMode == EGTAAudioFilesMode.Create)
                    {
                        // TODO
                    }
                    else*/
                    if (Directory.Exists(directory))
                    {
                        string config_directory = Path.Combine(directory, "CONFIG");
                        string sfx_directory = Path.Combine(directory, "SFX");
                        string streams_directory = Path.Combine(directory, "streams");
                        if (Directory.Exists(config_directory) &&
                            Directory.Exists(sfx_directory) &&
                            Directory.Exists(streams_directory))
                        {
                            string audio_event_history_txt_path = Path.Combine(config_directory, "AudioEventHistory.txt");
                            string bank_lookup_dat_path = Path.Combine(config_directory, "BankLkup.dat");
                            string bank_slot_dat_path = Path.Combine(config_directory, "BankSlot.dat");
                            string event_volume_dat_path = Path.Combine(config_directory, "EventVol.dat");
                            string pak_files_dat_path = Path.Combine(config_directory, "PakFiles.dat");
                            string stream_paks_dat_path = Path.Combine(config_directory, "StrmPaks.dat");
                            string trak_lookup_dat_path = Path.Combine(config_directory, "TrakLkup.dat");
                            if (File.Exists(audio_event_history_txt_path) &&
                                File.Exists(bank_lookup_dat_path) &&
                                File.Exists(bank_slot_dat_path) &&
                                File.Exists(event_volume_dat_path) &&
                                File.Exists(pak_files_dat_path) &&
                                File.Exists(stream_paks_dat_path) &&
                                File.Exists(trak_lookup_dat_path))
                            {
                                string[] sfx_files = null;
                                string[] streams_files = null;
                                GTAAudioSFXFile[] sfx_audio_files = null;
                                GTAAudioStreamsFile[] streams_audio_files = null;
                                List<GTAAudioBankData>[] sfx_bank_data;
                                List<GTAAudioBankData>[] streams_bank_data;
                                Dictionary<string, uint> sfx_files_lookup = new Dictionary<string, uint>();
                                Dictionary<string, uint> streams_files_lookup = new Dictionary<string, uint>();
                                byte[] volume = null;
                                using (FileStream stream = File.Open(pak_files_dat_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    long stream_length = stream.Length;
                                    if ((stream_length % 52L) == 0L)
                                    {
                                        byte[] data = new byte[52];
                                        long count = stream_length / 52L;
                                        sfx_files = new string[count];
                                        sfx_audio_files = new GTAAudioSFXFile[count];
                                        for (int i = 0; i < sfx_files.Length; i++)
                                        {
                                            if (stream.Read(data, 0, data.Length) == data.Length)
                                            {
                                                int len = GetNullTerminatedByteStringLength(data);
                                                if (len > 0)
                                                {
                                                    string sfx_file = Encoding.UTF8.GetString(data, 0, len);
                                                    sfx_files[i] = sfx_file;
                                                    sfx_file = sfx_file.ToLower();
                                                    if (!(sfx_files_lookup.ContainsKey(sfx_file)))
                                                    {
                                                        sfx_files_lookup.Add(sfx_file, (uint)i);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                sfx_files = new string[0];
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (sfx_files == null)
                                {
                                    sfx_files = new string[0];
                                    sfx_audio_files = new GTAAudioSFXFile[0];
                                }
                                using (FileStream stream = File.Open(stream_paks_dat_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    long stream_length = stream.Length;
                                    if ((stream_length % 16L) == 0L)
                                    {
                                        byte[] data = new byte[16];
                                        long count = stream_length / 16L;
                                        streams_files = new string[count];
                                        streams_audio_files = new GTAAudioStreamsFile[count];
                                        for (int i = 0; i < streams_files.Length; i++)
                                        {
                                            if (stream.Read(data, 0, data.Length) == data.Length)
                                            {
                                                int len = GetNullTerminatedByteStringLength(data);
                                                if (len > 0)
                                                {
                                                    string streams_file = Encoding.UTF8.GetString(data, 0, len);
                                                    streams_files[i] = streams_file;
                                                    streams_file = streams_file.ToLower();
                                                    if (!(streams_files_lookup.ContainsKey(streams_file)))
                                                    {
                                                        streams_files_lookup.Add(streams_file, (uint)i);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                streams_files = new string[0];
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (streams_files == null)
                                {
                                    streams_files = new string[0];
                                    streams_audio_files = new GTAAudioStreamsFile[0];
                                }
                                sfx_bank_data = new List<GTAAudioBankData>[sfx_files.Length];
                                for (int i = 0; i < sfx_bank_data.Length; i++)
                                {
                                    sfx_bank_data[i] = new List<GTAAudioBankData>();
                                }
                                using (FileStream stream = File.Open(bank_lookup_dat_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    using (BinaryReader reader = new BinaryReader(stream))
                                    {
                                        long stream_length = stream.Length;
                                        if ((stream_length % 12L) == 0L)
                                        {
                                            long count = stream_length / 12L;
                                            for (long i = 0; i < count; i++)
                                            {
                                                byte index = reader.ReadByte();
                                                stream.Seek(3L, SeekOrigin.Current);
                                                uint offset = reader.ReadUInt32();
                                                uint length = reader.ReadUInt32();
                                                if (index < sfx_bank_data.Length)
                                                {
                                                    sfx_bank_data[index].Add(new GTAAudioBankData(offset, length));
                                                }
                                            }
                                        }
                                    }
                                }
                                streams_bank_data = new List<GTAAudioBankData>[streams_files.Length];
                                for (int i = 0; i < streams_bank_data.Length; i++)
                                {
                                    streams_bank_data[i] = new List<GTAAudioBankData>();
                                }
                                using (FileStream stream = File.Open(trak_lookup_dat_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    using (BinaryReader reader = new BinaryReader(stream))
                                    {
                                        long stream_length = stream.Length;
                                        if ((stream_length % 12L) == 0L)
                                        {
                                            long count = stream_length / 12L;
                                            for (long i = 0; i < count; i++)
                                            {
                                                byte index = reader.ReadByte();
                                                stream.Seek(3L, SeekOrigin.Current);
                                                uint offset = reader.ReadUInt32();
                                                uint length = reader.ReadUInt32();
                                                if (index < streams_bank_data.Length)
                                                {
                                                    streams_bank_data[index].Add(new GTAAudioBankData(offset, length));
                                                }
                                            }
                                        }
                                    }
                                }
                                for (int i = 0, j, k; i < sfx_files.Length; i++)
                                {
                                    string sfx_file = sfx_files[i];
                                    FileStream stream = null;
                                    if (sfx_file != null)
                                    {
                                        string sfx_path = Path.Combine(sfx_directory, sfx_file);
                                        if (File.Exists(sfx_path))
                                        {
                                            stream = File.Open(sfx_path, FileMode.Open, FileAccess.Read, FileShare.Read);
                                            if (stream != null)
                                            {
                                                BinaryReader reader = new BinaryReader(stream);
                                                if (reader != null)
                                                {
                                                    int len = sfx_bank_data[i].Count;
                                                    for (j = 0; j < len; j++)
                                                    {
                                                        GTAAudioBankData bank_data = sfx_bank_data[i][j];
                                                        GTAAudioAudioClipData[] audio_clip_data = null;
                                                        HashSet<uint> offset_set = new HashSet<uint>();
                                                        stream.Seek(bank_data.Offset, SeekOrigin.Begin);
                                                        ushort num_audio_clips = reader.ReadUInt16();
                                                        stream.Seek(sizeof(ushort), SeekOrigin.Current);
                                                        if (num_audio_clips <= maxSoundEntries)
                                                        {
                                                            audio_clip_data = new GTAAudioAudioClipData[num_audio_clips];
                                                            for (uint l = 0U; l != num_audio_clips; l++)
                                                            {
                                                                uint sound_buffer_offset = reader.ReadUInt32();
                                                                uint loop_offset = reader.ReadUInt32();
                                                                ushort sample_rate = reader.ReadUInt16();
                                                                ushort sound_headroom = reader.ReadUInt16();
                                                                audio_clip_data[l] = new GTAAudioAudioClipData(sound_buffer_offset, loop_offset, sample_rate, sound_headroom, 0U);
                                                                if (!(offset_set.Contains(sound_buffer_offset)))
                                                                {
                                                                    offset_set.Add(sound_buffer_offset);
                                                                }
                                                            }
                                                        }
                                                        List<uint> offsets = new List<uint>(offset_set);
                                                        offset_set.Clear();
                                                        offsets.Sort();
                                                        if (audio_clip_data == null)
                                                        {
                                                            audio_clip_data = new GTAAudioAudioClipData[0];
                                                        }
                                                        for (k = 0; k < audio_clip_data.Length; k++)
                                                        {
                                                            GTAAudioAudioClipData a_c_d = audio_clip_data[k];
                                                            int offset_index = offsets.IndexOf(a_c_d.SoundBufferOffset);
                                                            audio_clip_data[k] = new GTAAudioAudioClipData(a_c_d.SoundBufferOffset, a_c_d.LoopOffset, a_c_d.SampleRate, a_c_d.SoundHeadroom, ((offset_index < (offsets.Count - 1)) ? offsets[offset_index + 1] : bank_data.Length) - a_c_d.SoundBufferOffset);
                                                        }
                                                        offsets.Clear();
                                                        sfx_bank_data[i][j] = new GTAAudioBankData(bank_data.Offset, bank_data.Length, audio_clip_data);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    sfx_audio_files[i] = new GTAAudioSFXFile(sfx_file, stream, sfx_bank_data[i].ToArray());
                                    sfx_bank_data[i].Clear();
                                }
                                for (int i = 0; i < streams_files.Length; i++)
                                {
                                    string streams_file = streams_files[i];
                                    FileStream stream = null;
                                    GTAAudioBeatData[] beats_data = null;
                                    if (streams_file != null)
                                    {
                                        string streams_path = Path.Combine(streams_directory, streams_file);
                                        if (File.Exists(streams_path))
                                        {
                                            stream = File.Open(streams_path, FileMode.Open, FileAccess.Read, FileShare.Read);
                                            if (stream != null)
                                            {
                                                DecodingBinaryReader reader = new DecodingBinaryReader(stream);
                                                if (reader != null)
                                                {
                                                    List<GTAAudioBeatData> beats_data_list = new List<GTAAudioBeatData>();
                                                    for (int j = 0; j < 1000; j++)
                                                    {
                                                        uint timing = reader.ReadDecodeUInt32();
                                                        uint control = reader.ReadDecodeUInt32();
                                                        if (timing != uint.MaxValue)
                                                        {
                                                            beats_data_list.Add(new GTAAudioBeatData(timing, control));
                                                        }
                                                        else
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    stream.Seek(8064L, SeekOrigin.Begin);
                                                    uint magic_number = reader.ReadDecodeUInt32();
                                                    if (magic_number == 0xCDCD0001)
                                                    {
                                                        beats_data = beats_data_list.ToArray();
                                                    }
                                                    beats_data_list.Clear();
                                                }
                                            }
                                        }
                                    }
                                    if (beats_data == null)
                                    {
                                        beats_data = new GTAAudioBeatData[0];
                                    }
                                    streams_audio_files[i] = new GTAAudioStreamsFile(streams_file, stream, streams_bank_data[i].ToArray(), beats_data);
                                    streams_bank_data[i].Clear();
                                }
                                using (FileStream stream = File.Open(event_volume_dat_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    volume = new byte[stream.Length];
                                    if (stream.Read(volume, 0, volume.Length) != volume.Length)
                                    {
                                        volume = new byte[0];
                                    }
                                }
                                if (volume == null)
                                {
                                    volume = new byte[0];
                                }
                                ret = new GTAAudioFiles(sfx_audio_files, streams_audio_files, sfx_files_lookup, streams_files_lookup, volume);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (ret != null)
                {
                    ret.Dispose();
                    ret = null;
                }
                throw e;
            }
            return ret;
        }

        /// <summary>
        /// Open GTA audio files in read only mode
        /// </summary>
        /// <param name="directory">Audio files directory</param>
        /// <returns>GTA audio files</returns>
        public static GTAAudioFiles OpenRead(string directory)
        {
            return Open(directory, EGTAAudioFilesMode.Read);
        }
    }
}
