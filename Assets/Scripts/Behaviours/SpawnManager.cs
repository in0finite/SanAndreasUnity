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

        List<Transform> m_spawnPositions = new List<Transform>();
        GameObject m_container;

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
            UpdateSpawnPositions();
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
            
            this.UpdateSpawnPositions();

            if (this.IsSpawningPaused)
                return;

            if (Time.time - m_lastSpawnTime >= this.spawnInterval)
            {
                // enough time passed
                m_lastSpawnTime = Time.time;
                this.SpawnPlayers();
            }

        }

        void UpdateSpawnPositions()
        {
            if (!Loader.HasLoaded)
                return;
            
            if (!NetStatus.IsServer)
                return;

            Transform focusPos = GetSpawnFocusPos();
            if (null == focusPos)
                return;
            
            if (null == m_container)
            {
                // create parent game object for spawn positions
                m_container = new GameObject("Spawn positions");

                // create spawn positions
                m_spawnPositions.Clear();
                for (int i = 0; i < 5; i++)
                {
                    Transform spawnPos = new GameObject("Spawn position " + i).transform;
                    spawnPos.SetParent(m_container.transform);

                    m_spawnPositions.Add(spawnPos);
                    //NetManager.AddSpawnPosition(spawnPos);
                }
            }

            // update spawn positions
            m_spawnPositions.RemoveDeadObjects();
            foreach (Transform spawnPos in m_spawnPositions)
            {
                spawnPos.position = focusPos.position + Random.insideUnitCircle.ToVector3XZ() * 15f;
            }
            
        }

        void SpawnPlayers()
        {
            if (!NetStatus.IsServer)
                return;

            if (!Loader.HasLoaded)
                return;

            //var list = NetManager.SpawnPositions.ToList();
            var list = m_spawnPositions;
            list.RemoveDeadObjects();

            if (list.Count < 1)
                return;
            
            foreach (var player in Player.AllPlayers)
            {
                SpawnPlayer(player, list);
            }

        }

        public static Ped SpawnPlayer (Player player, List<Transform> spawns)
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

            Debug.LogFormat("Spawned ped for player {0}, net id {1}", player.connectionToClient.address, ped.netId);

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

            m_spawnPositions.RemoveDeadObjects();
            SpawnPlayer(player, m_spawnPositions);

        }

    }

}
