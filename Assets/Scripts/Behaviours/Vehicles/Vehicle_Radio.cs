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

        private void PlayRadio()
        {
            radio.Stop();
            var clip = AudioManager.CreateAudioClipFromStream(radioStations[currentRadioStation - 1], currentRadioStationPos);
            if (clip != null)
            {
                radio.time = 0.0f;
                radio.clip = clip;
                radio.Play();

                Destroy(clip, clip.length);
            }
        }

        private void ContinueRadio()
        {
            currentRadioStationPos++;
            if (currentRadioStationPos > radioEndPos)
                currentRadioStationPos = radioStartPos;
            PlayRadio();
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
                radio.Stop();
            }
            else
            {
                PlayRadio();
            }
        }

    }
}