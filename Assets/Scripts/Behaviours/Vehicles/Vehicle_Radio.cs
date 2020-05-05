using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        private int m_currentRadioStationIndex;
        public int CurrentRadioStationIndex => m_currentRadioStationIndex;

        private AudioSource m_radioAudioSource;
        public AudioSource RadioAudioSource => m_radioAudioSource;

        bool m_isPlayingRadio = false;
        public bool IsPlayingRadio => m_isPlayingRadio;

        bool m_radio_pedAssignedToVehicleLastFrame = false;

        bool m_isWaitingForNewRadioSound = false;
        public bool IsWaitingForNewRadioSound => m_isWaitingForNewRadioSound;



        void Awake_Radio()
        {
            m_radioAudioSource = this.GetComponent<AudioSource>();
        }

        void Start_Radio()
        {
            m_currentRadioStationIndex = Random.Range(0, RadioStation.stations.Length);
        }

        void OnPedPreparedForVehicle_Radio(Ped ped, Seat seat)
        {
            m_radio_pedAssignedToVehicleLastFrame = true;
        }

        void Update_Radio()
        {

            bool pedAssignedToVehicleLastFrame = m_radio_pedAssignedToVehicleLastFrame;
            m_radio_pedAssignedToVehicleLastFrame = false;

            if (m_isPlayingRadio)
            {
                // check if we should stop playing radio sound

                // radio turned off => no sound
                if (m_currentRadioStationIndex < 0)
                {
                    this.StopPlayingRadio();
                    return;
                }

                // no local ped inside => no sound
                if (!this.IsLocalPedInside())
                {
                    this.StopPlayingRadio();
                    return;
                }

                // we should continue playing sound

                if (m_isWaitingForNewRadioSound)
                {
                    // we are waiting for sound to load
                    // don't do anything
                }
                else
                {
                    // check if sound finished playing
                    if (!m_radioAudioSource.isPlaying)
                    {
                        // sound finished playing
                        // switch to next stream in a current radio station
                        this.StopPlayingRadio();
                        RadioStation.stations[m_currentRadioStationIndex].NextClip();
                        this.StartPlayingRadio(true);
                        return;
                    }

                    // update current station time - this is needed in case vehicle gets destroyed - current time will not be updated
                    RadioStation.stations[m_currentRadioStationIndex].currentTime = m_radioAudioSource.time;
                }

            }
            else
            {
                // not playing radio sound
                // check if we should start playing
                // this can happen only if local ped entered vehicle

                if (pedAssignedToVehicleLastFrame)
                {
                    if (this.IsLocalPedInside())
                    {
                        // local ped is in vehicle

                        if (m_currentRadioStationIndex >= 0)
                        {
                            // start playing sound
                            this.StartPlayingRadio(false);
                            return;
                        }
                    }
                }

                // continue not playing radio sound

            }

        }

        void StopPlayingRadio()
        {
            if (!m_isPlayingRadio)
                return;

            m_isPlayingRadio = false;
            m_isWaitingForNewRadioSound = false;

            if (m_currentRadioStationIndex >= 0)
                RadioStation.stations[m_currentRadioStationIndex].currentTime = m_radioAudioSource.time;

            m_radioAudioSource.Stop();
        }

        void StartPlayingRadio(bool playImmediately)
        {
            if (m_isPlayingRadio)
                return;
            if (m_currentRadioStationIndex < 0)
                return;

            m_isPlayingRadio = true;
            m_isWaitingForNewRadioSound = false;

            if (playImmediately)
                this.LoadAndPlayRadioSoundNow();
            else
                this.RequestNewRadioSound();

        }

        void RequestNewRadioSound()
        {
            this.CancelInvoke(nameof(this.LoadRadioSoundDelayed));
            m_isWaitingForNewRadioSound = true;
            this.Invoke(nameof(this.LoadRadioSoundDelayed), 1.0f);
        }

        void LoadRadioSoundDelayed()
        {
            if (!m_isWaitingForNewRadioSound)    // canceled in the meantime
                return;

            m_isWaitingForNewRadioSound = false;

            if (!m_isPlayingRadio)
                return;
            if (m_currentRadioStationIndex < 0)
                return;

            this.LoadAndPlayRadioSoundNow();
        }

        void LoadAndPlayRadioSoundNow()
        {

            m_radioAudioSource.Stop();  // just in case

            // destroy current clip
            if (m_radioAudioSource.clip != null)
            {
                Destroy(m_radioAudioSource.clip);
                m_radioAudioSource.clip = null;
            }

            var clip = RadioStation.stations[m_currentRadioStationIndex].LoadCurrentClip();
            if (clip != null)
            {
                m_radioAudioSource.clip = clip;
                m_radioAudioSource.time = RadioStation.stations[m_currentRadioStationIndex].currentTime;
                m_radioAudioSource.Play();

                Destroy(clip, clip.length);
            }

        }

        public void SwitchRadioStation(bool next)
        {
            int index = m_currentRadioStationIndex;

            if (next)
            {
                index++;
            }
            else
            {
                index--;
                if (index < -1)
                    index = RadioStation.stations.Length - 1;
            }

            this.SwitchRadioStation(index);
        }

        public void SwitchRadioStation(int index)
        {
            if (index < -1 || index >= RadioStation.stations.Length)
                index = -1;

            if (m_currentRadioStationIndex == index)
                return;

            this.StopPlayingRadio();

            m_currentRadioStationIndex = index;

            if (this.IsLocalPedInside())
                this.StartPlayingRadio(false);
        }

    }
}