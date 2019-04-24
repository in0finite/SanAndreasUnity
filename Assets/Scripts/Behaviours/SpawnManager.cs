using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{

    public class SpawnManager : MonoBehaviour
    {
        Transform[] m_spawnPositions = null;
        GameObject m_container;


        void Start()
        {
            this.InvokeRepeating(nameof(UpdateSpawnPositions), 1f, 1f);
        }

        void UpdateSpawnPositions()
        {
            if (Net.NetStatus.IsServer && Ped.Instance)
            {
                if (null == m_spawnPositions)
                {
                    // create parent game object for spawn positions
                    if (null == m_container)
                        m_container = new GameObject("Spawn positions");

                    // create spawn positions
                    m_spawnPositions = new Transform[5];
                    for (int i = 0; i < m_spawnPositions.Length; i++)
                    {
                        m_spawnPositions[i] = new GameObject("Spawn position " + i).transform;
                        m_spawnPositions[i].SetParent(m_container.transform);
                        Net.NetManager.AddSpawnPosition(m_spawnPositions[i]);
                    }
                }

                // update spawn positions
                for (int i = 0; i < m_spawnPositions.Length; i++)
                {
                    m_spawnPositions[i].position = Ped.Instance.transform.position + Random.insideUnitCircle.ToVector3XZ() * 15f;
                }

            }
        }

    }

}
