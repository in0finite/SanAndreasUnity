using UnityEngine;
using System.Collections;
using SanAndreasUnity.Behaviours;

public class UIVehicleSpawner : MonoBehaviour {

	public	Vector3	spawnOffset = new Vector3( 0, 2, 5 );
	public	KeyCode	spawnKey = KeyCode.V;

	private PlayerController _playerController;
	private Player _player;

	// Use this for initialization
	void Start () {
		_playerController = GameObject.Find ("Player").GetComponent<PlayerController> ();
		_player = GameObject.Find ("Player").GetComponent<Player> ();

		windowRect = new Rect (Screen.width / 2 - 100, 10, 200, 100);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (this.spawnKey)) {
			this.SpawnVehicle ();
		}
	}

	private static Rect windowRect;
	private const int windowID = 2;

	void OnGUI() {
		if (_playerController.CursorLocked)
			return;

		windowRect = GUILayout.Window (windowID, windowRect, spawnWindow, "Utilities");
	}

	void spawnWindow(int windowID) {
		Vector2 pos = new Vector2 (_player.transform.position.x + 3000, 6000 - (_player.transform.position.z + 3000));
		GUILayout.Label ("Pos: X" + (int)pos.x + " Y" + (int)pos.y + " Z" + (int)_player.transform.position.y);

		if (GUILayout.Button("Spawn vehicle")) {
			this.SpawnVehicle ();
		}

		if (GUILayout.Button("Change player model")) {
			CharacterModelChanger.ChangePedestrianModel ();
		}

		GUI.DragWindow();
	}

	public void SpawnVehicle() {
		SanAndreasUnity.Behaviours.PlayerController cont =
			GameObject.FindObjectOfType<SanAndreasUnity.Behaviours.PlayerController> ();

		if (null == cont) {
			Debug.LogError ("PlayerController component not found - failed to spawn vehicle.");
		} else {
			Vector3 pos = cont.transform.position + cont.transform.forward * this.spawnOffset.z + cont.transform.up * this.spawnOffset.y
				+ cont.transform.right * this.spawnOffset.x ;
			Quaternion rotation = Quaternion.LookRotation (-cont.transform.right, Vector3.up);

			//	SanAndreasUnity.Behaviours.Vehicles.VehicleSpawner.Create ();
			SanAndreasUnity.Behaviours.Vehicles.Vehicle v = SanAndreasUnity.Behaviours.Vehicles.Vehicle.Create( -1, null, pos, rotation);
			Debug.Log ("Spawned vehicle with id" + v.Definition.Id);
		}

	}
}
