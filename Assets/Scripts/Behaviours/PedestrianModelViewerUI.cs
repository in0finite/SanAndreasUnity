using UnityEngine;
using System.Collections;
using SanAndreasUnity.Behaviours;

public class PedestrianModelViewerUI : MonoBehaviour {

	public	Pedestrian	pedestrian = null ;



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void	OnGUI() {

		if (null == pedestrian)
			return;

		if (GUILayout.Button ("Next")) {
			pedestrian.PedestrianId++;
		}

		if (GUILayout.Button ("Previous")) {
			int newId = pedestrian.PedestrianId - 1;
			if (newId < 0)
				newId = 0;
			pedestrian.PedestrianId = newId;
		}


	}

}
