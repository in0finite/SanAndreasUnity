using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Audio
{
    public class AudioLookupFiles
    {
        public static List<BankLookUp> bankLookUps;
        public static BankSlot[] bankSlots = new BankSlot[25];
        public static List<TrakLookUp> trakLookUps;
        // Use this for initialization
        public static void ReadAllAudioLookupFiles()
        {
            var Gpath = Config.GetGameDir;
            ReadFile(Gpath + "/Audio/CONFIG/TrakLkup.dat");
            ReadFile(Gpath + "/Audio/CONFIG/BankLkup.dat");
            ReadFile(Gpath + "/Audio/CONFIG/BankSlot.dat");
        }

        private static void ReadFile(string path)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(path));
            if (Path.GetFileNameWithoutExtension(path) == "TrakLkup")
                LoadTrakLkup(br);
            else if (Path.GetFileNameWithoutExtension(path) == "BankLkup")
                LoadBankLkup(br);
            else if (Path.GetFileNameWithoutExtension(path) == "BankSlot")
                LoadBankSlots(br);
            else
                Debug.LogWarning("What is that? Are u blind only files with name XXXXLkup allowed... Or did u even look at this code Validation? If so ur dumb :p ");
        }

        private static void LoadBankLkup(BinaryReader br)
        {
            bankLookUps = new List<BankLookUp>();
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                BankLookUp Blu = new BankLookUp();
                Blu.PakIndex = Convert.ToInt32(br.ReadByte());
                Blu._padding = br.ReadBytes(3);
                Blu.BankOffset = br.ReadInt32();
                Blu.BankSize = br.ReadInt32();
                bankLookUps.Add(Blu);
            }
        }

        private static void LoadBankSlots(BinaryReader br)
        {
            bankSlots = new BankSlot[br.ReadInt16()];
            int i = 0;
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                BankSlot Bls = new BankSlot();
                Bls.Sum = br.ReadInt32();
                Bls.BufferSize = br.ReadInt32();
                Bls.Unkown1 = br.ReadInt32();
                Bls.Unkown2 = br.ReadInt32();
                Bls.Bytes = br.ReadBytes(4808);
                bankSlots[i] = Bls;
                i++;
            }
        }

        public static List<TrakLookUp> GetTrakLookUps(int pakIndex)
        {
            List<TrakLookUp> tlus = new List<TrakLookUp>();
            for (int i = 0; i < trakLookUps.Count; i++)
            {
                if (trakLookUps[i].PakIndex == pakIndex)
                {
                    tlus.Add(trakLookUps[i]);
                }
            }
            return tlus;
        }

        public static List<BankLookUp> GetBankLookUps(int pakIndex)
        {
            List<BankLookUp> blus = new List<BankLookUp>();
            for (int i = 0; i < bankLookUps.Count; i++)
            {
                if (bankLookUps[i].PakIndex == pakIndex)
                {
                    blus.Add(bankLookUps[i]);
                }
            }
            return blus;
        }

        private static void LoadTrakLkup(BinaryReader br)
        {
            trakLookUps = new List<TrakLookUp>();
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                TrakLookUp Tlu = new TrakLookUp();
                Tlu.PakIndex = Convert.ToInt32(br.ReadByte());
                Tlu._padding = br.ReadBytes(3);
                Tlu.TrackOffset = br.ReadInt32();
                Tlu.TrackLenght = br.ReadInt32();
                trakLookUps.Add(Tlu);
            }
        }
    }

    public enum ConfigType
    {
        BANKLKUP,
        TRACKLKUP,
        BANKSLOT
    }

    public struct BankLookUp
    {
        public int PakIndex;
        public byte[] _padding;
        public int BankOffset;
        public int BankSize;
    }

    public struct BankSlot
    {
        public int Sum;
        public int BufferSize;
        public int Unkown1;
        public int Unkown2;
        public byte[] Bytes;
    }

    public struct TrakLookUp
    {
        public int PakIndex;
        public byte[] _padding;
        public int TrackOffset;
        public int TrackLenght;
    }
}