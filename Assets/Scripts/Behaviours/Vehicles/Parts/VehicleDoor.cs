using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Utilities;
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
            Debug.LogFormat("{0} {1}", _isLocked ? "Closing" : "Opening", transform.name);
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
        TextGizmo.Init();

        body = GetComponent<Rigidbody>();
        joint = GetComponent<HingeJoint>();

        _isLocked = true;

        joint.breakForce = 5000 / vehicle.HandlingData.CollisionDamageMult;

        //string prefix = transform.name.Substring(0, 7);

        //Debug.Log(prefix);
        //Debug.Log(transform.Find(string.Format("{0}_ok", prefix)).gameObject == null);

        //okCollider = transform.Find(string.Format("{0}_ok", prefix)).gameObject.AddComponent<NonConvexMeshCollider>();
        //damCollider = transform.Find(string.Format("{0}_dam", prefix)).gameObject.AddComponent<NonConvexMeshCollider>();

        // We have to build the collidrs, but they don't work correctly
    }
	
	// Update is called once per frame
	void Update ()
    {
        //drag = body.angularVelocity.magnitude;

        force = (vehicleBody.mass * .015f) * Mathf.Pow(vehicleBody.angularVelocity.magnitude, 2) * Vector3.Distance(vehicle.transform.TransformPoint(vehicleBody.centerOfMass), transform.position);

        //if (force > 100 && Random.value < lockHealth / 100f)
        //    _isLocked = false;

        // If rotation from the hinge is 0 (== can be closed) block the door
        if (transform.localEulerAngles.y < 1) //&& (1f - Random.value) < lockHealth / 100f
            _isLocked = true;

        if (force > lastForce && allowedToDebug)
        {
            //Debug.Log("NEW MAX: " + drag);
            lastForce = force;
        }
    }

    private void OnDrawGizmos()
    {
        TextGizmo.Draw(transform.position, transform.localEulerAngles.y.ToString("F2"));
    }
}
