using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.UI;
using UnityEngine;

namespace SanAndreasUnity.Settings
{
    public class SoundSettings : MonoBehaviour
    {
        [Range(0f, 1f)] public float defaultSoundVolume = 0.5f;

        OptionsWindow.FloatInput m_soundVolume = new OptionsWindow.FloatInput ("Global sound volume", 0, 1) {
            getValue = () => AudioListener.volume,
            setValue = (value) => { AudioListener.volume = value; },
            persistType = OptionsWindow.InputPersistType.OnStart
        };

        OptionsWindow.FloatInput m_radioVolume = new OptionsWindow.FloatInput ("Radio volume", 0, 1) {
            isAvailable = () => VehicleManager.Instance != null,
            getValue = () => VehicleManager.Instance.radioVolume,
            setValue = ApplyRadioVolume,
            persistType = OptionsWindow.InputPersistType.OnStart,
        };

        private void Awake()
        {
            // apply default value
            AudioListener.volume = this.defaultSoundVolume;

            OptionsWindow.RegisterInputs(
                "SOUND",
                m_soundVolume,
                m_radioVolume);
        }

        private static void ApplyRadioVolume(float newValue)
        {
            VehicleManager.Instance.radioVolume = newValue;
            foreach (var vehicle in Vehicle.AllVehicles)
            {
                if (vehicle.RadioAudioSource != null)
                    vehicle.RadioAudioSource.volume = newValue;
            }
        }
    }
}
