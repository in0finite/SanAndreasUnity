using SanAndreasUnity.UI;
using UnityEngine;

namespace SanAndreasUnity.Settings
{
    public class SoundSettings : MonoBehaviour
    {
        [Range(0f, 1f)] public float defaultSoundVolume = 0.5f;

        OptionsWindow.FloatInput m_soundVolume = new OptionsWindow.FloatInput ("Sound volume", 0, 1) {
            getValue = () => AudioListener.volume,
            setValue = (value) => { AudioListener.volume = value; },
            persistType = OptionsWindow.InputPersistType.OnStart
        };

        private void Awake()
        {
            // apply default value
            AudioListener.volume = this.defaultSoundVolume;

            OptionsWindow.RegisterInputs ("SOUND", m_soundVolume);
        }
    }
}
