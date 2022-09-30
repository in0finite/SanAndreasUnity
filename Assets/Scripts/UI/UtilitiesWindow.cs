using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using System.Linq;
using SanAndreasUnity.Behaviours.Vehicles;

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
			float height = 210;
			if (UGameCore.Utilities.F.ScreenHasHighDensity)
			{
				width = Screen.width * 0.375f;
				height = Screen.height * 0.4f;
			}
			this.windowRect = new Rect(Screen.width / 2 - width / 2, 10, width, height);
		}


		protected override void OnWindowGUI ()
		{

			if (Ped.Instance) {
				// display player position
			//	Vector2 pos = new Vector2 (_player.transform.position.x + 3000, 6000 - (_player.transform.position.z + 3000));
				GUILayout.Label ("Pos: " + Ped.InstancePos);
			}

			if (UGameCore.Utilities.NetUtils.IsServer)
				DisplayServerGui();
			else if (Net.Player.Local != null)
				DisplayClientOnlyGui();

		}

		void DisplayServerGui()
		{
			Transform nearbyTransform = Ped.Instance != null ? Ped.Instance.transform : null;

			if (GUILayout.Button ("Spawn vehicle")) {
				if (Ped.Instance != null)
					Vehicle.CreateRandomInFrontOf(nearbyTransform);
			}

			if (GUILayout.Button("Change player model"))
			{
				SendCommand("/skin");
			}

			if (GUILayout.Button("Spawn 5 peds"))
			{
				for (int i = 0; i < 5; i++)
				{
					Ped.SpawnPedAI(Ped.RandomPedId, nearbyTransform);
				}
			}

			if (GUILayout.Button("Spawn stalker ped"))
			{
				SendCommand("/stalker");
			}

			if (GUILayout.Button("Spawn enemy ped"))
			{
				SendCommand("/enemy");
			}

			if (GUILayout.Button("Destroy all vehicles"))
			{
				var vehicles = Behaviours.Vehicles.Vehicle.AllVehicles.ToArray();
				
				foreach (var v in vehicles) {
					v.Explode();
				}
			}

		}

		void SendCommand(string command)
		{
			Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(command);
		}

		void DisplayClientOnlyGui()
		{

			if (GUILayout.Button("Request vehicle"))
			{
				SendCommand("/veh");
			}

			if (GUILayout.Button("Request ped model change"))
			{
				SendCommand("/skin");
			}

			if (GUILayout.Button("Request suicide"))
			{
				SendCommand("/suicide");
			}

			if (GUILayout.Button("Request ped stalker"))
			{
				SendCommand("/stalker");
			}

			if (GUILayout.Button("Request enemy ped"))
			{
				SendCommand("/enemy");
			}

			if (GUILayout.Button("Request to destroy my vehicles"))
			{
				SendCommand("/dveh");
			}

		}

	}

}
