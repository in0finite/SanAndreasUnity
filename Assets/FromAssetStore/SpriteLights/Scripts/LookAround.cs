using UnityEngine;
using System.Collections;

/*
This script handles the in-game camera movement.

Usage:
-Drop this script onto the camera.
*/
public class LookAround : MonoBehaviour {

    public float flySpeed = 3000f;
	private float maxAngle = 80;
	private float sensitivity = 10;
	private Vector3 angles;
	private Vector3 iniAngles;

	[HideInInspector]
	public bool zoomScrollEnabled = true;

	void Start(){

		//Store the orientation of the camera.
		iniAngles = angles = transform.eulerAngles;
	}

	void Update (){

		//Right mouse button pressed.
		if(Input.GetMouseButton(1)){

			//Rotate the camera.
			Look();
		}

		//Move the camera.
		Move();
	}
	
	//Rotate the camera.
	void Look(){

		//Get the mouse movement.
		angles.y += Input.GetAxis("Mouse X") * sensitivity;
		angles.x += -Input.GetAxis("Mouse Y") * sensitivity;

		//Clamp the numbers.
		angles.x = Mathf.Clamp(angles.x, iniAngles.x-maxAngle, iniAngles.x+maxAngle);

		//Set the camera rotation.
		transform.eulerAngles = angles;
	}

	//Move the camera.
	void Move(){

		if(Input.GetKeyDown(KeyCode.Alpha1)){

			flySpeed /= 1.5f;
		}

		if(Input.GetKeyDown(KeyCode.Alpha2)){

			flySpeed *= 1.5f;
		}

		//Middle mouse button pressed.
		if(Input.GetMouseButton(2)){

			//Did the mouse move in X?
			if(Input.GetAxis("Mouse X") != 0){

				//Move the camera.
				transform.Translate(-Vector3.right * flySpeed * 4f * Input.GetAxis("Mouse X") * Time.deltaTime);
			}

			//Did the mouse move in Y?
			if(Input.GetAxis("Mouse Y") != 0){

				//Move the camera.
				transform.Translate(-Vector3.up * flySpeed * 4f * Input.GetAxis("Mouse Y") * Time.deltaTime);
			}
		}

		//Did the mouse move in Y?
		if (Input.GetAxis("Vertical") != 0){

			//Move the camera.
			transform.Translate(Vector3.forward * flySpeed * Input.GetAxis("Vertical") * Time.deltaTime);
		}

		//Did the mouse move in X?
		if (Input.GetAxis("Horizontal") != 0){

			//Move the camera.
			transform.Translate(Vector3.right * flySpeed * Input.GetAxis("Horizontal") * Time.deltaTime);
		}

		//Is the mouse scroll wheel moved?
		if(Input.GetAxis("Mouse ScrollWheel") != 0){

			//Move the camera.
			transform.Translate(Vector3.forward  * flySpeed * 10f * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime);
		}

		//Keyboard "E" key pressed.
		if (Input.GetKey(KeyCode.E)){

			//Move the camera.
			transform.Translate(Vector3.up * flySpeed * Time.deltaTime);
		}

		//Keyboard "Q" key pressed.
		else if (Input.GetKey(KeyCode.Q)){

			//Move the camera.
			transform.Translate(Vector3.down * flySpeed * Time.deltaTime);
		}
	}
}