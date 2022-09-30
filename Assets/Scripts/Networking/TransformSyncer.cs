using UnityEngine;
using Mirror;
using UGameCore.Utilities;
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
            SnapshotInterpolation,
        }

        [System.Serializable]
        public struct Parameters
        {
            public bool useSmoothDeltaTime;

            public ClientUpdateType clientUpdateType;

            public float constantVelocityMultiplier;

            public float lerpFactor;

            public bool useRigidBody;

            public bool visualize;

            public ushort maxNumVisualizations;

            public float visualizationScale;

            public float snapshotLatency;

            public static Parameters Default => new Parameters
            {
                useSmoothDeltaTime = true,
                clientUpdateType = ClientUpdateType.ConstantVelocity,
                constantVelocityMultiplier = 1f,
                lerpFactor = 30f,
                useRigidBody = true,
                visualize = false,
                maxNumVisualizations = 10,
                visualizationScale = 0.2f,
                snapshotLatency = 0.1f,
            };
        }

        private Parameters m_parameters = Parameters.Default;
        public Parameters Params => m_parameters;

        private SyncData m_currentSyncData = new SyncData { Rotation = Quaternion.identity };
        public SyncData CurrentSyncData => m_currentSyncData;

        // we will switch to this sync data when we reach the current sync data
        private SyncData? m_nextSyncData = null;

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

            // timestamp of server when this data was sent (batch timestamp)
            public double RemoteTimeStamp;

            public void Apply(Transform tr)
            {
                tr.localPosition = this.Position;
                tr.localRotation = this.Rotation;
            }
        }

        private readonly bool m_hasTransform = false;
        private readonly bool m_hasRigidBody = false;

        private readonly NetworkBehaviour m_networkBehaviour;

        private Queue<GameObject> m_visualizationQueue = null; // null to save memory

        // Buffer which holds received snapshots, ordered by remote timestamp. Queue seems to be better
        // choice than List, because
        // we often remove multiple elements from it, so the comparison boils down to: Dequeue() multiple
        // times vs RemoveRange().
        private Queue<SyncData> m_snapshotBuffer = null; // null to save memory server-side
        
        public int SnapshotBufferCount => m_snapshotBuffer?.Count ?? 0;

        // cached for fast access, otherwise we would have to iterate through Queue
        private SyncData m_lastAddedSnapshot;
        


        public TransformSyncer(Transform tr, Parameters parameters, NetworkBehaviour networkBehaviour)
        {
            m_transform = tr;
            m_rigidbody = tr != null ? tr.GetComponent<Rigidbody>() : null;
            m_parameters = parameters;
            m_networkBehaviour = networkBehaviour;
            m_hasTransform = tr != null;
            m_hasRigidBody = m_rigidbody != null;
        }

        public bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            byte flags = 0;
            writer.Write(flags);

            writer.Write(this.GetPosition());
            writer.Write(this.GetRotation().eulerAngles);

            return true;
        }

        public void OnDeserialize(NetworkReader reader, bool initialState)
        {
            byte flags = reader.ReadByte();

            var syncData = new SyncData();

            syncData.RemoteTimeStamp = NetworkClient.connection.remoteTimeStamp;

            syncData.Position = reader.ReadVector3();// + syncData.velocity * this.syncInterval;
            syncData.Rotation = Quaternion.Euler(reader.ReadVector3());

            if (initialState)
            {
                syncData.CalculatedVelocityMagnitude = float.PositiveInfinity;
                syncData.CalculatedAngularVelocityMagnitude = float.PositiveInfinity;

                m_currentSyncData = syncData;

                this.WarpToLatestSyncData();
            }
            else
            {
                syncData.CalculatedVelocityMagnitude = (syncData.Position - m_currentSyncData.Position).magnitude / m_networkBehaviour.syncInterval;
                syncData.CalculatedAngularVelocityMagnitude = Quaternion.Angle(syncData.Rotation, m_currentSyncData.Rotation) / m_networkBehaviour.syncInterval;

                m_nextSyncData = syncData;

                this.AddToVisualization(syncData);
            }

            this.AddToSnapshotBuffer(syncData);
        }

        void AddToVisualization(SyncData syncData)
        {
            if (!m_parameters.visualize || m_parameters.maxNumVisualizations <= 0)
            {
                if (m_visualizationQueue != null)
                {
                    while (m_visualizationQueue.Count > 0)
                        Object.Destroy(m_visualizationQueue.Dequeue());
                    m_visualizationQueue = null;
                }
                
                return;
            }

            m_visualizationQueue ??= new Queue<GameObject>();

            while (m_visualizationQueue.Count >= m_parameters.maxNumVisualizations)
                Object.Destroy(m_visualizationQueue.Dequeue());

            var newGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newGo.transform.SetPositionAndRotation(syncData.Position, syncData.Rotation);
            newGo.transform.localScale = Vector3.one * m_parameters.visualizationScale;
            Object.DestroyImmediate(newGo.GetComponent<Collider>());
            
            m_visualizationQueue.Enqueue(newGo);

            int i = 0;
            foreach (var go in m_visualizationQueue)
            {
                go.name = $"{m_networkBehaviour.name} - sync visualization {i}";
                go.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.black, i / (float)m_visualizationQueue.Count);
                i++;
            }
        }

        private void AddToSnapshotBuffer(SyncData syncData)
        {
            if (m_parameters.clientUpdateType != ClientUpdateType.SnapshotInterpolation)
            {
                m_snapshotBuffer = null;
                return;
            }

            m_snapshotBuffer ??= new Queue<SyncData>();

            if (m_snapshotBuffer.Count == 0)
            {
                m_snapshotBuffer.Enqueue(syncData);
                m_lastAddedSnapshot = syncData;
                return;
            }

            if (m_lastAddedSnapshot.RemoteTimeStamp >= syncData.RemoteTimeStamp)
            {
                // can happen if packets arrive out of order (eg. on UDP transport)
                return;
            }

            m_snapshotBuffer.Enqueue(syncData);
            m_lastAddedSnapshot = syncData;
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
                this.CheckIfArrivedToNextSyncData();

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
                    case ClientUpdateType.SnapshotInterpolation:
                        this.UpdateClientUsingSnapshotInterpolation();
                        break;
                    default:
                        break;
                }

                this.CheckIfArrivedToNextSyncData();
            }
        }

        private void UpdateClientUsingSnapshotInterpolation()
        {
            if (null == m_snapshotBuffer || m_snapshotBuffer.Count == 0)
                return;

            double currentNetworkTime = NetworkTime.time - m_parameters.snapshotLatency;

            // find higher and lower snapshots - those are snapshots that surround the 'currentNetworkTime'

            SyncData syncDataOfHigher = default;
            SyncData syncDataOfLower = default;

            // note: using foreach with Queue<T> will not allocate memory
            bool isFirst = true;
            bool foundFirstHigher = false;
            foreach (SyncData snapshot in m_snapshotBuffer)
            {
                if (snapshot.RemoteTimeStamp >= currentNetworkTime)
                {
                    syncDataOfHigher = snapshot;
                    if (isFirst)
                        syncDataOfLower = snapshot;
                    foundFirstHigher = true;
                    break;
                }

                isFirst = false;
                syncDataOfLower = snapshot;
            }

            if (!foundFirstHigher)
            {
                // we are ahead of all snapshots
                // use the last snapshot for both higher and lower snapshot
                syncDataOfHigher = syncDataOfLower;
            }

            // interpolate between lower and higher syncdata
            double ratio = (currentNetworkTime - syncDataOfLower.RemoteTimeStamp) / (syncDataOfHigher.RemoteTimeStamp - syncDataOfLower.RemoteTimeStamp);
            if (double.IsNaN(ratio))
                ratio = 1;
            ratio = Mathd.Clamp01(ratio);
            SyncData interpolated = DoSnapshotInterpolation(syncDataOfLower, syncDataOfHigher, ratio);
            this.Apply(interpolated);

            // remove old snapshots, but be careful not to remove the current lower snapshot
            this.RemoveOldSnapshots(System.Math.Min(currentNetworkTime, syncDataOfLower.RemoteTimeStamp));

        }

        private void RemoveOldSnapshots(double currentNetworkTime)
        {
            while (true)
            {
                if (m_snapshotBuffer.Count == 0)
                    break;

                SyncData syncData = m_snapshotBuffer.Peek();
                if (syncData.RemoteTimeStamp >= currentNetworkTime)
                    break;

                m_snapshotBuffer.Dequeue();
            }
        }

        private static SyncData DoSnapshotInterpolation(SyncData from, SyncData to, double t)
        {
            return new SyncData
            {
                Position = Vector3.LerpUnclamped(from.Position, to.Position, (float)t),
                Rotation = Quaternion.SlerpUnclamped(from.Rotation, to.Rotation, (float)t),
            };
        }

        private void UpdateClientUsingConstantVelocity()
        {
            this.SetPosition(Vector3.MoveTowards(
                this.GetPosition(),
                m_currentSyncData.Position,
                m_currentSyncData.CalculatedVelocityMagnitude * this.GetDeltaTime() * m_parameters.constantVelocityMultiplier));

            this.SetRotation(Quaternion.RotateTowards(
                this.GetRotation(),
                m_currentSyncData.Rotation,
                m_currentSyncData.CalculatedAngularVelocityMagnitude * this.GetDeltaTime() * m_parameters.constantVelocityMultiplier));

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

        public void ResetSyncDataToTransform()
        {
            if (m_hasTransform)
            {
                m_currentSyncData.Position = this.GetPosition();
                m_currentSyncData.Rotation = this.GetRotation();
            }
            m_currentSyncData.CalculatedVelocityMagnitude = float.PositiveInfinity;
            m_currentSyncData.CalculatedAngularVelocityMagnitude = float.PositiveInfinity;
            m_nextSyncData = null;
            m_snapshotBuffer?.Clear();
        }

        public void WarpToLatestSyncData()
        {
            var syncData = this.GetLatestSyncData();

            // assign position/rotation directly to transform, because rigid body may not warp ?
            if (m_hasTransform)
            {
                syncData.Apply(m_transform);
            }

            m_currentSyncData = syncData;
            m_nextSyncData = null;
            m_snapshotBuffer?.Clear();
        }

        public SyncData GetLatestSyncData()
        {
            return m_nextSyncData ?? m_currentSyncData;
        }

        private void Apply(SyncData syncData)
        {
            this.SetPosition(syncData.Position);
            this.SetRotation(syncData.Rotation);
        }

        private void SetPosition(Vector3 pos)
        {
            if (m_parameters.useRigidBody && m_hasRigidBody)
                m_rigidbody.MovePosition(pos);
            else if (m_hasTransform)
                m_transform.localPosition = pos;
        }

        private void SetRotation(Quaternion rot)
        {
            if (m_parameters.useRigidBody && m_hasRigidBody)
                m_rigidbody.MoveRotation(rot);
            else if (m_hasTransform)
                m_transform.localRotation = rot;
        }

        private Vector3 GetPosition()
        {
            if (m_parameters.useRigidBody && m_hasRigidBody)
                return m_rigidbody.position;
            if (m_hasTransform)
                return m_transform.localPosition;
            return m_currentSyncData.Position;
        }

        private Quaternion GetRotation()
        {
            if (m_parameters.useRigidBody && m_hasRigidBody)
                return m_rigidbody.rotation;
            if (m_hasTransform)
                return m_transform.localRotation;
            return m_currentSyncData.Rotation;
        }

        private bool ArrivedToCurrentSyncData()
        {
            return Vector3.Distance(m_currentSyncData.Position, this.GetPosition()) < 0.01f
                && Quaternion.Angle(m_currentSyncData.Rotation, this.GetRotation()) < 1f;
        }

        private void CheckIfArrivedToNextSyncData()
        {
            if (m_parameters.clientUpdateType == ClientUpdateType.SnapshotInterpolation)
                return;

            if (m_nextSyncData.HasValue && this.ArrivedToCurrentSyncData())
            {
                this.Apply(m_currentSyncData);

                m_currentSyncData = m_nextSyncData.Value;
                m_nextSyncData = null;
            }
        }
    }
}
