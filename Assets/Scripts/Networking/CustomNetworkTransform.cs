using Mirror;
using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Net
{
    public class CustomNetworkTransform : NetworkBehaviour
    {
        public bool useSmoothDeltaTime = false;

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



        void Awake()
        {
            m_syncInfo.Transform = this.transform;
        }

        public override void OnStartClient()
        {
            if (NetUtils.IsServer)
                return;

            // apply initial sync data
            // first sync should've been done before calling this function, so the data is available
            this.UpdateDataAfterDeserialization(m_syncData, true);
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            byte flags = 0;
            writer.Write(flags);

            Transform tr = m_syncInfo.Transform;

            writer.Write(tr.localPosition);
            writer.Write(tr.localRotation.eulerAngles);

            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
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

            if (applyToTransform)
            {
                syncInfo.Transform.localPosition = syncData.position;
                syncInfo.Transform.localRotation = Quaternion.Euler(syncData.rotation);
            }

            syncInfo.Position = syncData.position;// + syncData.velocity * this.syncInterval;
            syncInfo.CalculatedVelocityMagnitude = (syncInfo.Position - syncInfo.Transform.localPosition).magnitude / this.syncInterval;

            syncInfo.Rotation = Quaternion.Euler(syncData.rotation);
            syncInfo.CalculatedAngularVelocityMagnitude = Quaternion.Angle(syncInfo.Rotation, syncInfo.Transform.localRotation) / this.syncInterval;

            m_syncInfo = syncInfo;
        }

        private void Update()
        {
            if (NetUtils.IsServer)
            {
                this.SetSyncVarDirtyBit(1);
            }
            else
            {
                this.UpdateBasedOnSyncData();
            }
        }

        private void UpdateBasedOnSyncData()
        {
            SyncInfo syncInfo = m_syncInfo;

            // move transform to current position/rotation

            // position

            Vector3 transformPos = syncInfo.Transform.localPosition;
            Vector3 moveDiff = syncInfo.Position - transformPos;
            float sqrDistance = moveDiff.sqrMagnitude;
            Vector3 moveDelta = moveDiff.normalized * syncInfo.CalculatedVelocityMagnitude * this.GetDeltaTime();
            if (moveDelta.sqrMagnitude < sqrDistance && moveDelta.sqrMagnitude > float.Epsilon)
                syncInfo.Transform.localPosition += moveDelta;
            else
            {
                syncInfo.Transform.localPosition = syncInfo.Position;
            }

            // rotation

            Quaternion transformRotation = syncInfo.Transform.localRotation;
            float angle = Quaternion.Angle(transformRotation, syncInfo.Rotation);
            float angleDelta = syncInfo.CalculatedAngularVelocityMagnitude * this.GetDeltaTime();
            if (angleDelta < angle && angleDelta > float.Epsilon)
                syncInfo.Transform.localRotation = Quaternion.RotateTowards(transformRotation, syncInfo.Rotation, angleDelta);
            else
            {
                syncInfo.Transform.localRotation = syncInfo.Rotation;
            }

            m_syncInfo = syncInfo;
        }

        private float GetDeltaTime()
        {
            return this.useSmoothDeltaTime ? Time.smoothDeltaTime : Time.deltaTime;
        }
    }
}
