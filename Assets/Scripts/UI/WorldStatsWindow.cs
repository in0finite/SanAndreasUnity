using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {
	
	public class WorldStatsWindow : PauseMenuWindow {


		WorldStatsWindow() {

			this.windowName = "World stats";
			this.windowRect = new Rect(10, 10, 250, 330);

		}

		void Start () {
			
		}


		protected override void OnWindowGUI ()
		{

			if (Behaviours.World.Cell.Instance != null) {
				Behaviours.World.Cell.Instance.showWindow (this.WindowId);
			}

		}

	}

}
