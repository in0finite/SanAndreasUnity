using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.UI;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.Settings {

	public class MiscSettings : MonoBehaviour {
		
		OptionsWindow.FloatInput m_timeScaleInput = new OptionsWindow.FloatInput( "Time scale", 0f, 4f ) {
			getValue = () => Time.timeScale,
			setValue = (value) => { Time.timeScale = value; }
		};
		OptionsWindow.BoolInput m_displayHealthBarsInput = new OptionsWindow.BoolInput ("Display health bar above peds") {
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.displayHealthBarAbovePeds,
			setValue = (value) => { PedManager.Instance.displayHealthBarAbovePeds = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_displayMinimapInput = new OptionsWindow.BoolInput ("Display minimap") {
			isAvailable = () => MiniMap.Instance != null,
			getValue = () => MiniMap.Instance.gameObject.activeSelf,
			setValue = (value) => { MiniMap.Instance.gameObject.SetActive (value); },
			persistType = OptionsWindow.InputPersistType.AfterLoaderFinishes
		};
		OptionsWindow.BoolInput m_runInBackgroundInput = new OptionsWindow.BoolInput ("Run in background") {
			getValue = () => Application.runInBackground,
			setValue = (value) => { Application.runInBackground = value; },
			persistType = OptionsWindow.InputPersistType.AfterLoaderFinishes
		};



		void Awake ()
		{
			var inputs = new OptionsWindow.Input[] { m_timeScaleInput, m_displayHealthBarsInput, m_displayMinimapInput,
				m_runInBackgroundInput
			};

			foreach (var input in inputs)
			{
				input.category = "MISC";
				OptionsWindow.RegisterInput (input);
			}

		}


	}

}
