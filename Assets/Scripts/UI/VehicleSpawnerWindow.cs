using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;

namespace SanAndreasUnity.UI {

	public class VehicleSpawnerWindow : PauseMenuWindow {

		private	List<IGrouping<VehicleType, VehicleDef>>	vehicleGroupings = null;



		VehicleSpawnerWindow() {

			// set default parameters

			this.isOpened = false;
			this.windowName = "Vehicle Spawner";
			this.useScrollView = true;

		}

		void Start () {

			this.RegisterButtonInPauseMenu ();

			// adjust rect

			//float windowWidth = 400;
			//float windowHeight = Mathf.Min (700, Screen.height * 0.7f);

			//this.windowRect = Utilities.GUIUtils.GetCornerRect (SanAndreasUnity.Utilities.ScreenCorner.TopRight, 
			//	new Vector2 (windowWidth, windowHeight), new Vector2 (20, 20));
			this.windowRect = Utilities.GUIUtils.GetCenteredRectPerc( new Vector2(0.8f, 0.8f) );

		}


		void GetVehicleDefs() {

			// get all vehicle definitions
			var allVehicles = Item.GetDefinitions<VehicleDef> ();

			// group them by type
			var groupings = allVehicles.GroupBy (v => v.VehicleType);

			this.vehicleGroupings = groupings.ToList ();
		}


		protected override void OnWindowGUI ()
		{

			if (Behaviours.Loader.HasLoaded && null == this.vehicleGroupings) {
				GetVehicleDefs ();
			}

			if (null == this.vehicleGroupings)
				return;


			GUILayout.Space (10);

			// display list of all vehicles, and a button next to them which will spawn them

			foreach (var grouping in this.vehicleGroupings) {
				
				GUILayout.Label (grouping.Key.ToString ());

				GUILayout.Space (10);

				// display all vehicles of this type
				foreach (var v in grouping) {
					//GUILayout.BeginHorizontal ();

					if (GUILayout.Button (v.GameName)) {
						Behaviours.Vehicles.Vehicle.CreateInFrontOfPlayer (v.Id);
					}
					//GUILayout.Label (v.GameName);
					//GUILayout.Label (v.ClassName);
					//GUILayout.Label (v.Id);
					//GUILayout.Label (v.Frequency);

					//GUILayout.EndHorizontal ();
				}

				GUILayout.Space (10);
			}


			GUILayout.Space (20);

		}


	}

}
