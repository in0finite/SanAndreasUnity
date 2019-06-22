using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.UI;

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


		void Awake ()
		{
			OptionsWindow.RegisterInputs ("WORLD", m_maxDrawDistanceInput);
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (s1, s2) => OnActiveSceneChanged();
		}

		void OnActiveSceneChanged()
		{
			// apply settings

			// we need to find Cell with FindObjectOfType(), because it's Awake() method may have not been called yet
			Cell cell = Object.FindObjectOfType<Cell>();
			if (cell != null)
				cell.maxDrawDistance = s_maxDrawDistance;
			
		}

	}

}
