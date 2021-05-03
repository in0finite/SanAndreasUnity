using Mirror;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Net
{
    public class SyncedServerData : NetworkBehaviour
    {
        public static SyncedServerData Instance { get; private set; }

        SyncedBag.StringSyncDictionary _syncDictionary = new SyncedBag.StringSyncDictionary();

        public static SyncedBag Data { get; private set; } = new SyncedBag(new SyncedBag.StringSyncDictionary());

        public static event System.Action onInitialSyncDataAvailable = delegate {};



        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"{nameof(SyncedServerData)} object already exists. There should only be 1.");
                Destroy(this.gameObject);
                return;
            }

            Instance = this;

            var oldData = Data;

            if (NetStatus.IsServer)
            {
                var newData = new SyncedBag(_syncDictionary);
                newData.SetData(oldData);
                newData.SetCallbacks(oldData);

                Data = newData;
            }
            else
            {
                var newData = new SyncedBag(_syncDictionary);
                newData.SetCallbacks(oldData);
                Data = newData;
            }

        }

        private void OnDisable()
        {
            // clear data for next server start
            Data = new SyncedBag(new SyncedBag.StringSyncDictionary());
        }

        public override void OnStartClient()
        {
            if (NetStatus.IsServer)
                return;

            F.InvokeEventExceptionSafe(onInitialSyncDataAvailable);
        }
    }
}
