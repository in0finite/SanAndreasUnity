using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TanjentOGG;
using System;
using SanUnityAPI;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Audio
{
    public class AudioStream
    {
        private static Dictionary<string, BinaryReader> binaryReaders = new Dictionary<string, BinaryReader>();
        // Use this for initialization
        public static void LoadAll()
        {
            binaryReaders["AA"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/AA"));
            binaryReaders["ADVERTS"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/ADVERTS"));
            binaryReaders["AMBIENCE"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/AMBIENCE"));
            binaryReaders["BEATS"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/BEATS"));
            binaryReaders["CH"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/CH"));
            binaryReaders["CO"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/CO"));
            binaryReaders["CR"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/CR"));
            binaryReaders["CUTSCENE"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/CUTSCENE"));
            binaryReaders["DS"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/DS"));
            binaryReaders["HC"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/HC"));
            binaryReaders["MH"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/MH"));
            binaryReaders["MR"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/MR"));
            binaryReaders["NJ"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/NJ"));
            binaryReaders["RE"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/RE"));
            binaryReaders["RG"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/RG"));
            binaryReaders["TK"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/streams/TK"));
        }

        //Gets a clip at the given index
        public static AudioClip GetAudioClip(string filename, int index)
        {
            Ogg ogg = GetOgg(filename, index);
            AudioClip aclip = AudioClip.Create(filename+"/"+ index, ogg.DecodedFloats.Length - ogg.SampleRate, ogg.Channels, ogg.SampleRate, false);
            var arrayfloats = ogg.DecodedFloats;
            aclip.SetData(arrayfloats, 0);
            //Decodes the given byts to produce the needed datas
            //If decoded the same time the stream is loaded... Takes a lot of time... also dosent work on backgroung......
            return aclip;
        }


        public static beat_entry[] GetBeatEntrys(string filename, int index)
        {
            var br = binaryReaders[filename];
            TrakLookUp lookup = AudioLookupFiles.GetTrakLookUps(GetLookupIndex(filename))[index];
            br.BaseStream.Position = lookup.TrackOffset;
            beat_entry[] beat_Entries = new beat_entry[1000];
            for (int i = 0; i < 1000; i++)
            {
                beat_entry beat_Entry = new beat_entry();
                beat_Entry.control = BitConverter.ToInt32(Stream_Decode(br.ReadBytes(4), 4, (int)br.BaseStream.Position - 4), 0);
                beat_Entry.timing = BitConverter.ToInt32(Stream_Decode(br.ReadBytes(4), 4, (int)br.BaseStream.Position - 4), 0);
                beat_Entries[i] = beat_Entry;
            }
            return beat_Entries;
        }

        private static Ogg GetOgg(string filename, int index)
        {
            var br = binaryReaders[filename];
            TrakLookUp lookup = AudioLookupFiles.GetTrakLookUps(GetLookupIndex(filename))[index];
            br.BaseStream.Position = lookup.TrackOffset;
            track_header track_Header = new track_header();
            br.BaseStream.Position += 8000;
            track_Header.lengths[0].length = BitConverter.ToInt32(Stream_Decode(br.ReadBytes(4), 4, (int)br.BaseStream.Position - 4), 0);
            int lenght = 0;
            if (index == 0)
            {
                lenght = track_Header.lengths[0].length;
            }
            else
            {
                lenght = lookup.TrackLenght;
            }
            br.BaseStream.Position += 64;
            byte[] bytes = br.ReadBytes(lenght);
            bytes = Stream_Decode(bytes, lenght, (int)br.BaseStream.Position - lenght);
            //Assignes the bytes to a oggfile.......
            File.WriteAllBytes("D:/apps/NFH2andMORE/Grand Theft Auto San Andreas/Audio/streams/test.ogg", bytes);
            return new Ogg(bytes);
        }

        public static int GetLookupIndex(string filename)
        {
            // Just to be extra safe, I will explicitly allcaps this here even though
            // my SAAT code will allcaps the name before calling this function.
            if (filename == "AA")
                return (0);
            if (filename == "ADVERTS")
                return (1);
            if (filename == "AMBIENCE")
                return (3);
            if (filename == "BEATS")
                return (4);
            if (filename == "CH")
                return (5);
            if (filename == "CO")
                return (6);
            if (filename == "CR")
                return (7);
            if (filename == "CUTSCENE")
                return (8);
            if (filename == "DS")
                return (9);
            if (filename == "HC")
                return (10);
            if (filename == "MH")
                return (11);
            if (filename == "MR")
                return (12);
            if (filename == "NJ")
                return (13);
            if (filename == "RE")
                return (14);
            if (filename == "RG")
                return (15);
            if (filename == "TK")
                return (16);
            // default value; no match
            return -1;
        }

        //Decodes all bytes with the encode Key.... 
        private static byte[] Stream_Decode(byte[] bytes, int size, int offset)
        {
            int encodeIndex = offset;
            if (offset > 15)
                encodeIndex = (((offset / 16) % 16) != 0) ? offset - ((int)(offset / 16d) * 16) : 0;
            Debug.Log(encodeIndex);
            byte[] encode_key = { 0xea, 0x3a, 0xc4, 0xa1, 0x9a, 0xa8, 0x14, 0xf3, 0x48, 0xb0, 0xd7, 0x23, 0x9d, 0xe8, 0xff, 0xf1 };
            for (int i = 0; i < size; ++i)
            {
                bytes[i] ^= encode_key[encodeIndex];
                encodeIndex = (encodeIndex + 1) % 16;
            }
            return bytes;
        }
    }

    public struct beat_entry
    {
        public int timing;
        public int control;
    }

    public struct length_entry
    {
        public int length;
        public int extra;
    }

    public class track_header
    {
        const int TH_NUM_BEAT_ENTRIES = 1000;
        const int TH_NUM_LENGTH_ENTRIES = 8;
        public beat_entry[] beats = new beat_entry[TH_NUM_BEAT_ENTRIES];
        public length_entry[] lengths = new length_entry[TH_NUM_LENGTH_ENTRIES];
        public int trailer;
    }
}