using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.UI;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Settings {

	public class MiscSettings : MonoBehaviour {
		
		OptionsWindow.FloatInput m_timeScaleInput = new OptionsWindow.FloatInput( "Time scale", 0f, 4f );


		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nMISC\n");


			m_timeScaleInput.value = Time.timeScale;
			if (OptionsWindow.FloatSlider (m_timeScaleInput)) {
				Time.timeScale = m_timeScaleInput.value;
			}

		}

	}

}
