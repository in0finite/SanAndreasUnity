using UnityEngine;
using SanAndreasUnity.UI;

namespace SanAndreasUnity.Settings {

	public class CameraSettings : MonoBehaviour {
		
		OptionsWindow.FloatInput m_farClipPlaneInput = new OptionsWindow.FloatInput() {
			description = "Far clip plane",
			minValue = 100,
			maxValue = 5000,
			isAvailable = () => Camera.main != null,
			getValue = () => Camera.main.farClipPlane,
			setValue = (value) => { Camera.main.farClipPlane = value; },
		};
		OptionsWindow.FloatInput m_fieldOfViewInput = new OptionsWindow.FloatInput() {
			description = "Field of view",
			minValue = 20,
			maxValue = 120,
			isAvailable = () => Camera.main != null,
			getValue = () => Camera.main.fieldOfView,
			setValue = (value) => { Camera.main.fieldOfView = value; },
		};



		void Awake ()
		{
			OptionsWindow.RegisterInputs ("CAMERA", m_farClipPlaneInput, m_fieldOfViewInput);
		}

	}

}
