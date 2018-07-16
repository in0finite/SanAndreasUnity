using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Quality = UnityEngine.QualitySettings;

namespace SanAndreasUnity.Settings {

	public class QualitySettings : MonoBehaviour {

		string[] qualitySettingsNames;


		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

			this.qualitySettingsNames = Quality.names;
		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nQUALITY\n");


			Quality.antiAliasing = UI.OptionsWindow.MultipleOptions( Quality.antiAliasing,
				"Anti aliasing", 0, 1, 2, 4);

			string newLevel = UI.OptionsWindow.MultipleOptions( this.qualitySettingsNames[Quality.GetQualityLevel()],
				"Quality level", this.qualitySettingsNames);
			int newLevelIndex = System.Array.FindIndex (this.qualitySettingsNames, n => n == newLevel);
			if (Quality.GetQualityLevel () != newLevelIndex) {
				Quality.SetQualityLevel (newLevelIndex);
			}

			Quality.shadowDistance = UI.OptionsWindow.FloatSlider (Quality.shadowDistance,
				0, 500, "Shadow distance");


		}

	}

}
