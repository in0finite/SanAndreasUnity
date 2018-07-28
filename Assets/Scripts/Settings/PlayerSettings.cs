using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.UI;

namespace SanAndreasUnity.Settings {
	
	public class PlayerSettings : MonoBehaviour {



		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nPLAYER\n");

			if (null == Player.Instance) {
				GUILayout.Label ("Player object not found");
				return;
			}

			//GUILayout.Label ("Jump speed:");
			//Player.Instance.jumpSpeed = GUILayout.HorizontalSlider (Player.Instance.jumpSpeed, 3f, 30f);
			OptionsWindow.FloatSlider( ref Player.Instance.jumpSpeed, 3f, 30f, "Jump speed");

			//GUILayout.Label ("Turn speed:");
			//Player.Instance.TurnSpeed = GUILayout.HorizontalSlider (Player.Instance.TurnSpeed, 3f, 30f);
			OptionsWindow.FloatSlider( ref Player.Instance.TurnSpeed, 3f, 30f, "Turn speed");

			if (PlayerController.Instance != null) {
				
				PlayerController._showVel = GUILayout.Toggle (PlayerController._showVel, "Show speedometer");

				//GUILayout.Label ("Mouse sensitivity x:");
				//PlayerController.Instance.CursorSensitivity.x = GUILayout.HorizontalSlider (PlayerController.Instance.CursorSensitivity.x, 0.2f, 10f);
				OptionsWindow.FloatSlider (ref PlayerController.Instance.CursorSensitivity.x, 0.2f, 10f, "Mouse sensitivity x");

				//GUILayout.Label ("Mouse sensitivity y:");
				//PlayerController.Instance.CursorSensitivity.y = GUILayout.HorizontalSlider (PlayerController.Instance.CursorSensitivity.y, 0.2f, 10f);
				OptionsWindow.FloatSlider (ref PlayerController.Instance.CursorSensitivity.y, 0.2f, 10f, "Mouse sensitivity y");

				//GUILayout.Label ("Enter vehicle radius:");
				//PlayerController.Instance.EnterVehicleRadius = GUILayout.HorizontalSlider (PlayerController.Instance.EnterVehicleRadius, 1.0f, 15f);
				OptionsWindow.FloatSlider (ref PlayerController.Instance.EnterVehicleRadius, 1.0f, 15f, "Enter vehicle radius");
			
			}

		}

	}

}
