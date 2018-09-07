using SanAndreasUnity.Behaviours;
using UnityEngine;

public class UIVehicleSpawner : MonoBehaviour
{
    public Vector3 spawnOffset = new Vector3(0, 2, 5);
    public KeyCode spawnKey = KeyCode.V;

    private PlayerController _playerController;
    private Ped _player;

    // Use this for initialization
    private void Start()
    {
        _playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        _player = GameObject.Find("Player").GetComponent<Ped>();

    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnVehicle();
        }
    }


    public void SpawnVehicle()
    {
        var cont = GameObject.FindObjectOfType<SanAndreasUnity.Behaviours.PlayerController>();

        if (null == cont)
        {
            Debug.LogError("PlayerController component not found - failed to spawn vehicle.");
        }
        else
        {
            Vector3 pos = cont.transform.position + cont.transform.forward * spawnOffset.z + cont.transform.up * spawnOffset.y
                + cont.transform.right * spawnOffset.x;
            Quaternion rotation = Quaternion.LookRotation(-cont.transform.right, Vector3.up);

            //	SanAndreasUnity.Behaviours.Vehicles.VehicleSpawner.Create ();
			var v = SanAndreasUnity.Behaviours.Vehicles.Vehicle.Create(-1, null, pos, rotation);
            Debug.Log("Spawned vehicle with id " + v.Definition.Id);
        }
    }
}