using UnityEngine;
using SanAndreasUnity.UI;

namespace SanAndreasUnity.Settings {

	public class CameraSettings : MonoBehaviour {
		
		static float s_farClipPlane = 1000;
		static float s_fieldOfView = 60;

		OptionsWindow.FloatInput m_farClipPlaneInput = new OptionsWindow.FloatInput() {
			description = "Far clip plane",
			minValue = 100,
			maxValue = 5000,
			getValue = () => Camera.main != null ? Camera.main.farClipPlane : s_farClipPlane,
			setValue = (value) => { s_farClipPlane = value; if (Camera.main != null) Camera.main.farClipPlane = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};
		OptionsWindow.FloatInput m_fieldOfViewInput = new OptionsWindow.FloatInput() {
			description = "Field of view",
			minValue = 20,
			maxValue = 120,
			getValue = () => Camera.main != null ? Camera.main.fieldOfView : s_fieldOfView,
			setValue = (value) => { s_fieldOfView = value; if (Camera.main != null) Camera.main.fieldOfView = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};



		void Awake ()
		{
			OptionsWindow.RegisterInputs ("CAMERA", m_farClipPlaneInput, m_fieldOfViewInput);
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (s1, s2) => OnActiveSceneChanged();
		}

		void OnActiveSceneChanged()
		{
			// apply settings

			Camera cam = Utilities.F.FindMainCameraEvenIfDisabled();
			if (cam != null)
			{
				cam.farClipPlane = s_farClipPlane;
				cam.fieldOfView = s_fieldOfView;
			}
			
		}

	}

}
