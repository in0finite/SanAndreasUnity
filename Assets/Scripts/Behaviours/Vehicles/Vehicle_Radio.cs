using SanAndreasUnity.Behaviours.Audio;
using SanAndreasUnity.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        private static readonly string[] radioStations = { "CH", "CO", "CR", "DS", "HC", "MH", "MR", "NJ", "RE", "RG", "TK" };
        private static readonly int radioStartPos = 3;
        private static readonly int radioEndPos = 114;

        public int currentRadioStation = 3;
        public int currentRadioStationPos = 3;

        private AudioSource radio;

        public void PlayRadio()
        {
            if (currentRadioStation == 0)
                return;
            if (null != radio)
                radio.Stop();
            radio = AudioManager.PlayStream(radioStations[currentRadioStation - 1], currentRadioStationPos);
        }

        public void RadioUpdate()
        {
            if (currentRadioStation == 0)
                return;
            if (null == radio || !radio.isPlaying)
            {
                currentRadioStationPos++;
                if (currentRadioStationPos > radioEndPos)
                    currentRadioStationPos = radioStartPos;
                PlayRadio();
            }
        }

        public void StopRadio()
        {
            if (null != radio)
                radio.Stop();
        }

        public void SwitchRadioStation(bool next)
        {
            if (next)
            {
                currentRadioStation++;
                if (currentRadioStation > radioStations.Length)
                    currentRadioStation = 0;
            }
            else
            {
                currentRadioStation--;
                if (currentRadioStation < 0)
                    currentRadioStation = radioStations.Length;
            }
            if (currentRadioStation == 0)
            {
                StopRadio();
            }
            else
            {
                currentRadioStationPos = Random.Range(radioStartPos, radioEndPos);
                PlayRadio();
            }
        }

    }
}