using UnityEngine;
using Mirror;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;

namespace SanAndreasUnity.Net
{
    public class TransformSyncer
    {
        public enum ClientUpdateType
        {
            ConstantVelocity,
            Lerp,
            Slerp,
        }

        [System.Serializable]
        public struct Parameters
        {
            public bool useSmoothDeltaTime;

            public ClientUpdateType clientUpdateType;

            public float constantVelocityMultiplier;

            public float lerpFactor;

            public static Parameters Default => new Parameters
            {
                useSmoothDeltaTime = true,
                clientUpdateType = ClientUpdateType.ConstantVelocity,
                constantVelocityMultiplier = 1f,
                lerpFactor = 30f,
            };
        }

        private Parameters m_parameters = Parameters.Default;

        private readonly Queue<SyncData> m_syncDataQueue = new Queue<SyncData>();
        public int NumSyncDatasInQueue => m_syncDataQueue.Count;

        private SyncData m_currentSyncData = new SyncData { Rotation = Quaternion.identity };
        public SyncData CurrentSyncData => m_currentSyncData;

        private Vector3 m_positionForSending;
        private Quaternion m_rotationForSending = Quaternion.identity;

        private readonly Transform m_transform;
        public Transform Transform => m_transform;

        private readonly Rigidbody m_rigidbody;

        public struct SyncData
        {
            // sync data (as reported by server) toward which we move the transform
            public Vector3 Position;
            public Quaternion Rotation;

            // these are the velocities used to move the object, calculated when new server data arrives
            public float CalculatedVelocityMagnitude;
            public float CalculatedAngularVelocityMagnitude;
        }

        private readonly bool m_hasTransform = false;
        private readonly bool m_hasRigidBody = false;

        private readonly NetworkBehaviour m_networkBehaviour;



        public TransformSyncer(Transform tr, Parameters parameters, NetworkBehaviour networkBehaviour)
        {
            m_transform = tr;
            m_rigidbody = tr != null ? tr.GetComponent<Rigidbody>() : null;
            m_parameters = parameters;
            m_networkBehaviour = networkBehaviour;
            m_hasTransform = tr != null;
            m_hasRigidBody = m_rigidbody != null;

            this.AssignDataForSending();
        }

        public void OnStartClient()
        {
            if (NetUtils.IsServer)
                return;

            // apply initial sync data
            // first sync should've been done before calling this function, so the data is available
            this.ApplyCurrentSyncData();
        }

        public bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            byte flags = 0;
            writer.Write(flags);

            writer.Write(m_positionForSending);
            writer.Write(m_rotationForSending.eulerAngles);

            return true;
        }

        public void OnDeserialize(NetworkReader reader, bool initialState)
        {
            byte flags = reader.ReadByte();

            var syncData = new SyncData();

            syncData.Position = reader.ReadVector3();// + syncData.velocity * this.syncInterval;
            syncData.Rotation = Quaternion.Euler(reader.ReadVector3());

            if (initialState)
                m_currentSyncData = syncData;
            else
                this.Enqueue(syncData);
        }

        private void Enqueue(SyncData syncData)
        {
            if (m_syncDataQueue.Count >= 4)
                m_syncDataQueue.Dequeue();
            m_syncDataQueue.Enqueue(syncData);
        }

        public void Update()
        {
            if (!m_hasTransform)
                return;

            if (NetUtils.IsServer)
            {
                m_networkBehaviour.SetSyncVarDirtyBit(1);

                this.AssignDataForSending();
            }
            else
            {
                switch (m_parameters.clientUpdateType)
                {
                    case ClientUpdateType.ConstantVelocity:
                        this.UpdateClientUsingConstantVelocity();
                        break;
                    case ClientUpdateType.Lerp:
                        this.UpdateClientUsingLerp();
                        break;
                    case ClientUpdateType.Slerp:
                        this.UpdateClientUsingSphericalLerp();
                        break;
                    default:
                        break;
                }

                // check if we reached current snapshot
                if (Vector3.SqrMagnitude(m_transform.localPosition - m_currentSyncData.Position) < 0.001f
                    && Quaternion.Angle(m_transform.localRotation, m_currentSyncData.Rotation) < 1f)
                {
                    // current snapshot reached, switch to next one
                    if (m_syncDataQueue.Count > 0)
                    {
                        var newSyncData = m_syncDataQueue.Dequeue();
                        // calculate velocities
                        newSyncData.CalculatedVelocityMagnitude = (newSyncData.Position - m_currentSyncData.Position).magnitude / m_networkBehaviour.syncInterval;
                        newSyncData.CalculatedAngularVelocityMagnitude = Quaternion.Angle(newSyncData.Rotation, m_currentSyncData.Rotation) / m_networkBehaviour.syncInterval;
                        m_currentSyncData = newSyncData;
                    }
                }
            }
        }

        private void UpdateClientUsingConstantVelocity()
        {
            var syncInfo = m_currentSyncData;

            float moveDelta = syncInfo.CalculatedVelocityMagnitude * this.GetDeltaTime() * m_parameters.constantVelocityMultiplier;

            float distanceSqr = (m_transform.localPosition - syncInfo.Position).sqrMagnitude;

            if (moveDelta < float.Epsilon || distanceSqr < float.Epsilon || Mathf.Sqrt(distanceSqr) < float.Epsilon)
                m_transform.localPosition = syncInfo.Position;
            else
                m_transform.localPosition = Vector3.MoveTowards(
                    m_transform.localPosition,
                    syncInfo.Position,
                    moveDelta);

            m_transform.localRotation = Quaternion.RotateTowards(
                m_transform.localRotation,
                syncInfo.Rotation,
                syncInfo.CalculatedAngularVelocityMagnitude * this.GetDeltaTime() * m_parameters.constantVelocityMultiplier);

        }

        private void UpdateClientUsingLerp()
        {
            m_transform.localPosition = Vector3.Lerp(
                m_transform.localPosition,
                m_currentSyncData.Position,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime()));

            m_transform.localRotation = Quaternion.Lerp(
                m_transform.localRotation,
                m_currentSyncData.Rotation,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime()));

        }

        private void UpdateClientUsingSphericalLerp()
        {
            m_transform.localPosition = Vector3.Slerp(
                m_transform.localPosition,
                m_currentSyncData.Position,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime()));

            m_transform.localRotation = Quaternion.Slerp(
                m_transform.localRotation,
                m_currentSyncData.Rotation,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime()));

        }

        private float GetDeltaTime()
        {
            return m_parameters.useSmoothDeltaTime ? Time.smoothDeltaTime : Time.deltaTime;
        }

        public void OnValidate(Parameters parameters)
        {
            m_parameters = parameters;
        }

        public void ResetSyncInfoToTransform()
        {
            if (m_hasTransform)
            {
                m_currentSyncData.Position = m_transform.localPosition;
                m_currentSyncData.Rotation = m_transform.localRotation;
            }
            m_currentSyncData.CalculatedVelocityMagnitude = 0;
            m_currentSyncData.CalculatedAngularVelocityMagnitude = 0;

            m_syncDataQueue.Clear();
        }

        private void ApplyCurrentSyncData()
        {
            if (!m_hasTransform)
                return;

            m_transform.localPosition = m_currentSyncData.Position;
            m_transform.localRotation = m_currentSyncData.Rotation;
        }

        private void AssignDataForSending()
        {
            if (!m_hasTransform)
                return;

            m_positionForSending = m_hasRigidBody ? m_rigidbody.position : m_transform.localPosition;
            m_rotationForSending = m_hasRigidBody ? m_rigidbody.rotation : m_transform.localRotation;
        }
    }
}
