using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.UI;

namespace SanAndreasUnity.Settings {
	
	public class PlayerSettings : MonoBehaviour {

		OptionsWindow.FloatInput m_turnSpeedInput = new OptionsWindow.FloatInput() {
			description = "Turn speed",
			minValue = 3,
			maxValue = 30,
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.pedTurnSpeed,
			setValue = (value) => { PedManager.Instance.pedTurnSpeed = value; },
		};
		OptionsWindow.FloatInput m_enterVehicleRadiusInput = new OptionsWindow.FloatInput() {
			description = "Enter vehicle radius",
			minValue = 1,
			maxValue = 15,
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.enterVehicleRadius,
			setValue = (value) => { PedManager.Instance.enterVehicleRadius = value; },
		};

		OptionsWindow.BoolInput m_showSpeedometerInput = new OptionsWindow.BoolInput() {
			description = "Show speedometer",
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.showPedSpeedometer,
			setValue = (value) => { PedManager.Instance.showPedSpeedometer = value; },
		};
		OptionsWindow.FloatInput m_mouseSensitivityXInput = new OptionsWindow.FloatInput() {
			description = "Mouse sensitivity x",
			minValue = 0.2f,
			maxValue = 10f,
			isAvailable = () => PlayerController.Instance != null,
			getValue = () => PlayerController.Instance.CursorSensitivity.x,
			setValue = (value) => { PlayerController.Instance.CursorSensitivity.x = value; },
		};
		OptionsWindow.FloatInput m_mouseSensitivityYInput = new OptionsWindow.FloatInput() {
			description = "Mouse sensitivity y",
			minValue = 0.2f,
			maxValue = 10f,
			isAvailable = () => PlayerController.Instance != null,
			getValue = () => PlayerController.Instance.CursorSensitivity.y,
			setValue = (value) => { PlayerController.Instance.CursorSensitivity.y = value; },
		};



		void Awake ()
		{
			OptionsWindow.RegisterInputs ("PLAYER", m_turnSpeedInput, m_enterVehicleRadiusInput, 
				m_showSpeedometerInput, m_mouseSensitivityXInput, m_mouseSensitivityYInput);
		}

	}

}
