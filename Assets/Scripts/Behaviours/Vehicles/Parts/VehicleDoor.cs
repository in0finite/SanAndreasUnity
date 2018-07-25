using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleDoor : MonoBehaviour
{

    public float drag;

    private Rigidbody body;

    public static VehicleDoor InitializateDoor(Transform door)
    {
        VehicleDoor doorObj = door.gameObject.AddComponent<VehicleDoor>();

        return doorObj;
    }

	// Use this for initialization
	void Start ()
    {
        body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        drag = body.drag;
	}
}
