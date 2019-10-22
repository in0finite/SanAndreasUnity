using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{

    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }

        public bool spawnPlayerWhenConnected = true;
        public bool IsSpawningPaused { get; set; } = false;
        public float spawnInterval = 4f;
        float m_lastSpawnTime = 0f;



        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            this.InvokeRepeating(nameof(RepeatedMethod), 1f, 1f);
            Player.onStart += OnPlayerConnected;
        }

        void OnLoaderFinished()
        {
            if (!NetStatus.IsServer)
                return;
            
            // spawn players that were connected during loading process - this will always be the case for
            // local player
            SpawnPlayers();

        }

        Transform GetSpawnFocusPos()
        {
            if (Ped.Instance)
                return Ped.Instance.transform;

            if (Camera.main)
                return Camera.main.transform;

            return null;
        }

        void RepeatedMethod()
        {
            if (!NetStatus.IsServer)
                return;
            
            if (this.IsSpawningPaused)
                return;

            if (Time.time - m_lastSpawnTime >= this.spawnInterval)
            {
                // enough time passed
                m_lastSpawnTime = Time.time;
                this.SpawnPlayers();
            }

        }

        public List<TransformDataStruct> GetSpawnPositions()
        {
            var spawns = new List<TransformDataStruct>();

            Transform focusPos = GetSpawnFocusPos();
            if (null == focusPos)
                return spawns;
            
            for (int i=0; i < 5; i++)
            {
                var transformData = new TransformDataStruct(focusPos.position + Random.insideUnitCircle.ToVector3XZ() * 15f);
                spawns.Add(transformData);
            }

            return spawns;
        }

        void SpawnPlayers()
        {
            if (!NetStatus.IsServer)
                return;

            if (!Loader.HasLoaded)
                return;

            var list = GetSpawnPositions();
            
            if (list.Count < 1)
                return;
            
            foreach (var player in Player.AllPlayers)
            {
                SpawnPlayer(player, list);
            }

        }

        public static Ped SpawnPlayer (Player player, List<TransformDataStruct> spawns)
        {
            if (player.OwnedPed != null)
                return null;

            var spawn = spawns.RandomElement();
            var ped = Ped.SpawnPed(Ped.RandomPedId, spawn.position, spawn.rotation, false);
            ped.NetPlayerOwnerGameObject = player.gameObject;
            ped.WeaponHolder.autoAddWeapon = true;
            // this ped should not be destroyed when he gets out of range
            ped.gameObject.DestroyComponent<OutOfRangeDestroyer>();

            NetManager.Spawn(ped.gameObject);

            player.OwnedPed = ped;

            Debug.LogFormat("Spawned ped {0} for player {1}, time: {2}", ped.DescriptionForLogging, player.DescriptionForLogging, 
                F.CurrentDateForLogging);

            return ped;
        }

        void OnPlayerConnected(Player player)
        {
            if (!NetStatus.IsServer)
                return;
            if (!Loader.HasLoaded)  // these players will be spawned when loading process finishes
                return;
            if (!this.spawnPlayerWhenConnected)
                return;

            SpawnPlayer(player, GetSpawnPositions());

        }

    }

}
