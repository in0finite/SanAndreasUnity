using SanAndreasUnity.Behaviours;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{

    public class UIVehicleSpawner : MonoBehaviour
    {
        public KeyCode spawnKey = KeyCode.V;


        private void Update()
        {
            if (Input.GetKeyDown(spawnKey))
            {
                if (Utilities.NetUtils.IsServer)
                    SpawnVehicle();
                else if (Net.PlayerRequests.Local != null)
                    Net.PlayerRequests.Local.RequestVehicleSpawn(-1);
            }
        }

        private void SpawnVehicle()
        {
    		var ped = Ped.Instance;

    		if (null == ped)
    			return;
            
            Vehicles.Vehicle.CreateRandomInFrontOf(ped.transform);
            
        }

    }

}
