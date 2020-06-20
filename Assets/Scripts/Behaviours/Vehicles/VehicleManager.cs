using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    
    public class VehicleManager : MonoBehaviour
    {
        public static VehicleManager Instance { get; private set; }

        public GameObject vehiclePrefab;

        public float cameraDistanceFromVehicle = 6f;

        public bool syncLinearVelocity = true;
        public bool syncAngularVelocity = true;
        public Utilities.WhenOnClient whenToDisableRigidBody = Utilities.WhenOnClient.OnlyOnOtherClients;
        public bool syncPedTransformWhileInVehicle = false;
        public bool syncVehicleTransformUsingSyncVars = false;
        public bool controlInputOnLocalPlayer = true;
        public bool controlWheelsOnLocalPlayer = true;

        public float vehicleSyncRate = 20;

        public float explosionForceMultiplier = 0.15f;

        public float explosionLeftoverPartsLifetime = 20f;
        public float explosionLeftoverPartsMaxDepenetrationVelocity = 15f;
        public float explosionLeftoverPartsMass = 10f;


        void Awake()
        {
            Instance = this;
        }

    }

}
