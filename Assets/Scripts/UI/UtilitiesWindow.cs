using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.UI {

	public class UtilitiesWindow : PauseMenuWindow {
		


		UtilitiesWindow() {

			// set default parameters

			this.windowName = "Utilities";
			this.useScrollView = true;

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = new Rect(Screen.width / 2 - 100, 10, 200, 180);
		}


		protected override void OnWindowGUI ()
		{

			if (Player.Instance) {
				// display player position
			//	Vector2 pos = new Vector2 (_player.transform.position.x + 3000, 6000 - (_player.transform.position.z + 3000));
				GUILayout.Label ("Pos: " + Player.InstancePos);
			}

			if (GUILayout.Button ("Spawn random vehicle")) {
				var spawner = FindObjectOfType<UIVehicleSpawner> ();
				if (spawner)
					spawner.SpawnVehicle ();
			}

			if (GUILayout.Button("Change player model"))
			{
				CharacterModelChanger.ChangePedestrianModel();
			}

			if (GUILayout.Button("Spawn 5 peds"))
			{
				for (int i = 0; i < 5; i++)
				{
					Player.SpawnPed (Player.RandomPedId);
				}
			}

			if (GUILayout.Button("Spawn 5 stalker peds"))
			{
				for (int i = 0; i < 5; i++)
				{
					Player.SpawnPedStalker (Player.RandomPedId);
				}
			}

			if (GUILayout.Button("Destroy all vehicles"))
			{
				var vehicles = FindObjectsOfType<Behaviours.Vehicles.Vehicle> ();
				var vehicleToIgnore = Player.Instance != null ? Player.Instance.CurrentVehicle : null;

				foreach (var v in vehicles) {
					if (v != vehicleToIgnore)
						Destroy (v.gameObject);
				}
			}

		}

	}

}
