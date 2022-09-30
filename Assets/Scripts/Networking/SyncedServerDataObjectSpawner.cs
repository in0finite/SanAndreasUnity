using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanAndreasUnity.Net
{
    public class SyncedServerDataObjectSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject m_syncedServerDataPrefab = null;


        private void Awake()
        {
            m_syncedServerDataPrefab.GetComponentOrLogError<SyncedServerData>();
        }

        private void Start()
        {
            NetManager.Instance.onServerStatusChanged += OnServerStatusChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnServerStatusChanged()
        {
            // scene is not changed yet, so the object will get destroyed when it changes
            //Spawn();
        }

        private void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            Spawn();
        }

        private void Spawn()
        {
            if (!NetStatus.IsServer)
                return;

            if (null != SyncedServerData.Instance)
                return;

            var go = Instantiate(m_syncedServerDataPrefab);
            NetManager.Spawn(go);
        }
    }
}
