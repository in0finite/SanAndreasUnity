using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReferenceDistance : MonoBehaviour {


	private Text speedCanvasText;
	private Text distanceCanvasText;

	private LookAround lookAround;

	void Start () {	

        GameObject textObject = GameObject.Find("speed");

		if(textObject != null){

			speedCanvasText = textObject.GetComponent<Text>();
		}

        textObject = GameObject.Find("distance");

		if(textObject != null){

			distanceCanvasText = textObject.GetComponent<Text>();
		}

		lookAround = Camera.main.gameObject.transform.parent.GetComponent<LookAround>();
	}
	
	void Update () {
	
		if(distanceCanvasText != null){

			//Calculate the distance to the reference object.
			float distance = Vector3.Distance(gameObject.transform.position, Camera.main.transform.position);

			//Unit conversion
			float km = distance / 1000f;
			float nm = distance * 0.000539956803f;
			string kmString = km.ToString("0.00");
			string nmString = nm.ToString("0.00");

			//Compile the string.
			string text = "DME 25L: " + nmString + " nm, " + kmString + " km";

			//Output the text on the screen.
			distanceCanvasText.text = text;
			speedCanvasText.text = "Slew speed: " + (int)lookAround.flySpeed;
		}
	}
}
