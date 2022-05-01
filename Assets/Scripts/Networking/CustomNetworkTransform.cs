using Mirror;
using UnityEngine;

namespace SanAndreasUnity.Net
{
    public class CustomNetworkTransform : NetworkBehaviour
    {
        private TransformSyncer m_transformSyncer;
        public TransformSyncer TransformSyncer => m_transformSyncer;

        [SerializeField]
        private Transform m_transformToSync;

        [SerializeField]
        private TransformSyncer.Parameters m_transformSyncParameters = TransformSyncer.Parameters.Default;



        void Awake()
        {
            m_transformSyncer = new TransformSyncer(m_transformToSync, m_transformSyncParameters, this);
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            return m_transformSyncer.OnSerialize(writer, initialState);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            m_transformSyncer.OnDeserialize(reader, initialState);
        }

        private void Update()
        {
            m_transformSyncer.Update();
        }

        private void OnValidate()
        {
            m_transformSyncer?.OnValidate(m_transformSyncParameters);
        }

        public void ChangeSyncedTransform(Transform newTransform)
        {
            m_transformToSync = newTransform;

            m_transformSyncer = new TransformSyncer(newTransform, m_transformSyncParameters, this);
            m_transformSyncer.ResetSyncDataToTransform();
        }
    }
}
