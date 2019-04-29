using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class VehicleManager : MonoBehaviour
    {
        public static VehicleManager Instance { get; private set; }

        public GameObject vehiclePrefab;


        void Awake()
        {
            Instance = this;
        }

    }

}
