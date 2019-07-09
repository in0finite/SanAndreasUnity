using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {

	public class ControlsWindow : PauseMenuWindow {
		


		ControlsWindow() {

			// set default parameters

			this.windowName = "Controls";
			this.useScrollView = true;

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = Utilities.GUIUtils.GetCenteredRect(new Vector2(400, 450));
		}


		protected override void OnWindowGUI ()
		{
			
			GUILayout.Label ( GetControlsText() );

		}


		public static string GetControlsText() {

			return
			"V - Spawn vehicle\n\n" +
			"P - Change ped model\n\n" +
			"W/A/S/D - Move ped\n\n" +
			"Space - Sprint\n\n" +
			"Alt - Walk\n\n" +
			"Left shift - Jump\n\n" +
			"Mouse scroll - Zoom in / out camera\n\n" +
			"E/Q - Next/previous weapon\n\n" +
			"Esc - Toggle pause menu\n\n" +
			(Utilities.NetUtils.IsServer ?
			("T - Fly mode\n\n" +
			"R - Fly through mode\n\n") : "") +
			"Enter - Enter/exit vehicles\n\n" +
			"F10 - Toggle FPS counter\n\n" +
			"B - Zoom out minimap\n\n" +
			"N - Zoom in minimap\n\n" +
			"M - Open the entire map\n\n" +
			"F8 - Show more info in the minimap\n\n";

		}

	}

}
