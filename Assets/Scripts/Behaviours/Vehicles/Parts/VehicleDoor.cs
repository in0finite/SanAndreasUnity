using SanAndreasUnity.Behaviours.Vehicles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleDoor : MonoBehaviour
{

    public float drag;

    private float lastForce;
    private bool allowedToDebug;
    private Rigidbody body, vehicleBody;
    private Vehicle vehicle;

    public static VehicleDoor InitializateDoor(Transform door, Vehicle vehicle)
    {
        VehicleDoor doorObj = door.gameObject.AddComponent<VehicleDoor>();

        doorObj.vehicle = vehicle;
        doorObj.vehicleBody = vehicle.GetComponent<Rigidbody>();

        doorObj.allowedToDebug = doorObj.name.Contains("_lf_");

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
        //drag = body.angularVelocity.magnitude;

        drag = body.mass * Mathf.Pow(vehicleBody.angularVelocity.magnitude, 2) * Vector3.Distance(vehicle.transform.TransformPoint(vehicleBody.centerOfMass), transform.position);

        if (drag > lastForce && allowedToDebug)
        {
            Debug.Log("NEW MAX: " + drag);
            lastForce = drag;
        }
    }
}
