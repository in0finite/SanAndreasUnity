using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours.World;

namespace SanAndreasUnity.UI {

	public class DayTimeWindow : PauseMenuWindow {
		


		DayTimeWindow() {

			// set default parameters

			this.windowName = "DayTime";

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = new Rect(10, Screen.height - 220, 250, 200);
		}


		protected override void OnWindowGUI ()
		{

			GUILayout.Label("Set Time:");

			foreach (var en in System.Enum.GetValues(typeof(TimeState)))
			{
				TimeState e = (TimeState)en;
				if (GUILayout.Button(e.ToString()))
					WorldController.SetTime(e);
			}

		}

	}

}
