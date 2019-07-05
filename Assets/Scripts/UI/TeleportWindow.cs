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

		void OnSceneChanged (SceneChangedMessage msg) {

			_spawns.Clear ();

			if (!GameManager.IsInStartupScene)
				_spawns = FindSpawnPlaces ().ToList ();

			this.AdjustWindowRect ();

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			this.AdjustWindowRect ();
		}


		public static Transform[] FindSpawnPlaces ()
		{
			if (Utilities.NetUtils.IsServer)
			{
				var obj = GameObject.Find("Player Spawns");
				if (obj)
					return obj.GetComponentsInChildren<Transform> ();
			}
			return new Transform[0];
		}


		private void AdjustWindowRect ()
		{
			float width = 260;
			float height = Mathf.Min( 0.7f * Screen.height, 10 + 25 * _spawns.Count );
			this.windowRect = new Rect(Screen.width - width - 10, 10, width, height);
		}

		protected override void OnWindowGUI ()
		{

			if (!Utilities.NetUtils.IsServer)
				return;

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
