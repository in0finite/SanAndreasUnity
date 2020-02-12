using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        private int currentRadioStationIndex;
		private RadioStation CurrentRadioStation { get { return RadioStation.stations[currentRadioStationIndex]; } }
        private AudioSource m_radioAudioSource;

        public void PlayRadio()
        {
            m_radioAudioSource.Stop();

            // destroy current clip
            if (m_radioAudioSource.clip != null)
            {
                Destroy(m_radioAudioSource.clip);
                m_radioAudioSource.clip = null;
            }

            var clip = CurrentRadioStation.LoadCurrentClip();
            if (clip != null)
            {
                m_radioAudioSource.time = CurrentRadioStation.currentTime;
                m_radioAudioSource.clip = clip;
                m_radioAudioSource.Play();

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
                CurrentRadioStation.currentTime = m_radioAudioSource.time;
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
                m_radioAudioSource.Stop();
            }
            else
            {
                PlayRadio();
            }

        }

    }
}