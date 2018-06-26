using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours.World;

namespace SanAndreasUnity.Settings {

	public class WorldSettings : MonoBehaviour {



		void Start () {

			UI.OptionsWindow.onGUI += this.OnOptionsGUI;

		}

		void OnOptionsGUI() {

			GUILayout.Label ("\nWORLD\n");


			if (Cell.Instance) {
				
				Cell.Instance.loadParkedVehicles = GUILayout.Toggle( Cell.Instance.loadParkedVehicles, "Load parked vehicles");

				//GUILayout.Label ("Max draw distance:");
				//Cell.Instance.maxDrawDistance = GUILayout.HorizontalSlider (Cell.Instance.maxDrawDistance, 50f, 1000f);
				UI.OptionsWindow.FloatSlider( ref Cell.Instance.maxDrawDistance, 50f, 1000f, "Max draw distance");

			}

		}

	}

}
