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

        void UpdateSpawnPositions()
        {
            if (NetStatus.IsServer && Ped.Instance)
            {
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
                        NetManager.AddSpawnPosition(spawnPos);
                    }
                }

                // update spawn positions
                m_spawnPositions.RemoveDeadObjects();
                foreach (Transform spawnPos in m_spawnPositions)
                {
                    spawnPos.position = Ped.Instance.transform.position + Random.insideUnitCircle.ToVector3XZ() * 15f;
                }

            }
        }

        void SpawnPlayers()
        {
            if (!NetStatus.IsServer)
                return;

            var list = NetManager.SpawnPositions.ToList();
            list.RemoveDeadObjects();

            if (list.Count < 1)
                return;
            
            foreach (var player in Player.AllPlayers)
            {
                // if player is not spawned, spawn him

                

                var spawn = list.RandomElement();

            }

        }

    }

}
