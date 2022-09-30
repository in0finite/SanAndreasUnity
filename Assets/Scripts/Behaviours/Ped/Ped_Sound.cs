using UnityEngine;
using SanAndreasUnity.Behaviours.Audio;
using UGameCore.Utilities;
using System.Collections.Generic;

namespace SanAndreasUnity.Behaviours
{
    public partial class Ped
    {
        private AudioSource _mouthAudioSource;


        private void Sound_OnDisable()
        {
            if (_mouthAudioSource != null)
                _mouthAudioSource.Stop();
        }

        public void PlaySoundFromPedMouth(SoundId soundId)
        {
            if (!soundId.IsValid)
                return;

            this.AddAudioSourceForMouth();

            if (null == _mouthAudioSource)
                return;

            AudioClip audioClip = AudioManager.GetAudioClipCached(soundId);

            if (Net.NetStatus.IsServer)
                m_net_mouthSoundId = soundId;
            
            if (_mouthAudioSource.isPlaying)
                _mouthAudioSource.Stop();

            _mouthAudioSource.clip = audioClip;
            _mouthAudioSource.Play();
        }

        private void AddAudioSourceForMouth()
        {
            if (null != _mouthAudioSource)
                return;

            if (null == this.PlayerModel.Jaw)
                return;

            var go = Object.Instantiate(PedManager.Instance.pedMouthAudioSourcePrefab, this.PlayerModel.Jaw.transform);
            _mouthAudioSource = go.GetComponentOrLogError<AudioSource>();
        }
    }
}
