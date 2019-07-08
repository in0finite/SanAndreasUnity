using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.UI {

	public class VehicleSpawnerWindow : PauseMenuWindow {

		private	List<IGrouping<VehicleType, VehicleDef>>	vehicleGroupings = null;
		private	int[]	columnWidths = new int[]{ 120, 120, 30, 70 };



		VehicleSpawnerWindow() {

			// set default parameters

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
			this.windowRect = Utilities.GUIUtils.GetCenteredRectPerc( new Vector2(0.4f, 0.8f) );

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

			// for each vehicle, display a button which spawns it

			foreach (var grouping in this.vehicleGroupings) {
				
				GUILayout.Label (grouping.Key.ToString ());

				GUILayout.Space (10);

				// table columns
//				GUILayout.BeginHorizontal();
//				GUILayout.Label ("Game name", GUILayout.Width (this.columnWidths [0]));
//				GUILayout.Label ("Class name", GUILayout.Width (this.columnWidths [1]));
//				GUILayout.Label ("Id", GUILayout.Width (this.columnWidths [2]));
//				GUILayout.Label ("Frequency", GUILayout.Width (this.columnWidths [3]));
//				GUILayout.EndHorizontal ();
//
//				GUILayout.Space (10);

				// display all vehicles of this type
				foreach (var v in grouping) {
					//GUILayout.BeginHorizontal ();

					if (GUILayout.Button (v.GameName, GUILayout.Width(this.columnWidths[0]))) {
						
						if (Utilities.NetUtils.IsServer)
						{
							if (Ped.Instance != null)
								Vehicle.CreateInFrontOf (v.Id, Ped.Instance.transform);
						}
						else if (Net.PlayerRequests.Local != null)
						{
							Net.PlayerRequests.Local.RequestVehicleSpawn(v.Id);
						}
						
					}
					//GUILayout.Label (v.ClassName, GUILayout.Width (this.columnWidths [1]));
					//GUILayout.Label (v.Id.ToString(), GUILayout.Width (this.columnWidths [2]));
					//GUILayout.Label (v.Frequency.ToString(), GUILayout.Width (this.columnWidths [3]));

					//GUILayout.EndHorizontal ();
				}

				GUILayout.Space (10);
			}


			GUILayout.Space (20);

		}


	}

}
