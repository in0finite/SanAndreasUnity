using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.UI;

namespace SanAndreasUnity.Settings {

	public class WorldSettings : MonoBehaviour {

		OptionsWindow.FloatInput m_maxDrawDistanceInput = new OptionsWindow.FloatInput() {
			description = "Max draw distance",
			minValue = 50,
			maxValue = 1000,
			isAvailable = () => Cell.Instance != null,
			getValue = () => Cell.Instance.maxDrawDistance,
			setValue = (value) => { Cell.Instance.maxDrawDistance = value; },
			persistType = OptionsWindow.InputPersistType.AfterLoaderFinishes
		};


		void Awake ()
		{
			OptionsWindow.RegisterInputs ("WORLD", m_maxDrawDistanceInput);
		}

	}

}
