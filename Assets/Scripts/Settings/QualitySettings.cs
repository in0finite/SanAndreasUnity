using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.UI;
using SanAndreasUnity.Utilities;
using Quality = UnityEngine.QualitySettings;

namespace SanAndreasUnity.Settings {

	public class QualitySettings : MonoBehaviour {

		string[] qualitySettingsNames;

		OptionsWindow.FloatInput m_fpsInput = new OptionsWindow.FloatInput( "Max fps", 0f, 200f ) {
			getValue = () => Behaviours.GameManager.GetMaxFps (),
			setValue = (value) => { Behaviours.GameManager.SetMaxFps (value.RoundToInt ()); }
		};
		OptionsWindow.FloatInput m_shadowDistanceInput = new OptionsWindow.FloatInput( "Shadow distance", 0f, 200f ) {
			getValue = () => Quality.shadowDistance,
			setValue = (value) => { Quality.shadowDistance = value; }
		};
		OptionsWindow.EnumInput<ShadowProjection> m_shadowProjectionInput = new OptionsWindow.EnumInput<ShadowProjection>() {
			description = "Shadow projection",
			getValue = () => Quality.shadowProjection,
			setValue = (value) => { Quality.shadowProjection = value; }
		};
		OptionsWindow.EnumInput<ShadowResolution> m_shadowResolutionInput = new OptionsWindow.EnumInput<ShadowResolution>() {
			description = "Shadow resolution",
			getValue = () => Quality.shadowResolution,
			setValue = (value) => { Quality.shadowResolution = value; }
		};
		OptionsWindow.EnumInput<ShadowQuality> m_shadowQualityInput = new OptionsWindow.EnumInput<ShadowQuality>() {
			description = "Shadow quality",
			getValue = () => Quality.shadows,
			setValue = (value) => { Quality.shadows = value; }
		};



		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

			this.qualitySettingsNames = Quality.names;
		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nQUALITY\n");


			OptionsWindow.Input (m_fpsInput);

			Quality.antiAliasing = UI.OptionsWindow.MultipleOptions( Quality.antiAliasing,
				"Anti aliasing", 0, 2, 4);

			string newLevel = UI.OptionsWindow.MultipleOptions( this.qualitySettingsNames[Quality.GetQualityLevel()],
				"Quality level", this.qualitySettingsNames);
			int newLevelIndex = System.Array.FindIndex (this.qualitySettingsNames, n => n == newLevel);
			if (Quality.GetQualityLevel () != newLevelIndex) {
				Quality.SetQualityLevel (newLevelIndex);
			}

			OptionsWindow.Input (m_shadowDistanceInput);

//			Quality.shadowCascades;
			OptionsWindow.Input (m_shadowProjectionInput);
			OptionsWindow.Input (m_shadowResolutionInput);
			OptionsWindow.Input (m_shadowQualityInput);

		}

	}

}
