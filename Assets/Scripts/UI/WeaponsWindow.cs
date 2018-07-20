using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Behaviours;
using System.Linq;
using SanAndreasUnity.Utilities;

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
			this.windowRect = Utilities.GUIUtils.GetCenteredRect( new Vector2( 600, 400 ) );
		}


		protected override void OnWindowGUI ()
		{

			// display all weapons from the game

			// add option to add them to player



			bool playerExists = Player.Instance != null;


			// button to remove all weapons from player
			if (playerExists) {
				if (GUILayout.Button ("Remove weapon"))
					Player.Instance.WeaponHolder.RemoveAllWeapons ();
				GUILayout.Space (15);
			}


		//	var defs = Item.GetDefinitions<Importing.Items.Definitions.WeaponDef> ();
			var datas = Importing.Weapons.WeaponData.LoadedWeaponsData.DistinctBy( wd => wd.weaponType );

			foreach (var data in datas) {

				GUILayout.Label ("Id: " + data.modelId1 + " Name: " + data.weaponType + " Slot: " + data.weaponslot +
					" Flags: " + ( null == data.gunData ? "" : string.Join(" ", data.gunData.Flags) ) );

				if (playerExists) {
					if (GUILayout.Button ("Give", GUILayout.Width(70))) {
						// give weapon to player
						Player.Instance.WeaponHolder.SetWeaponAtSlot( data.modelId1, WeaponSlot.Machine );
						Player.Instance.WeaponHolder.SwitchWeapon (WeaponSlot.Machine);
					}
				}

				GUILayout.Space (5);
			}

		}

	}

}
