using UnityEngine;
using SanAndreasUnity.Behaviours.Audio;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;

namespace SanAndreasUnity.Behaviours
{
    public partial class Ped
    {
        private AudioSource _mouthAudioSource;


        private void PlaySoundFromPedMouth(SoundId soundId)
        {
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
