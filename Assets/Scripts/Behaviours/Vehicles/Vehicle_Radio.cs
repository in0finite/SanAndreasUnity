using System.Collections;
using System.Collections.Generic;
using UGameCore.Utilities;
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

        public double TimeWhenRadioStationChanged { get; private set; } = double.NegativeInfinity;
        public double TimeSinceRadioStationChanged => Time.timeAsDouble - this.TimeWhenRadioStationChanged;



        void Awake_Radio()
        {
            m_radioAudioSource = this.GetComponentOrThrow<AudioSource>();
            m_radioAudioSource.volume = VehicleManager.Instance.radioVolume;
            m_currentRadioStationIndex = Random.Range(0, RadioStation.stations.Length);
        }

        void OnDisable_Radio()
        {
            // need to destroy audio clip here, because otherwise it will stay in memory
            this.StopPlayingRadio();
        }

        void Start_Radio()
        {
            
        }

        void OnPedAssignedToVehicle_Radio(Ped ped, Seat seat)
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

                    // update current station time - this is needed in case vehicle gets destroyed - current time will not be updated - or maybe it will ?
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

            // save current station's time
            if (m_currentRadioStationIndex >= 0 && m_radioAudioSource.isPlaying)
                RadioStation.stations[m_currentRadioStationIndex].currentTime = m_radioAudioSource.time;

            m_radioAudioSource.Stop();

            this.DestroyCurrentRadioClip();
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

            this.DestroyCurrentRadioClip();

            var clip = RadioStation.stations[m_currentRadioStationIndex].LoadCurrentClip();
            if (clip != null)
            {
                m_radioAudioSource.clip = clip;
                m_radioAudioSource.time = RadioStation.stations[m_currentRadioStationIndex].currentTime;
                m_radioAudioSource.Play();

                // this shouldn't be done because if the audio source is stopped and started again (eg. when ped exits the vehicle), 
                // the clip may get destroyed before the audio source finishes playing
                //Destroy(clip, clip.length);
            }

        }

        void DestroyCurrentRadioClip()
        {
            if (m_radioAudioSource.clip != null)
            {
                Destroy(m_radioAudioSource.clip);
                m_radioAudioSource.clip = null;
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

            this.TimeWhenRadioStationChanged = Time.timeAsDouble;

            this.StopPlayingRadio();

            m_currentRadioStationIndex = index;

            if (this.IsLocalPedInside())
                this.StartPlayingRadio(false);
        }

    }
}