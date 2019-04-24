using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{

    public class SpawnManager : MonoBehaviour
    {
        List<Transform> m_spawnPositions = new List<Transform>();
        GameObject m_container;


        void Start()
        {
            this.InvokeRepeating(nameof(UpdateSpawnPositions), 1f, 1f);
            this.InvokeRepeating(nameof(SpawnPlayers), 1f, 3f);
        }

        Transform GetSpawnFocusPos()
        {
            if (Ped.Instance)
                return Ped.Instance.transform;

            if (Camera.main)
                return Camera.main.transform;

            return null;
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
                // if player is not spawned, spawn him

                if (player.OwnedGameObject != null)
                    continue;

                var spawn = list.RandomElement();
                var ped = Ped.SpawnPed(Ped.RandomPedId, spawn.position, spawn.rotation);
                player.OwnedGameObject = ped.gameObject;

                Debug.LogFormat("Spawned ped for player {0}, net id {1}", player.connectionToClient.address, ped.netId);
            }

        }

    }

}
