using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class VehicleManager : MonoBehaviour
    {
        public static VehicleManager Instance { get; private set; }

        public GameObject vehiclePrefab;

        public bool syncLinearVelocity = true;
        public bool syncAngularVelocity = true;

        public bool disableRigidBodyOnClients = true;

        public bool syncPedTransformWhileInVehicle = false;

        public bool syncVehicleTransformUsingSyncVars = false;

        public float vehicleSyncRate = 20;


        void Awake()
        {
            Instance = this;
        }

    }

}
