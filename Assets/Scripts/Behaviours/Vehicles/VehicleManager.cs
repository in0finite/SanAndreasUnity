﻿using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    
    public class VehicleManager : MonoBehaviour
    {
        public static VehicleManager Instance { get; private set; }

        public GameObject vehiclePrefab;

        public float cameraDistanceFromVehicle = 6f;

        public RigidbodyInterpolation rigidbodyInterpolationOnServer = RigidbodyInterpolation.None;
        public RigidbodyInterpolation rigidbodyInterpolationOnClient = RigidbodyInterpolation.None;

        public CollisionDetectionMode rigidBodyCollisionDetectionMode = CollisionDetectionMode.Discrete;

        public bool destroyRigidBodyOnClients = true;
        public bool syncPedTransformWhileInVehicle = false;
        public bool controlInputOnLocalPlayer = true;
        public bool controlWheelsOnLocalPlayer = true;
        public bool destroyWheelCollidersOnClient = true;

        public float vehicleSyncRate = 20;

        [Range(0.1f, 3f)] public float massToHealthExponent = 1f;

        public float explosionDamageRadius = 7f;
        public AnimationCurve explosionDamageOverDistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [Range(0.1f, 3f)] public float explosionMassToDamageExponent = 1f;

        public GameObject explosionLeftoverPartPrefab;
        public float explosionLeftoverPartsLifetime = 20f;
        public float explosionLeftoverPartsMaxDepenetrationVelocity = 15f;
        public float explosionLeftoverPartsMass = 100f;

        public GameObject smokePrefab;
        public GameObject flamePrefab;
        public GameObject explosionPrefab;

        [Range(0f, 1f)] public float radioVolume = 1f;



        void Awake()
        {
            Instance = this;
        }

    }

}
