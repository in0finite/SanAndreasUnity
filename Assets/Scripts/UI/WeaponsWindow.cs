using UnityEngine;
using SanAndreasUnity.Behaviours;
using UGameCore.Utilities;

namespace SanAndreasUnity.UI {

	public class WeaponsWindow : PauseMenuWindow {



		WeaponsWindow() {

			// set default parameters

			this.windowName = "Weapons";
			this.useScrollView = true;

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = GUIUtils.GetCenteredRect( new Vector2( 600, 400 ) );
		}


		void SendCommand(string command)
		{
			Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(command);
		}

		protected override void OnWindowGUIBeforeContent ()
		{
			base.OnWindowGUIBeforeContent ();

			bool playerExists = Ped.Instance != null;

			if (playerExists)
			{
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button ("Add random weapons", GUILayout.ExpandWidth(false)))
					SendCommand("/rand_w");

				GUILayout.Space (5);

				if (GUILayout.Button ("Remove all weapons", GUILayout.ExpandWidth(false)))
					SendCommand("/rem_w");

				GUILayout.Space (5);

				if (GUILayout.Button ("Remove current weapon", GUILayout.ExpandWidth(false)))
					SendCommand("/rem_current_w");

				GUILayout.Space (5);

				if (GUILayout.Button ("Give ammo", GUILayout.ExpandWidth (false)))
					SendCommand("/ammo");
				
				GUILayout.EndHorizontal ();
				GUILayout.Space (15);
			}

		}

		protected override void OnWindowGUI ()
		{

			// display all weapons from the game

			// add option to add them to player



			bool playerExists = Ped.Instance != null;


		//	var defs = Item.GetDefinitions<Importing.Items.Definitions.WeaponDef> ();
			var datas = Importing.Weapons.WeaponData.LoadedWeaponsData.DistinctBy( wd => wd.weaponType );

			foreach (var data in datas) {

				GUILayout.Label ("Id: " + data.modelId1 + " Name: " + data.weaponType + " Slot: " + data.weaponslot +
					" Flags: " + ( null == data.gunData ? "" : string.Join(" ", data.gunData.Flags) ) );

				if (playerExists) {
					if (GUILayout.Button ("Give", GUILayout.Width(70))) {
						// give weapon to player
						SendCommand($"/w {data.modelId1}");
					}
				}

				GUILayout.Space (5);
			}

		}

	}

}
