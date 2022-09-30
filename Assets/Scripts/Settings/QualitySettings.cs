using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.UI;
using UGameCore.Utilities;
using Quality = UnityEngine.QualitySettings;

namespace SanAndreasUnity.Settings {

	public class QualitySettings : MonoBehaviour {

		static string[] s_qualitySettingsNames;

		OptionsWindow.FloatInput m_fpsInput = new OptionsWindow.FloatInput( "Max fps", 0f, 200f ) {
			getValue = () => Behaviours.GameManager.GetMaxFps (),
			setValue = (value) => { Behaviours.GameManager.SetMaxFps (value.RoundToInt ()); },
			persistType = OptionsWindow.InputPersistType.OnStart
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
		OptionsWindow.MultipleOptionsInput<string> m_qualityLevelInput = new OptionsWindow.MultipleOptionsInput<string>() {
			description = "Quality level",
			getValue = () => s_qualitySettingsNames[ Quality.GetQualityLevel() ],
			setValue = (value) => { Quality.SetQualityLevel (System.Array.FindIndex (s_qualitySettingsNames, n => n == value)); }
		};



		void Awake ()
		{
			
			s_qualitySettingsNames = m_qualityLevelInput.Options = Quality.names;

			var inputs = new OptionsWindow.Input[]{ m_fpsInput, m_antiAliasingInput, m_qualityLevelInput, m_shadowQualityInput, m_shadowDistanceInput,
				m_shadowProjectionInput, m_shadowResolutionInput, m_shadowCascadesInput, m_anisotropicFilteringInput };

			foreach (var input in inputs)
			{
				input.category = "QUALITY";
				OptionsWindow.RegisterInput (input);
			}

		}


	}

}
