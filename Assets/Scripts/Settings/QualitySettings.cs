using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.UI;
using SanAndreasUnity.Utilities;
using Quality = UnityEngine.QualitySettings;

namespace SanAndreasUnity.Settings {

	public class QualitySettings : MonoBehaviour {

		string[] qualitySettingsNames;
		OptionsWindow.FloatInput m_fpsInput = new OptionsWindow.FloatInput( "Max fps", 0f, 200f );


		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

			this.qualitySettingsNames = Quality.names;
		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nQUALITY\n");


			m_fpsInput.value = Behaviours.GameManager.GetMaxFps ();
			if (OptionsWindow.FloatSlider (m_fpsInput)) {
				Behaviours.GameManager.SetMaxFps (m_fpsInput.value.RoundToInt ());
			}

			Quality.antiAliasing = UI.OptionsWindow.MultipleOptions( Quality.antiAliasing,
				"Anti aliasing", 0, 2, 4);

			string newLevel = UI.OptionsWindow.MultipleOptions( this.qualitySettingsNames[Quality.GetQualityLevel()],
				"Quality level", this.qualitySettingsNames);
			int newLevelIndex = System.Array.FindIndex (this.qualitySettingsNames, n => n == newLevel);
			if (Quality.GetQualityLevel () != newLevelIndex) {
				Quality.SetQualityLevel (newLevelIndex);
			}

			Quality.shadowDistance = UI.OptionsWindow.FloatSlider (Quality.shadowDistance,
				0, 200, "Shadow distance");


		}

	}

}
