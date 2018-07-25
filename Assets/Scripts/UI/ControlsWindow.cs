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
			this.windowRect = new Rect(Screen.width / 2 - 150, 30, 300, 400);
		}


		protected override void OnWindowGUI ()
		{
			
			GUILayout.Label ( GetControlsText() );

		}


		public static string GetControlsText() {

			return
			"V - Spawn vehicle\n\n" +
            "F - Flip car\n\n" +
			"P - Change pedestrian model\n\n" +
			"Left Shift - Jump / Fly fast\n\n" +
			"Space - Sprint\n\n" +
			"Mouse Scroll - Zoom in / out player camera\n\n" +
			"Esc / F1 (WIP) - Toggle pause menu\n\n" +
			"F2 - Open / close console (WIP)\n\n" +
			"T - Enable debug flying mode\n\n" +
			"R - Enable debug noclip mode\n\n" +
			"Backspace - Fly up\n\n" +
			"Delete - Fly down\n\n" +
			"Z - Fly very fast\n\n" +
			"E - Enter/exit vehicles\n\n" +
			"L - Turn off / on car lights\n\n" +
			"F10 - Toggle FPS\n\n" +
			"F9 - Toggle velocimeter\n\n" +
			"O - Toggle quality\n\n" +
			"B - Zoom out minimap\n\n" +
			"N - Zoom in minimap\n\n" +
			"M - Open the entire map\n\n" +
			"F8 - Show more info in the minimap";

		}

	}

}
