using SanAndreasUnity.Behaviours.Vehicles;
using UnityEngine;

public class VehicleCollider : MonoBehaviour
{
    private Vehicle vehicle;
    private new Collider collider;
    private VehicleBehaviour[] behaviours;

    public static VehicleCollider Init(GameObject gameObject, Vehicle vehicle, VehicleBehaviour[] vehicleBehaviour = null)
    {
        VehicleCollider collider = gameObject.AddComponent<VehicleCollider>();

        if (vehicleBehaviour != null) collider.behaviours = vehicleBehaviour;
        collider.vehicle = vehicle;

        return collider;
    }

    private void Awake ()
    {
        if (behaviours == null) behaviours = FindObjectsOfType<VehicleBehaviour>();
        collider = GetComponent<Collider>();
    }

    // Use this for initialization
    private void Start ()
    {
		
	}
	
	// Update is called once per frame
	private void Update ()
    {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        SendCollision("OnVehicleCollisionEnter", collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        SendCollision("OnVehicleCollisionExit", collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        SendCollision("OnVehicleCollisionStay", collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        SendTrigger("OnVehicleTriggerStay", other);
    }

    private void OnTriggerExit(Collider other)
    {
        SendTrigger("OnVehicleTriggerStay", other);
    }

    private void OnTriggerStay(Collider other)
    {
        SendTrigger("OnVehicleTriggerStay", other);
    }

    private void SendCollision(string method, Collision collision)
    {
        foreach (VehicleBehaviour behaviour in behaviours)
            behaviour.SendMessage(method, collision);
    }

    private void SendTrigger(string method, Collider collider)
    {
        foreach (VehicleBehaviour behaviour in behaviours)
            if(behaviour != null) behaviour.SendMessage(method, collider);
    }
}
