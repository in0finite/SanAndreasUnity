using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.World;
using UGameCore.Utilities;

namespace SanAndreasUnity.UI {

	public class TeleportWindow : PauseMenuWindow {

		// spawn locations
		private	List<TransformDataStruct> _spawns = new List<TransformDataStruct>();
		public List<TransformDataStruct> Spawns { get { return this._spawns; } }
		private List<string> _spawnNames = new List<string>();



		TeleportWindow() {

			// set default parameters

			this.windowName = "Teleport";
			this.useScrollView = true;

		}

		void OnSceneChanged (SceneChangedMessage msg) {

			_spawns.Clear ();
			_spawnNames.Clear();

			if (!GameManager.IsInStartupScene)
				FindSpawnPlacesInternal();

			this.AdjustWindowRect ();

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			this.AdjustWindowRect ();
		}

		protected override void OnLoaderFinished()
		{
			base.OnLoaderFinished();

			if (_spawns.Count < 1)
			{
				this.FindSpawnPlacesInternal();
				this.AdjustWindowRect();
			}
			
		}


		private void FindSpawnPlacesInternal()
		{
			_spawns.Clear();
			_spawnNames.Clear();

			// if exterior is not loaded, then use enexes from loaded interiors
			if (Cell.Instance != null && ! Cell.Instance.HasMainExterior)
			{
				foreach(var enex in Cell.Instance.GetEnexesFromLoadedInteriors())
				{
					_spawns.Add(Cell.Instance.GetEnexExitTransform(enex));
					_spawnNames.Add(enex.Name);
				}
			}
			else
			{
				var spawnPlaces = FindSpawnPlaces ();
				_spawns = spawnPlaces.Select(tr => new TransformDataStruct(tr)).ToList();
				_spawnNames = spawnPlaces.Select(tr => tr.name).ToList();
			}
		}

		public static Transform[] FindSpawnPlaces ()
		{
			var obj = GameObject.Find("Player Spawns");
			if (obj)
				return obj.transform.GetFirstLevelChildren().ToArray();
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

			if (null == Ped.Instance) {
				GUILayout.Label ("No local ped");
				return;
			}


			for (int i = 0; i < _spawns.Count; i++)
			{
				var spawnLocation = _spawns [i];
				
				if (GUILayout.Button (_spawnNames[i]))
				{
					Vector3 pos = spawnLocation.position;
					Vector3 eulers = spawnLocation.rotation.eulerAngles;
					SendCommand($"/teleport {pos.x} {pos.y} {pos.z} {eulers.y}");
				}
			}

		}

		void SendCommand(string command)
		{
			Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(command);
		}

	}

}
