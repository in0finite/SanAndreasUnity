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

			if (Ped.Instance) {
				// display player position
			//	Vector2 pos = new Vector2 (_player.transform.position.x + 3000, 6000 - (_player.transform.position.z + 3000));
				GUILayout.Label ("Pos: " + Ped.InstancePos);
			}

			if (Utilities.NetUtils.IsServer)
				DisplayServerGui();
			else if (Net.PlayerRequests.Local != null)
				DisplayClientOnlyGui();

		}

		void DisplayServerGui()
		{
			Transform nearbyTransform = Ped.Instance != null ? Ped.Instance.transform : null;

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
					Ped.SpawnPed (Ped.RandomPedId, nearbyTransform);
				}
			}

			if (GUILayout.Button("Spawn 5 stalker peds"))
			{
				for (int i = 0; i < 5; i++)
				{
					Ped.SpawnPedStalker (Ped.RandomPedId, nearbyTransform);
				}
			}

			if (GUILayout.Button("Destroy all vehicles"))
			{
				var vehicles = FindObjectsOfType<Behaviours.Vehicles.Vehicle> ();
				
				foreach (var v in vehicles) {
					Destroy (v.gameObject);
				}
			}

		}

		void DisplayClientOnlyGui()
		{

			var pr = Net.PlayerRequests.Local;

			if (GUILayout.Button("Request vehicle"))
			{
				pr.RequestVehicleSpawn();
			}

			if (GUILayout.Button("Request ped model change"))
			{
				pr.RequestPedModelChange();
			}

			if (GUILayout.Button("Request suicide"))
			{
				pr.RequestSuicide();
			}

			if (GUILayout.Button("Request ped stalker"))
			{
				pr.SpawnPedStalker();
			}

			if (GUILayout.Button("Request to destroy all vehicles"))
			{
				pr.RequestToDestroyAllVehicles();
			}

		}

	}

}
