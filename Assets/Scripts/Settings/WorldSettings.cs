using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.UI;
using UnityEngine.XR.ARFoundation;

namespace SanAndreasUnity.Settings {

	public class WorldSettings : MonoBehaviour {

		static float s_maxDrawDistance = 500f;

		OptionsWindow.FloatInput m_maxDrawDistanceInput = new OptionsWindow.FloatInput() {
			description = "Max draw distance",
			minValue = 50,
			maxValue = 1000,
			getValue = () => Cell.Instance != null ? Cell.Instance.maxDrawDistance : s_maxDrawDistance,
			setValue = (value) => { s_maxDrawDistance = value; if (Cell.Instance != null) Cell.Instance.maxDrawDistance = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};

		private float _arScaleValue = 0f;

		private readonly OptionsWindow.FloatInput m_arScale = new OptionsWindow.FloatInput
		{
			description = "AR scale",
			minValue = -1f,
			maxValue = 1f,
			persistType = OptionsWindow.InputPersistType.OnStart,
		};

		private ARSessionOrigin _arSessionOrigin;


		void Awake ()
		{
			m_arScale.getValue = () => _arScaleValue;
			m_arScale.setValue = SetArScale;

			OptionsWindow.RegisterInputs ("WORLD", m_maxDrawDistanceInput, m_arScale);

			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (s1, s2) => OnActiveSceneChanged();
		}

		void OnActiveSceneChanged()
		{
			// apply settings

			// we need to find Cell with FindObjectOfType(), because it's Awake() method may have not been called yet
			Cell cell = Object.FindObjectOfType<Cell>();
			if (cell != null)
				cell.maxDrawDistance = s_maxDrawDistance;


			_arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
			SetArScale(_arScaleValue);
		}

		void SetArScale(float value)
		{
			_arScaleValue = value;

			if (_arSessionOrigin != null)
			{
				float absValue = Mathf.Abs(_arScaleValue);

				float scale = Mathf.Pow(2.7f, absValue * 8); // max is 2.7^8 = ~2800

				_arSessionOrigin.transform.localScale = _arScaleValue >= 0f ? Vector3.one * scale : Vector3.one * 1f / scale;
			}
		}

	}

}
