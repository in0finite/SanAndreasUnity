using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.UI {

	public class TeleportWindow : PauseMenuWindow {

		// spawn locations
		private	List<Transform> _spawns = new List<Transform>();
		public List<Transform> Spawns { get { return this._spawns; } }



		TeleportWindow() {

			// set default parameters

			this.windowName = "Teleport";
			this.useScrollView = true;

		}

		void Awake () {

			// find all spawn locations
			var obj = GameObject.Find("Player Spawns");
			if (obj)
				_spawns = obj.GetComponentsInChildren<Transform> ().ToList ();
			
		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			float width = 260;
			float height = Mathf.Min( 0.7f * Screen.height, 10 + 25 * _spawns.Count );
			this.windowRect = new Rect(Screen.width - width - 10, 10, width, height);
		}


		protected override void OnWindowGUI ()
		{

			if (null == Ped.Instance) {
				GUILayout.Label ("Player object not found");
				return;
			}


			for (int i = 1; i < _spawns.Count; i++)
			{
				var spawnLocation = _spawns [i];
				if (null == spawnLocation)
					continue;

				if (GUILayout.Button (spawnLocation.name))
				{
					Ped.Instance.Teleport (spawnLocation.position, spawnLocation.rotation);
				}
			}

		}

	}

}
