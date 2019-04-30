using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class VehicleManager : MonoBehaviour
    {
        public static VehicleManager Instance { get; private set; }

        public GameObject vehiclePrefab;

        public bool syncLinearVelocity = true;
        public bool syncAngularVelocity = true;


        void Awake()
        {
            Instance = this;
        }

    }

}
