using SanAndreasUnity.Behaviours;
using SanAndreasUnity.UI;
using UnityEngine;

namespace SanAndreasUnity.Settings
{
    public class InputSettings : MonoBehaviour
    {
        OptionsWindow.FloatInput m_mouseSensitivityXInput = new OptionsWindow.FloatInput() {
            description = "Mouse sensitivity x",
            minValue = 0.2f,
            maxValue = 20f,
            isAvailable = () => GameManager.Instance != null,
            getValue = () => GameManager.Instance.cursorSensitivity.x,
            setValue = (value) => { GameManager.Instance.cursorSensitivity.x = value; },
            persistType = OptionsWindow.InputPersistType.OnStart,
        };
        OptionsWindow.FloatInput m_mouseSensitivityYInput = new OptionsWindow.FloatInput() {
            description = "Mouse sensitivity y",
            minValue = 0.2f,
            maxValue = 20f,
            isAvailable = () => GameManager.Instance != null,
            getValue = () => GameManager.Instance.cursorSensitivity.y,
            setValue = (value) => { GameManager.Instance.cursorSensitivity.y = value; },
            persistType = OptionsWindow.InputPersistType.OnStart,
        };
        OptionsWindow.BoolInput m_useTouchInput = new OptionsWindow.BoolInput ("Use touch input") {
            isAvailable = () => UIManager.Instance != null,
            getValue = () => UIManager.Instance.UseTouchInput,
            setValue = (value) => { UIManager.Instance.UseTouchInput = value; },
            persistType = OptionsWindow.InputPersistType.OnStart
        };

        private void Awake()
        {
            OptionsWindow.RegisterInputs ("INPUT",
                m_mouseSensitivityXInput,
                m_mouseSensitivityYInput,
                m_useTouchInput);
        }
    }
}
