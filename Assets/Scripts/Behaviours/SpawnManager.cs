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

            var cam = Camera.current;
            if (cam != null)
                return cam.transform;

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

                F.RunExceptionSafe(() => this.SpawnPlayers());
            }

        }

        bool GetSpawnPositionFromFocus(out TransformDataStruct transformData)
        {
            Transform focusPos = GetSpawnFocusPos();
            if (null == focusPos)
            {
                transformData = new TransformDataStruct();
                return false;
            }

            transformData = new TransformDataStruct(
                focusPos.position + Random.insideUnitCircle.ToVector3XZ() * 15f,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

            return true;
        }

        bool GetSpawnPositionFromInteriors(out TransformDataStruct transformData)
        {
            transformData = World.Cell.GetEnexExitTransform(
                World.Cell.Instance.GetEnexesFromLoadedInteriors()
                .ToList()
                .RandomElement());
            return true;
        }

        public bool GetSpawnPosition(out TransformDataStruct transformData)
        {
            if (null == World.Cell.Instance || World.Cell.Instance.HasExterior)
                return GetSpawnPositionFromFocus(out transformData);
            else
                return GetSpawnPositionFromInteriors(out transformData);
        }

        void SpawnPlayers()
        {
            if (!NetStatus.IsServer)
                return;

            if (!Loader.HasLoaded)
                return;

            TransformDataStruct transformData;

            foreach (var player in Player.AllPlayers)
            {
                if (this.GetSpawnPosition(out transformData))
                    SpawnPlayer(player, transformData);
                else
                    break;
            }

        }

        public static Ped SpawnPlayer (Player player, TransformDataStruct spawnPlace)
        {
            if (player.OwnedPed != null)
                return null;

            var ped = Ped.SpawnPed(Ped.RandomPedId, spawnPlace.position, spawnPlace.rotation, false);
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

            if (this.GetSpawnPosition(out TransformDataStruct transformData))
                SpawnPlayer(player, transformData);

        }

    }

}
