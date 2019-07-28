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
			float width = 240;
			if (Utilities.F.ScreenHasHighDensity)
			{
				width *= 1.33f;
				width = Mathf.Min(width, Screen.width * 0.3f);
			}
			float height = width * 0.9f;
			this.windowRect = new Rect(Screen.width / 2 - width / 2, 10, width, height);
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
				if (Ped.Instance != null)
					Behaviours.Vehicles.Vehicle.CreateRandomInFrontOf(Ped.Instance.transform);
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
					Ped.SpawnPedStalker (Ped.RandomPedId, nearbyTransform, Ped.Instance);
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
				pr.RequestVehicleSpawn(-1);
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

			if (GUILayout.Button("Request to destroy my vehicles"))
			{
				pr.RequestToDestroyMyVehicles();
			}

		}

	}

}
