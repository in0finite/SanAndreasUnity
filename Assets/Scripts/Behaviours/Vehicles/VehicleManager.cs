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

        public float explosionForceMultiplier = 700f;
        public float explosionChassisForceMultiplier = 11000f;
        public float explosionDamageRadius = 7f;
        public AnimationCurve explosionDamageOverDistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [Range(0.1f, 3f)] public float explosionMassToDamageExponent = 1f;

        public float explosionLeftoverPartsLifetime = 20f;
        public float explosionLeftoverPartsMaxDepenetrationVelocity = 15f;
        public float explosionLeftoverPartsMass = 100f;

        public GameObject smokePrefab;
        public GameObject flamePrefab;
        public GameObject explosionPrefab;



        void Awake()
        {
            Instance = this;
        }

    }

}
