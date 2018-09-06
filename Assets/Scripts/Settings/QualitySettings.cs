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
		OptionsWindow.MultipleOptionsInput<int> m_antiAliasingInput = new OptionsWindow.MultipleOptionsInput<int>() {
			description = "Anti aliasing",
			getValue = () => Quality.antiAliasing,
			setValue = (value) => { Quality.antiAliasing = value; },
			Options = new int[]{0, 2, 4}
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
			description = "Shadows",
			getValue = () => Quality.shadows,
			setValue = (value) => { Quality.shadows = value; }
		};
		OptionsWindow.MultipleOptionsInput<int> m_shadowCascadesInput = new OptionsWindow.MultipleOptionsInput<int>() {
			description = "Num shadow cascades",
			getValue = () => Quality.shadowCascades,
			setValue = (value) => { Quality.shadowCascades = value; },
			Options = new int[]{1, 2, 4}
		};
		OptionsWindow.EnumInput<AnisotropicFiltering> m_anisotropicFilteringInput = new OptionsWindow.EnumInput<AnisotropicFiltering>() {
			description = "Anisotropic filtering",
			getValue = () => Quality.anisotropicFiltering,
			setValue = (value) => { Quality.anisotropicFiltering = value; }
		};



		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

			this.qualitySettingsNames = Quality.names;
		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nQUALITY\n");


			OptionsWindow.DisplayInput (m_fpsInput);

			OptionsWindow.DisplayInput (m_antiAliasingInput);

			string newLevel = UI.OptionsWindow.MultipleOptions( this.qualitySettingsNames[Quality.GetQualityLevel()],
				"Quality level", this.qualitySettingsNames);
			int newLevelIndex = System.Array.FindIndex (this.qualitySettingsNames, n => n == newLevel);
			if (Quality.GetQualityLevel () != newLevelIndex) {
				Quality.SetQualityLevel (newLevelIndex);
			}

			OptionsWindow.DisplayInput (m_shadowQualityInput);
			OptionsWindow.DisplayInput (m_shadowDistanceInput);
			OptionsWindow.DisplayInput (m_shadowProjectionInput);
			OptionsWindow.DisplayInput (m_shadowResolutionInput);
			OptionsWindow.DisplayInput (m_shadowCascadesInput);

			OptionsWindow.DisplayInput (m_anisotropicFilteringInput);

		}

	}

}
