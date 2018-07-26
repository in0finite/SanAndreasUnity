using SanAndreasUnity.Behaviours.Vehicles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleDoor : MonoBehaviour
{
    private bool _isLocked;
    public bool isLocked
    {
        get
        {
            return _isLocked;
        }

        set
        {
            _isLocked = value;
            body.constraints = _isLocked ? RigidbodyConstraints.FreezeRotationY : RigidbodyConstraints.None;
        }
    }

    public float force;

    private float lastForce;
    private bool allowedToDebug;
    private Rigidbody body, vehicleBody;
    private Vehicle vehicle;

    private float lockHealth = 100;
    private HingeJoint joint;
    //private MeshCollider okCollider, damCollider;
    private NonConvexMeshCollider okCollider, damCollider;

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
        joint = GetComponent<HingeJoint>();

        _isLocked = true;

        joint.breakForce = 5000 / vehicle.HandlingData.CollisionDamageMult;

        string prefix = transform.name.Substring(0, 7);

        okCollider = transform.Find(string.Format("{0}_ok", prefix)).gameObject.AddComponent<NonConvexMeshCollider>();
        damCollider = transform.Find(string.Format("{0}_dam", prefix)).gameObject.AddComponent<NonConvexMeshCollider>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        //drag = body.angularVelocity.magnitude;

        force = (vehicleBody.mass * .015f) * Mathf.Pow(vehicleBody.angularVelocity.magnitude, 2) * Vector3.Distance(vehicle.transform.TransformPoint(vehicleBody.centerOfMass), transform.position);

        if (force > 100 && Random.value < lockHealth / 100f)
            _isLocked = false;

        // If rotation from the hinge is 0 (== can be closed) block the door

        if (force > lastForce && allowedToDebug)
        {
            //Debug.Log("NEW MAX: " + drag);
            lastForce = force;
        }
    }
}
