using UnityEngine;
using System.Collections;

public class UIVehicleSpawner : MonoBehaviour {

	public	Vector3	spawnOffset = new Vector3( 0, 2, 5 );
	public	KeyCode	spawnKey = KeyCode.V;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (this.spawnKey)) {
			this.SpawnVehicle ();
		}
	}

	void OnGUI() {
		if (!Cursor.visible)
			return;

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();

		if( GUILayout.Button("Spawn vehicle") ) {
			this.SpawnVehicle ();
		}

		GUILayout.EndHorizontal();
	}

	public void SpawnVehicle() {
		SanAndreasUnity.Behaviours.PlayerController cont =
			GameObject.FindObjectOfType<SanAndreasUnity.Behaviours.PlayerController> ();

		if (null == cont) {
			Debug.LogError ("PlayerController component not found - failed to spawn vehicle.");
		} else {
			Vector3 pos = cont.transform.position + cont.transform.forward * this.spawnOffset.z + cont.transform.up * this.spawnOffset.y
				+ cont.transform.right * this.spawnOffset.x ;
			Quaternion rotation = Quaternion.LookRotation (cont.transform.right, Vector3.up);

			//	SanAndreasUnity.Behaviours.Vehicles.VehicleSpawner.Create ();
			SanAndreasUnity.Behaviours.Vehicles.Vehicle v = SanAndreasUnity.Behaviours.Vehicles.Vehicle.Create( -1, null, pos, rotation);
			Debug.Log ("Spawned vehicle with id" + v.Definition.Id);
		}

	}
}
