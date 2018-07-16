using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Settings {

	public class CameraSettings : MonoBehaviour {



		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nCAMERA\n");

			if (null == Camera.main) {
				return;
			}

			Camera.main.farClipPlane = UI.OptionsWindow.FloatSlider( Camera.main.farClipPlane, 100f, 5000f, "Far clip plane");

			Camera.main.fieldOfView = UI.OptionsWindow.FloatSlider( Camera.main.fieldOfView, 20f, 120f, "Field of view");


		}

	}

}
