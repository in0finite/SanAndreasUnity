using UnityEngine;
using System.Collections;
using SanAndreasUnity.Behaviours;

public class CharacterModelChanger : MonoBehaviour {

	public	KeyCode	actionKey = KeyCode.P;



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyDown (this.actionKey)) {
			ChangePedestrianModel ();
		}

	}

	public	static	void	ChangePedestrianModel() {

		SanAndreasUnity.Behaviours.Pedestrian ped =
			GameObject.FindObjectOfType<SanAndreasUnity.Behaviours.Pedestrian> ();

		if (ped != null) {
			ChangePedestrianModel (ped, -1);
		}

	}

	public	static	void	ChangePedestrianModel(Pedestrian ped, int newModelId) {

		// model id range: 9 - 288

		if (-1 == newModelId)
			newModelId = Random.Range (9, 289);

		if (newModelId < 9 || newModelId > 288) {
			return;
		}

		// Retry with another random model if this one doesn't work
		try {
			ped.Load (newModelId);
		} catch (System.NullReferenceException) {
			ChangePedestrianModel (ped, -1);
		}
	}
}
