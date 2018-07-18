using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Behaviours;

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
			this.windowRect = Utilities.GUIUtils.GetCenteredRect( new Vector2( 500, 400 ) );
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


			var defs = Item.GetDefinitions<Importing.Items.Definitions.WeaponDef> ();

			foreach (var def in defs) {

				GUILayout.Label ("Id " + def.Id + " Name " + def.ModelName);

				if (playerExists) {
					if (GUILayout.Button ("Give", GUILayout.Width(70))) {
						// give weapon to player
						// in which slot ?
						Player.Instance.WeaponHolder.SetWeaponAtSlot( def, WeaponSlot.Machine );
						Player.Instance.WeaponHolder.SwitchWeapon (WeaponSlot.Machine);
					}
				}

				GUILayout.Space (5);
			}

		}

	}

}
