using SanAndreasUnity.Behaviours;
using UnityEngine;

public class UIVehicleSpawner : MonoBehaviour
{
    public Vector3 spawnOffset = new Vector3(0, 2, 5);
    public KeyCode spawnKey = KeyCode.V;


    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            if (Utilities.NetUtils.IsServer)
                SpawnVehicle();
            else if (Net.PlayerRequests.Local != null)
                Net.PlayerRequests.Local.RequestVehicleSpawn();
        }
    }


    public void SpawnVehicle()
    {
		var ped = Ped.Instance;

		if (null == ped)
			return;
        
        SpawnVehicle(ped);
        
    }

    public void SpawnVehicle(Ped ped)
    {
        
        Vector3 pos = ped.transform.position + ped.transform.forward * spawnOffset.z + ped.transform.up * spawnOffset.y
            + ped.transform.right * spawnOffset.x;
        Quaternion rotation = Quaternion.LookRotation(-ped.transform.right, Vector3.up);

        SpawnVehicle(pos, rotation);

    }

    public void SpawnVehicle(Vector3 pos, Quaternion rotation)
    {
        //  SanAndreasUnity.Behaviours.Vehicles.VehicleSpawner.Create ();
        var v = SanAndreasUnity.Behaviours.Vehicles.Vehicle.Create(-1, null, pos, rotation);
        Debug.Log("Spawned vehicle with id " + v.Definition.Id);
    }

}