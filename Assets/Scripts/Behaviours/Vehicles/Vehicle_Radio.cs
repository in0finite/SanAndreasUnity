using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        private int currentRadioStationIndex;
        //private RadioStation CurrentRadioStation { get { return RadioStation.stations[currentRadioStationIndex]; } }

        private AudioSource m_radioAudioSource;

        bool m_isPlayingRadio = false;
        bool m_radio_pedEnteredOrExitedVehicleLastFrame = false;
        bool m_isWaitingForNewSound = false;



        void Awake_Radio()
        {
            m_radioAudioSource = this.GetComponent<AudioSource>();
        }

        void Start_Radio()
        {
            currentRadioStationIndex = Random.Range(0, RadioStation.stations.Length);
        }

        void OnPedPreparedForVehicle_Radio(Ped ped, Seat seat)
        {
            m_radio_pedEnteredOrExitedVehicleLastFrame = true;
        }

        void Update_Radio()
        {

            bool pedEnteredOrExitedVehicleLastFrame = m_radio_pedEnteredOrExitedVehicleLastFrame;
            m_radio_pedEnteredOrExitedVehicleLastFrame = false;

            if (m_isPlayingRadio)
            {
                // check if we should stop playing radio sound

                // radio turned off => no sound
                if (currentRadioStationIndex < 0)
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

                if (m_isWaitingForNewSound)
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
                        RadioStation.stations[currentRadioStationIndex].NextClip();
                        this.StartPlayingRadio(true);
                        return;
                    }

                    // update current station time - this is needed in case vehicle gets destroyed - current time will not be updated
                    RadioStation.stations[currentRadioStationIndex].currentTime = m_radioAudioSource.time;
                }

            }
            else
            {
                // not playing radio sound
                // check if we should start playing
                // this can happen only if local ped entered vehicle

                if (pedEnteredOrExitedVehicleLastFrame)
                {
                    if (this.IsLocalPedInside())
                    {
                        // local ped is in vehicle

                        if (currentRadioStationIndex >= 0)
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
            m_isWaitingForNewSound = false;

            if (currentRadioStationIndex >= 0)
                RadioStation.stations[currentRadioStationIndex].currentTime = m_radioAudioSource.time;

            m_radioAudioSource.Stop();
        }

        void StartPlayingRadio(bool playImmediately)
        {
            if (m_isPlayingRadio)
                return;
            if (currentRadioStationIndex < 0)
                return;

            m_isPlayingRadio = true;
            m_isWaitingForNewSound = false;

            if (playImmediately)
                this.LoadAndPlayRadioSoundNow();
            else
                this.RequestNewRadioSound();

        }

        void RequestNewRadioSound()
        {
            this.CancelInvoke(nameof(this.LoadRadioSoundDelayed));
            m_isWaitingForNewSound = true;
            this.Invoke(nameof(this.LoadRadioSoundDelayed), 1.5f);
        }

        void LoadRadioSoundDelayed()
        {
            if (!m_isWaitingForNewSound)    // canceled in the meantime
                return;

            m_isWaitingForNewSound = false;

            if (!m_isPlayingRadio)
                return;
            if (currentRadioStationIndex < 0)
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

            var clip = RadioStation.stations[currentRadioStationIndex].LoadCurrentClip();
            if (clip != null)
            {
                m_radioAudioSource.clip = clip;
                m_radioAudioSource.time = RadioStation.stations[currentRadioStationIndex].currentTime;
                m_radioAudioSource.Play();

                Destroy(clip, clip.length);
            }

        }

        public void SwitchRadioStation(bool next)
        {
            int index = currentRadioStationIndex;

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

            if (currentRadioStationIndex == index)
                return;

            this.StopPlayingRadio();

            currentRadioStationIndex = index;

            if (this.IsLocalPedInside())
                this.StartPlayingRadio(false);
        }

    }
}