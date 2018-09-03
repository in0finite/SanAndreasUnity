using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.UI;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.Settings {

	public class MiscSettings : MonoBehaviour {
		
		OptionsWindow.FloatInput m_timeScaleInput = new OptionsWindow.FloatInput( "Time scale", 0f, 4f );
		OptionsWindow.BoolInput m_displayHealthBarsInput = new OptionsWindow.BoolInput ("Display health bar above peds");
		OptionsWindow.BoolInput m_displayMinimapInput = new OptionsWindow.BoolInput ("Display minimap");



		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nMISC\n");


			m_timeScaleInput.value = Time.timeScale;
			if (OptionsWindow.FloatSlider (m_timeScaleInput)) {
				Time.timeScale = m_timeScaleInput.value;
			}

			if (PedManager.Instance)
			{
				m_displayHealthBarsInput.value = PedManager.Instance.displayHealthBarAbovePeds;
				if (OptionsWindow.Toggle (m_displayHealthBarsInput))
				{
					PedManager.Instance.displayHealthBarAbovePeds = m_displayHealthBarsInput.value;
				}
			}

			if (MiniMap.Instance)
			{
				m_displayMinimapInput.value = MiniMap.Instance.gameObject.activeSelf;
				if (OptionsWindow.Toggle (m_displayMinimapInput))
				{
					MiniMap.Instance.gameObject.SetActive (m_displayMinimapInput.value);
				}
			}

		}

	}

}
