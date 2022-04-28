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
                if (Vector3.SqrMagnitude(this.GetPosition() - m_currentSyncData.Position) < 0.01f
                    && Quaternion.Angle(this.GetRotation(), m_currentSyncData.Rotation) < 1f)
                {
                    this.SetPosition();
                    this.SetRotation();

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

            float distanceSqr = (this.GetPosition() - syncInfo.Position).sqrMagnitude;

            if (moveDelta < float.Epsilon || distanceSqr < float.Epsilon || Mathf.Sqrt(distanceSqr) < float.Epsilon)
                this.SetPosition(syncInfo.Position);
            else
                this.SetPosition(Vector3.MoveTowards(
                    this.GetPosition(),
                    syncInfo.Position,
                    moveDelta));

            this.SetRotation(Quaternion.RotateTowards(
                this.GetRotation(),
                syncInfo.Rotation,
                syncInfo.CalculatedAngularVelocityMagnitude * this.GetDeltaTime() * m_parameters.constantVelocityMultiplier));

        }

        private void UpdateClientUsingLerp()
        {
            this.SetPosition(Vector3.Lerp(
                this.GetPosition(),
                m_currentSyncData.Position,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime())));

            this.SetRotation(Quaternion.Lerp(
                this.GetRotation(),
                m_currentSyncData.Rotation,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime())));

        }

        private void UpdateClientUsingSphericalLerp()
        {
            this.SetPosition(Vector3.Slerp(
                this.GetPosition(),
                m_currentSyncData.Position,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime())));

            this.SetRotation(Quaternion.Slerp(
                this.GetRotation(),
                m_currentSyncData.Rotation,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime())));

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
                m_currentSyncData.Position = this.GetPosition();
                m_currentSyncData.Rotation = this.GetRotation();
            }
            m_currentSyncData.CalculatedVelocityMagnitude = 0;
            m_currentSyncData.CalculatedAngularVelocityMagnitude = 0;

            m_syncDataQueue.Clear();
        }

        private void ApplyCurrentSyncData()
        {
            if (!m_hasTransform)
                return;

            this.SetPosition();
            this.SetRotation();
        }

        private void AssignDataForSending()
        {
            if (!m_hasTransform)
                return;

            m_positionForSending = this.GetPosition();
            m_rotationForSending = this.GetRotation();
        }

        private void SetPosition()
        {
            this.SetPosition(m_currentSyncData.Position);
        }

        private void SetPosition(Vector3 pos)
        {
            if (m_hasRigidBody)
                m_rigidbody.MovePosition(pos);
            else if (m_hasTransform)
                m_transform.localPosition = pos;
        }

        private void SetRotation()
        {
            this.SetRotation(m_currentSyncData.Rotation);
        }

        private void SetRotation(Quaternion rot)
        {
            if (m_hasRigidBody)
                m_rigidbody.MoveRotation(rot);
            else if (m_hasTransform)
                m_transform.localRotation = rot;
        }

        private Vector3 GetPosition()
        {
            if (m_hasRigidBody)
                return m_rigidbody.position;
            if (m_hasTransform)
                return m_transform.localPosition;
            return m_currentSyncData.Position;
        }

        private Quaternion GetRotation()
        {
            if (m_hasRigidBody)
                return m_rigidbody.rotation;
            if (m_hasTransform)
                return m_transform.localRotation;
            return m_currentSyncData.Rotation;
        }
    }
}
