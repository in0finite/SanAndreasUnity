using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        private int currentRadioStationIndex;
        private RadioStation CurrentRadioStation { get { return RadioStation.stations[currentRadioStationIndex]; } }

        private AudioSource radio;

        public void PlayRadio()
        {
            radio.Stop();
            var clip = CurrentRadioStation.CurrentClip;
            if (clip != null)
            {
                radio.time = CurrentRadioStation.currentTime;
                radio.clip = clip;
                radio.Play();

                Destroy(clip, clip.length);
            }
        }

        private void ContinueRadio()
        {
            CurrentRadioStation.NextClip();
            PlayRadio();
        }

        public void SwitchRadioStation(bool next)
        {
            if (currentRadioStationIndex != -1)
            {
                CurrentRadioStation.currentTime = radio.time;
            }
            if (next)
            {
                currentRadioStationIndex++;
                if (currentRadioStationIndex >= RadioStation.stations.Length)
                    currentRadioStationIndex = -1;
            }
            else
            {
                currentRadioStationIndex--;
                if (currentRadioStationIndex < -1)
                    currentRadioStationIndex = RadioStation.stations.Length - 1;
            }
            if (currentRadioStationIndex == -1)
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