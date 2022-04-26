using UnityEngine;
using Mirror;
using SanAndreasUnity.Utilities;

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

        private SyncInfo m_syncInfo;
        private SyncData m_syncData;

        private struct SyncInfo
        {
            public Transform Transform;

            // current sync data (as reported by server) toward which we move the transform
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;

            // these are the velocities used to move the object, calculated when new server data arrives
            public float CalculatedVelocityMagnitude;
            public float CalculatedAngularVelocityMagnitude;
        }

        private struct SyncData
        {
            public Vector3 position;
            public Vector3 rotation;
        }

        private readonly bool m_hasTransform = false;

        private readonly NetworkBehaviour m_networkBehaviour;



        public TransformSyncer(Transform tr, Parameters parameters, NetworkBehaviour networkBehaviour)
        {
            m_syncInfo.Transform = tr;
            m_parameters = parameters;
            m_networkBehaviour = networkBehaviour;
            m_hasTransform = tr != null;
        }

        public void OnStartClient()
        {
            if (NetUtils.IsServer)
                return;

            // apply initial sync data
            // first sync should've been done before calling this function, so the data is available
            this.UpdateDataAfterDeserialization(m_syncData, true);
        }

        public bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (!m_hasTransform)
                return false;

            byte flags = 0;
            writer.Write(flags);

            Transform tr = m_syncInfo.Transform;

            writer.Write(tr.localPosition);
            writer.Write(tr.localRotation.eulerAngles);

            return true;
        }

        public void OnDeserialize(NetworkReader reader, bool initialState)
        {
            byte flags = reader.ReadByte();

            var syncData = new SyncData();
            syncData.position = reader.ReadVector3();
            syncData.rotation = reader.ReadVector3();
            m_syncData = syncData;

            F.RunExceptionSafe(() => UpdateDataAfterDeserialization(syncData, false));
        }

        private void UpdateDataAfterDeserialization(SyncData syncData, bool applyToTransform)
        {
            SyncInfo syncInfo = m_syncInfo;

            if (applyToTransform && m_hasTransform)
            {
                syncInfo.Transform.localPosition = syncData.position;
                syncInfo.Transform.localRotation = Quaternion.Euler(syncData.rotation);
            }

            syncInfo.Position = syncData.position;// + syncData.velocity * this.syncInterval;
            if (m_hasTransform)
                syncInfo.CalculatedVelocityMagnitude = (syncInfo.Position - syncInfo.Transform.localPosition).magnitude / m_networkBehaviour.syncInterval;

            syncInfo.Rotation = Quaternion.Euler(syncData.rotation);
            if (m_hasTransform)
                syncInfo.CalculatedAngularVelocityMagnitude = Quaternion.Angle(syncInfo.Rotation, syncInfo.Transform.localRotation) / m_networkBehaviour.syncInterval;

            m_syncInfo = syncInfo;
        }

        public void Update()
        {
            if (!m_hasTransform)
                return;

            if (NetUtils.IsServer)
            {
                m_networkBehaviour.SetSyncVarDirtyBit(1);
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
            }
        }

        private void UpdateClientUsingConstantVelocity()
        {
            SyncInfo syncInfo = m_syncInfo;

            float moveDelta = syncInfo.CalculatedVelocityMagnitude * this.GetDeltaTime() * m_parameters.constantVelocityMultiplier;

            float distanceSqr = (syncInfo.Transform.localPosition - syncInfo.Position).sqrMagnitude;

            if (moveDelta < float.Epsilon || distanceSqr < float.Epsilon || Mathf.Sqrt(distanceSqr) < float.Epsilon)
                syncInfo.Transform.localPosition = syncInfo.Position;
            else
                syncInfo.Transform.localPosition = Vector3.MoveTowards(
                    syncInfo.Transform.localPosition,
                    syncInfo.Position,
                    moveDelta);

            syncInfo.Transform.localRotation = Quaternion.RotateTowards(
                syncInfo.Transform.localRotation,
                syncInfo.Rotation,
                syncInfo.CalculatedAngularVelocityMagnitude * this.GetDeltaTime() * m_parameters.constantVelocityMultiplier);

        }

        private void UpdateClientUsingLerp()
        {
            m_syncInfo.Transform.localPosition = Vector3.Lerp(
                m_syncInfo.Transform.localPosition,
                m_syncInfo.Position,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime()));

            m_syncInfo.Transform.localRotation = Quaternion.Lerp(
                m_syncInfo.Transform.localRotation,
                m_syncInfo.Rotation,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime()));

        }

        private void UpdateClientUsingSphericalLerp()
        {
            m_syncInfo.Transform.localPosition = Vector3.Slerp(
                m_syncInfo.Transform.localPosition,
                m_syncInfo.Position,
                1 - Mathf.Exp(-m_parameters.lerpFactor * this.GetDeltaTime()));

            m_syncInfo.Transform.localRotation = Quaternion.Slerp(
                m_syncInfo.Transform.localRotation,
                m_syncInfo.Rotation,
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
                m_syncInfo.Position = m_syncInfo.Transform.localPosition;
                m_syncInfo.Rotation = m_syncInfo.Transform.localRotation;
            }
            m_syncInfo.CalculatedVelocityMagnitude = 0;
            m_syncInfo.CalculatedAngularVelocityMagnitude = 0;
        }
    }
}
