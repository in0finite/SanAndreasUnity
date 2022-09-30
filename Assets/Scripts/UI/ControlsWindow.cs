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
			this.windowRect = UGameCore.Utilities.GUIUtils.GetCenteredRect(new Vector2(400, 450));
		}


		protected override void OnWindowGUI ()
		{
			
			GUILayout.Label ( GetControlsText() );

		}


		public static string GetControlsText() {

			return

			"<b>GENERAL</b>\n\n" +
			"V - Spawn vehicle\n\n" +
			"P - Change ped model\n\n" +
			"Mouse scroll - Zoom in / out camera\n\n" +
			"Esc - Toggle pause menu\n\n" +

			"\n<b>PED</b>\n\n" +
			"W/A/S/D - Move ped\n\n" +
			"Right click - Aim\n\n" +
			"Left click - Fire\n\n" +
			"Space - Sprint\n\n" +
			"Alt - Walk\n\n" +
			"Left shift - Jump\n\n" +
			"C - Crouch\n\n" +
			"Left/right while crouch aiming - Roll\n\n" +
			"E/Q - Switch weapon\n\n" +
			"G while aiming - Recruit peds to follow you\n\n" +
			(UGameCore.Utilities.NetUtils.IsServer ?
			("T - Fly mode\n\n" +
			"R - Fly through mode\n\n") : "") +
			"Enter - Enter vehicles\n\n" +

			"\n<b>VEHICLE</b>\n\n" +
			"W/A/S/D - Move vehicle\n\n" +
			"Space - Handbrake\n\n" +
			"Right click - Switch to drive-by mode\n\n" +
			"Enter - Exit vehicle\n\n" +
			"E/Q as driver - Switch radio station\n\n" +
			"E/Q as passenger - Switch weapon\n\n" +

			"\n<b>MINIMAP</b>\n\n" +
			"B - Zoom out minimap\n\n" +
			"N - Zoom in minimap\n\n" +
			"M - Open the entire map\n\n";

		}

	}

}
