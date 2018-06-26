using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {

	public class UtilitiesWindow : PauseMenuWindow {

		private	Behaviours.Player	_player;



		UtilitiesWindow() {

			// set default parameters

			this.isOpened = true;
			this.windowName = "Utilities";

		}

		void Start () {

			_player = Behaviours.Player.FindInstance ();

			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = new Rect(Screen.width / 2 - 100, 10, 200, 100);
		}


		protected override void OnWindowGUI ()
		{

			if (_player) {
				// display player position
				Vector2 pos = new Vector2 (_player.transform.position.x + 3000, 6000 - (_player.transform.position.z + 3000));
				GUILayout.Label ("Pos: X" + (int)pos.x + " Y" + (int)pos.y + " Z" + (int)_player.transform.position.y);
			}

			if (GUILayout.Button ("Spawn vehicle")) {
				var spawner = FindObjectOfType<UIVehicleSpawner> ();
				if (spawner)
					spawner.SpawnVehicle ();
			}

			if (GUILayout.Button("Change player model"))
			{
				CharacterModelChanger.ChangePedestrianModel();
			}

		}

	}

}
