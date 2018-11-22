using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Audio
{
    public class AudioSFX
    {
        private static Dictionary<string, BinaryReader> binaryReaders = new Dictionary<string, BinaryReader>();
        // Use this for initialization
        public static void LoadAll()
        {
            binaryReaders["FEET"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/FEET"));
            binaryReaders["GENRL"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/GENRL"));
            binaryReaders["PAIN_A"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/PAIN_A"));
            binaryReaders["SCRIPT"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/SCRIPT"));
            binaryReaders["SPC_EA"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/SPC_EA"));
            binaryReaders["SPC_FA"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/SPC_FA"));
            binaryReaders["SPC_GA"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/SPC_GA"));
            binaryReaders["SPC_NA"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/SPC_NA"));
            binaryReaders["SPC_PA"] = new BinaryReader(File.OpenRead(Config.GetGameDir + "/Audio/SFX/SPC_PA"));
        }

        public static AudioClip GetAudioClip(string filename, int bank, int sound)
        {
            var rawclip = GetRaw(filename, bank, sound);
            var floats = rawclip.DecodedFloats;
            AudioClip aclip = AudioClip.Create(filename + "/" + bank + "/" + sound, rawclip.SampleRate * 2, 1, rawclip.SampleRate, false);
            aclip.SetData(floats, 0);
            return aclip;
        }

        private static RawClip GetRaw(string filename, int bank, int sound)
        {
            var br = binaryReaders[filename];
            int pakIndex = GetLookupIndex(filename);
            List<BankLookUp> lookups = AudioLookupFiles.GetBankLookUps(pakIndex);
            int numofBanks = lookups.Count;
            br.BaseStream.Position = lookups[bank].BankOffset;
            bank_header bank_Header = new bank_header();
            bank_Header.num_sounds = (int)br.ReadUInt32();
            for (int i = 0; i < 400; i++)
            {
                bank_Header.sounds[i].offset = br.ReadInt32();
                bank_Header.sounds[i].unknown_32 = br.ReadInt32();
                bank_Header.sounds[i].sample_rate = br.ReadUInt16();
                bank_Header.sounds[i].unknown_16 = br.ReadUInt16();
            }
            br.BaseStream.Position = bank_Header.sounds[sound].offset + 4804;
            int lenght = 0;
            if (sound == (bank_Header.num_sounds - 1))
                if (bank == numofBanks - 1)
                    lenght = (int)(br.BaseStream.Length - br.BaseStream.Position);
                else
                    lenght = lookups[bank + 1].BankOffset - (int)br.BaseStream.Position;
            else
                lenght = bank_Header.sounds[sound + 1].offset - bank_Header.sounds[sound].offset;
            byte[] bytes = br.ReadBytes(lenght);

            return new RawClip(bank_Header.sounds[sound].sample_rate, bytes);
        }

        private static int GetLookupIndex(string filename)
        {
            // Just to be extra safe, I will explicitly allcaps this here even though
            // my SAAT code will allcaps the name before calling this function.
            if (filename == "FEET")
                return (0);
            if (filename == "GENRL")
                return (1);
            if (filename == "PAIN_A")
                return (2);
            if (filename == "SCRIPT")
                return (3);
            if (filename == "SPC_EA")
                return (4);
            if (filename == "SPC_FA")
                return (5);
            if (filename == "SPC_GA")
                return (6);
            if (filename == "SPC_NA")
                return (7);
            if (filename == "SPC_PA")
                return (8);
            // default value; no match
            return -1;
        }

        public struct sound_entry
        {
            public int offset;
            public int unknown_32;
            public int sample_rate;
            public int unknown_16;
        }

        public class bank_header
        {
            const int BH_NUM_SOUND_ENTRIES = 400;
            public int num_sounds;
            public sound_entry[] sounds = new sound_entry[BH_NUM_SOUND_ENTRIES];
        }
    }
}