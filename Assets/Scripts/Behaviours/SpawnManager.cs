using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Net;
using UGameCore.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{

    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }

        public bool spawnPlayerWhenConnected = true;
        public bool IsSpawningPaused { get; set; } = false;
        public float spawnInterval = 4f;
        double m_lastSpawnTime = 0;

        public bool addWeaponsToSpawnedPlayers = true;

        public static SpawnHandler DefaultSpawnHandler { get; } = new SpawnHandler();

        private SpawnHandler m_spawnHandler = DefaultSpawnHandler;
        public SpawnHandler SpawnHandler
        {
            get => m_spawnHandler;
            set
            {
                if (value == null)
                    value = DefaultSpawnHandler;
                m_spawnHandler = value;
            }
        }



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

        public static Transform GetSpawnFocusPos()
        {
            var ped = Ped.Instance;
            if (ped != null)
                return ped.transform;

            var cam = Camera.main;
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

            if (Time.timeAsDouble - m_lastSpawnTime >= this.spawnInterval)
            {
                // enough time passed
                m_lastSpawnTime = Time.timeAsDouble;

                F.RunExceptionSafe(() => this.SpawnPlayers());
            }

        }

        public static bool GetSpawnPositionFromFocus(Transform focusPos, out TransformDataStruct transformData)
        {
            if (Player.AllPlayersList.Count <= 1)
                // if there is only 1 player, always spawn him on the same place - no randomization
                transformData = new TransformDataStruct(focusPos);
            else
                transformData = new TransformDataStruct(
                    focusPos.position + Random.insideUnitCircle.ToVec3XZ() * 15f,
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

            return true;
        }

        public static bool GetSpawnPositionFromFocus(out TransformDataStruct transformData)
        {
            Transform focusPos = GetSpawnFocusPos();
            if (null == focusPos)
            {
                transformData = new TransformDataStruct();
                return false;
            }

            return GetSpawnPositionFromFocus(focusPos, out transformData);
        }

        public static bool GetSpawnPositionFromInteriors(out TransformDataStruct transformData)
        {
            transformData = World.Cell.Instance.GetEnexExitTransform(
                World.Cell.Instance.GetEnexesFromLoadedInteriors()
                .ToList()
                .RandomElement());
            return true;
        }

        bool GetSpawnPositionFromHandler(Player player, out TransformDataStruct transformData)
        {
            bool success = false;
            TransformDataStruct t = new TransformDataStruct();

            F.RunExceptionSafe(() =>
            {
                success = this.SpawnHandler.GetSpawnPosition(player, out t);
            });

            transformData = t;
            return success;
        }

        void SpawnPlayers()
        {
            if (!NetStatus.IsServer)
                return;

            if (!Loader.HasLoaded)
                return;

            TransformDataStruct transformData;

            foreach (var player in Player.AllPlayersCopy)
            {
                if (this.GetSpawnPositionFromHandler(player, out transformData))
                    SpawnPlayer(player, transformData);
                else
                    break;
            }

        }

        public Ped SpawnPlayer (Player player, TransformDataStruct spawnPlace)
        {
            if (player.OwnedPed != null)
                return null;

            var ped = Ped.SpawnPed(Ped.RandomPedId, spawnPlace.position, spawnPlace.rotation, false);
            ped.NetPlayerOwnerGameObject = player.gameObject;
            if (this.addWeaponsToSpawnedPlayers)
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

            if (this.GetSpawnPositionFromHandler(player, out TransformDataStruct transformData))
                SpawnPlayer(player, transformData);

        }

    }

}
