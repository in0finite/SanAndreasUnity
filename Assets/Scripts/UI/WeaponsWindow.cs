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



			bool playerExists = Ped.Instance != null;


			if (playerExists)
			{
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button ("Remove all weapons", GUILayout.ExpandWidth(false)))
					Ped.Instance.WeaponHolder.RemoveAllWeapons ();
				
				GUILayout.Space (5);

				if (GUILayout.Button ("Give ammo", GUILayout.ExpandWidth (false)))
				{
					foreach (var weapon in Ped.Instance.WeaponHolder.AllWeapons)
						WeaponHolder.AddRandomAmmoAmountToWeapon (weapon);
				}
				
				GUILayout.EndHorizontal ();
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
						Ped.Instance.WeaponHolder.SetWeaponAtSlot( data.modelId1, data.weaponslot );
						Ped.Instance.WeaponHolder.SwitchWeapon (data.weaponslot);
					}
				}

				GUILayout.Space (5);
			}

		}

	}

}
