using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Vehicles;
using UGameCore.Utilities;

namespace SanAndreasUnity.UI {

	public class VehicleSpawnerWindow : PauseMenuWindow {

		private List<IGrouping<VehicleType, VehicleDef>> vehicleGroupings = null;
		private int[] columnWidths = new int[] { 150, 120, 30, 70 };
		private string m_searchText = "";

		private static GUIStyle s_colorBoxStyle = null;
		private static GUIStyle ColorBoxStyle
		{
			get
			{
				if (s_colorBoxStyle != null)
					return s_colorBoxStyle;

				s_colorBoxStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };
				return s_colorBoxStyle;
			}
		}



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
			this.windowRect = GUIUtils.GetCenteredRectPerc( new Vector2(0.4f, 0.8f) );

		}


		void SendCommand(string command)
		{
			Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(command);
		}

		void GetVehicleDefs() {

			// get all vehicle definitions
			var allVehicles = Item.GetDefinitions<VehicleDef> ();

			// group them by type
			var groupings = allVehicles.GroupBy (v => v.VehicleType);

			this.vehicleGroupings = groupings.ToList ();
		}


		protected override void OnWindowGUIBeforeContent()
		{
			// current vehicle info

			if (Ped.Instance != null && Ped.Instance.CurrentVehicle != null)
			{
				var vehicle = Ped.Instance.CurrentVehicle;

				GUILayout.Label("Current vehicle:");

				GUILayout.Label(vehicle.Definition.GameName, GUILayout.Width(this.columnWidths[0]));

				GUILayout.Label("Change paintjob:");

				var oldBackgroundColor = GUI.backgroundColor;

				GUILayout.BeginHorizontal();

				if (GUIUtils.ButtonWithCalculatedSize("Random"))
					OnPaintjobPressed(null);

				var defaultClrs = CarColors.GetCarDefaults(vehicle.Definition.ModelName);

				if (defaultClrs != null)
                {
					var colors = defaultClrs.GetAllColorIndices()
						.Select(indices => CarColors.FromIndices(indices));

					foreach (var colorArray in colors)
					{
						if (colorArray.Length == 0)
							continue;
						DisplayPaintjob(colorArray);
						GUILayout.Space(5);
					}
				}

				GUILayout.EndHorizontal();

				GUI.backgroundColor = oldBackgroundColor;

				GUILayout.Space(15);
			}

			// search box

			GUILayout.BeginHorizontal();
			GUILayout.Label("Search:");
			GUILayout.Space(5);
			m_searchText = GUILayout.TextField(m_searchText, GUILayout.Width(120));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
		}

		protected override void OnWindowGUI ()
		{

			if (Behaviours.Loader.HasLoaded && null == this.vehicleGroupings) {
				GetVehicleDefs ();
			}

			if (null == this.vehicleGroupings)
				return;


            GUILayout.Space(10);

			// for each vehicle, display a button which spawns it

			string nameFilter = m_searchText.Trim();

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

					if (!string.IsNullOrWhiteSpace(nameFilter))
					{
						if (v.GameName.IndexOf(nameFilter, System.StringComparison.InvariantCultureIgnoreCase) < 0)
							continue;
					}

					//GUILayout.BeginHorizontal ();

					if (GUILayout.Button (v.GameName, GUILayout.Width(this.columnWidths[0]))) {
						
						if (Net.Player.Local != null)
						{
							SendCommand($"/veh {v.Id}");
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

		private void DisplayPaintjob(Color32[] colors)
        {
			int width = 30 / colors.Length;

            foreach (var color in colors)
            {
				DisplaySingleColor(color, width, colors);
            }
		}

		private void DisplaySingleColor(Color32 color, int width, Color32[] allColors)
        {
			GUI.backgroundColor = color;
			if (GUILayout.Button(GUIContent.none, ColorBoxStyle, GUILayout.Width(width), GUILayout.Height(30)))
				OnPaintjobPressed(allColors);
		}

		private void OnPaintjobPressed(Color32[] colors)
        {
			SendCommand("/paintjob " + (colors != null ? string.Join(" ", colors.Select(c => GetColorString(c))) : ""));
        }

		private string GetColorString(Color32 color)
        {
			return "#" + ColorUtility.ToHtmlStringRGBA(color);
		}


	}

}
