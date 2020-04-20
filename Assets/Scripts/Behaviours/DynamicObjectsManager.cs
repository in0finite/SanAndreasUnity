using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public struct DynamicObjectProperties
    {
        public float mass;
        // collision impulse which will break that model
        public uint breakImpulse;
        public uint health;

        // if enabled, ped or vehicle can destroy it by colliding with that object
        public bool canBeCrashedByPed;
        public bool canBeCrashedByVehicle;

        // if enabled, shooting can destroy that object, otherwise it will just move
        public bool canBeShooted;
    }

    public class DynamicObjectsManager : MonoBehaviour
    {
        public static DynamicObjectsManager Instance { get; private set; }

        [Tooltip("Time after breakable model get respawn")]
        [SerializeField] private float m_respawnTime = 30.0f;
        public float RespawnTime { get { return m_respawnTime; } set { m_respawnTime = value; } }

        // https://dev.prineside.com/en/gtasa_samp_model_id/search/?q=1265
        private Dictionary<int, DynamicObjectProperties> m_dynamicObjects = new Dictionary<int, DynamicObjectProperties>
        {
            [1265] = new DynamicObjectProperties
            {
                mass = 10.0f,
                breakImpulse = 1000,
                health = 50,
                canBeCrashedByPed = true,
                canBeCrashedByVehicle = true,
                canBeShooted = true,
            },
            [1230] = new DynamicObjectProperties
            {
                mass = 20.0f,
                breakImpulse = 1500,
                health = 75,
                canBeCrashedByPed = true,
                canBeCrashedByVehicle = true,
                canBeShooted = true,
            },
            [1220] = new DynamicObjectProperties
            {
                mass = 20.0f,
                breakImpulse = 2000,
                health = 100,
                canBeCrashedByPed = true,
                canBeCrashedByVehicle = true,
                canBeShooted = true,
            },
            [1221] = new DynamicObjectProperties
            {
                mass = 20.0f,
                breakImpulse = 1500,
                health = 75,
                canBeCrashedByPed = true,
                canBeCrashedByVehicle = true,
                canBeShooted = true,
            },
            [1370] = new DynamicObjectProperties // barrel
            {
                mass = 50.0f,
                breakImpulse = 5000,
                health = 300,
                canBeCrashedByPed = false,
                canBeCrashedByVehicle = true,
                canBeShooted = true,
            },
        };

        public bool IsModelDynamic(int model)
        {
            return m_dynamicObjects.ContainsKey(model);
        }
        
        public DynamicObjectProperties? GetModelProperties(int model)
        {
            if (m_dynamicObjects.TryGetValue(model, out DynamicObjectProperties value))
                return value;
            return null;
        }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {

        }
    }

}
