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

			this.isOpened = true;
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
			this.windowRect = new Rect(Screen.width - 260, 10, 250, 10 + (25 * _spawns.Count));
		}


		protected override void OnWindowGUI ()
		{

			if (null == Player.Instance) {
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
					Player.Instance.transform.position = spawnLocation.position;
					Player.Instance.transform.rotation = spawnLocation.rotation;
				}
			}

		}

	}

}
