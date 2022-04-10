using System.Collections.Generic;
using System.Linq;
using Mirror;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SanAndreasUnity.Behaviours.Peds
{
    public class DeadBody : NetworkBehaviour
    {
        private static List<DeadBody> _deadBodies = new List<DeadBody>();
        public static IEnumerable<DeadBody> DeadBodies => _deadBodies;
        public static int NumDeadBodies => _deadBodies.Count;

        public FocusPointParameters focusPointParameters = FocusPointParameters.Default;

        public PushableByDamage PushableByDamage { get; private set; }

        public struct BoneInfo
        {
            public static BoneInfo Create(Transform transform)
            {
                var boneInfo = new BoneInfo();
                boneInfo.Transform = transform;
                boneInfo.Rigidbody = transform.GetComponent<Rigidbody>();
                return boneInfo;
            }

            public Transform Transform { get; private set; }
            public Rigidbody Rigidbody { get; private set; }

            // current sync data toward which we move the transform
            public Vector3 CurrentPosition;
            public Quaternion CurrentRotation;
            public Vector3 CurrentVelocity;
            public float CurrentAngularVelocity;

            // next sync data, which will be used when we reach the current sync data
            public Vector3? NextPosition;
            public Quaternion? NextRotation;
            public Vector3? NextVelocity;
            public float? NextAngularVelocity;
        }

        private Dictionary<int, BoneInfo> m_framesDict = new Dictionary<int, BoneInfo>();
        public int NumBones => m_framesDict.Count;
        public IReadOnlyDictionary<int, BoneInfo> GetBoneDictionary() => m_framesDict;

        private Dictionary<int, BoneInfo> m_rigidBodiesDict = new Dictionary<int, BoneInfo>();
        public int NumRigidBodies => m_rigidBodiesDict.Count;

        private int[] m_boneIds = System.Array.Empty<int>();

        public float TrafficKbps => (2 + NumRigidBodies * (1 + 12 + 12) + 12) / 1000f / this.syncInterval;

        private int m_net_modelId;

        private struct BoneSyncData
        {
            public byte boneId;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 velocity;

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(this.boneId);
                writer.Write(this.position);
                writer.Write(this.rotation);
                if (this.boneId == 0)
                    writer.Write(this.velocity);
            }

            public static BoneSyncData DeSerialize(NetworkReader reader)
            {
                var boneSyncData = new BoneSyncData();
                boneSyncData.boneId = reader.ReadByte();
                boneSyncData.position = reader.ReadVector3();
                boneSyncData.rotation = reader.ReadVector3();
                if (boneSyncData.boneId == 0)
                    boneSyncData.velocity = reader.ReadVector3();
                return boneSyncData;
            }
        }

        private List<BoneSyncData> m_bonesSyncData = new List<BoneSyncData>();



        private void Awake()
        {
            this.PushableByDamage = this.GetComponentOrThrow<PushableByDamage>();
            this.PushableByDamage.forceMultiplier = PedManager.Instance.ragdollDamageForceWhenDetached;

            this.RefreshSyncRate();
        }

        private void OnEnable()
        {
            _deadBodies.Add(this);
        }

        private void OnDisable()
        {
            _deadBodies.Remove(this);
        }

        public override void OnStartClient()
        {
            if (NetStatus.IsServer)
                return;

            F.RunExceptionSafe(this.InitialClientOnlySetup);
        }

        private void InitialClientOnlySetup()
        {
            var def = Item.GetDefinition<PedestrianDef>(m_net_modelId);
            if (null == def)
            {
                Debug.LogError($"Failed to initialize dead body: ped definition not found by id {m_net_modelId}");
                return;
            }

            this.gameObject.name = $"dead body {m_net_modelId} {def.ModelName}";

            var model = this.gameObject.GetOrAddComponent<PedModel>();
            model.Load(m_net_modelId);

            /*// add rigid bodies - syncing looks smoother with them
            model.RagdollBuilder.BuildBodies();
            foreach (var rb in this.transform.GetComponentsInChildren<Rigidbody>())
            {
                rb.useGravity = false;
                rb.detectCollisions = false;
                rb.maxAngularVelocity = 0;
                rb.interpolation = PedManager.Instance.ragdollInterpolationMode;
            }*/

            m_framesDict = model.Frames.ToDictionary(f => f.BoneId, f => BoneInfo.Create(f.transform));

            m_boneIds = m_framesDict.Keys.ToArray();

            /*// destroy all rigid bodies except for the root bone - they work for themselves, and bones look deformed and stretched
            m_framesDict
                .Where(pair => pair.Key != 0)
                .Select(pair => pair.Value.Rigidbody)
                .WhereAlive()
                .ForEach(Object.Destroy);*/

            Object.Destroy(model.AnimComponent);
            Object.Destroy(model);

            // apply initial sync data
            // first sync should've been done before calling this function, so the data is available

            /*foreach (int boneId in m_framesDict.Keys)
            {
                BoneInfo boneInfo = m_framesDict[boneId];
                boneInfo.CurrentPosition = boneInfo.Transform.localPosition;
                boneInfo.CurrentRotation = boneInfo.Transform.localRotation;
                m_framesDict[boneId] = boneInfo;
            }*/

            this.UpdateBonesDataAfterDeserialization((byte)m_bonesSyncData.Count, true);

        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
                writer.Write(m_net_modelId);

            byte flags = 0;
            writer.Write(flags);

            Dictionary<int, BoneInfo> bonesDict = initialState ? m_framesDict : m_rigidBodiesDict;
            bool checkRigidBodyForNull = initialState;

            writer.Write((byte)bonesDict.Count);

            foreach (var pair in bonesDict)
            {
                int boneId = pair.Key;
                Transform tr = pair.Value.Transform;
                Rigidbody rb = pair.Value.Rigidbody;

                var boneSyncData = new BoneSyncData();
                boneSyncData.boneId = (byte)boneId;
                boneSyncData.position = tr.localPosition;
                boneSyncData.rotation = tr.localRotation.eulerAngles;
                if (checkRigidBodyForNull)
                    boneSyncData.velocity = rb != null ? GetVelocityForSending(rb) : Vector3.zero;
                else
                    boneSyncData.velocity = GetVelocityForSending(rb);

                boneSyncData.Serialize(writer);
            }

            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
                m_net_modelId = reader.ReadInt();

            byte flags = reader.ReadByte();

            byte count = reader.ReadByte();

            m_bonesSyncData.EnsureCount(count);

            for (int i = 0; i < count; i++)
            {
                m_bonesSyncData[i] = BoneSyncData.DeSerialize(reader);
            }

            F.RunExceptionSafe(() => UpdateBonesDataAfterDeserialization(count, false));
        }

        private void UpdateBonesDataAfterDeserialization(byte count, bool applyToTransform)
        {
            for (int i = 0; i < count; i++)
            {
                var boneSyncData = m_bonesSyncData[i];
                if (m_framesDict.TryGetValue(boneSyncData.boneId, out BoneInfo boneInfo))
                {
                    if (applyToTransform)
                    {
                        boneInfo.Transform.localPosition = boneInfo.CurrentPosition = boneSyncData.position;
                        boneInfo.Transform.localRotation = boneInfo.CurrentRotation = Quaternion.Euler(boneSyncData.rotation);
                        boneInfo.CurrentVelocity = Vector3.zero;
                        boneInfo.CurrentAngularVelocity = 0;
                    }

                    boneInfo.NextPosition = boneSyncData.position;
                    boneInfo.NextVelocity = (boneInfo.NextPosition.Value - boneInfo.CurrentPosition).NormalizedOrZero() / this.syncInterval;

                    boneInfo.NextRotation = Quaternion.Euler(boneSyncData.rotation);
                    boneInfo.NextAngularVelocity = Quaternion.Angle(boneInfo.NextRotation.Value, boneInfo.CurrentRotation) / this.syncInterval;

                    m_framesDict[boneSyncData.boneId] = boneInfo;
                }
            }
        }

        public static DeadBody Create(Transform ragdollTransform, Ped ped)
        {
            NetStatus.ThrowIfNotOnServer();

            GameObject ragdollGameObject = Object.Instantiate(PedManager.Instance.ragdollPrefab);
            DeadBody deadBody = ragdollGameObject.GetComponentOrThrow<DeadBody>();

            Object.Destroy(ragdollGameObject, PedManager.Instance.ragdollLifetime * Random.Range(0.85f, 1.15f));

            ragdollGameObject.name = "dead body " + ped.name;

            ragdollTransform.SetParent(ragdollGameObject.transform);

            deadBody.m_framesDict = ragdollTransform.GetComponentsInChildren<Frame>()
                .ToDictionary(f => f.BoneId, f => BoneInfo.Create(f.transform));

            foreach (var pair in deadBody.m_framesDict)
            {
                var rb = pair.Value.Rigidbody;
                if (rb != null)
                    deadBody.m_rigidBodiesDict.Add(pair.Key, BoneInfo.Create(rb.transform));
            }

            FocusPoint.Create(ragdollTransform.gameObject, deadBody.focusPointParameters);

            deadBody.InitSyncVarsOnServer(ped);

            NetManager.Spawn(ragdollGameObject);

            return deadBody;
        }

        private void InitSyncVarsOnServer(Ped ped)
        {
            m_net_modelId = ped.PedDef.Id;
        }

        private void Update()
        {
            if (NetStatus.IsServer)
            {
                this.SetSyncVarDirtyBit(1);
            }
            else
            {
                this.UpdateRagdollBasedOnSyncData();
            }
        }

        private void UpdateRagdollBasedOnSyncData()
        {
            for (int i = 0; i < m_boneIds.Length; i++)
            {
                int boneId = m_boneIds[i];
                BoneInfo boneInfo = m_framesDict[boneId];

                // move transform to current position/rotation

                // position

                Vector3 transformPos = boneInfo.Transform.localPosition;
                Vector3 moveDiff = boneInfo.CurrentPosition - transformPos;
                float sqrDistance = moveDiff.sqrMagnitude;
                Vector3 moveDelta = moveDiff.normalized * boneInfo.CurrentVelocity.magnitude * Time.deltaTime;
                if (moveDelta.sqrMagnitude < sqrDistance && moveDelta.sqrMagnitude > float.Epsilon)
                    boneInfo.Transform.localPosition += moveDelta;
                else
                {
                    boneInfo.Transform.localPosition = boneInfo.CurrentPosition;
                    if (boneInfo.NextPosition.HasValue)
                    {
                        boneInfo.CurrentPosition = boneInfo.NextPosition.Value;
                        boneInfo.NextPosition = null;
                        boneInfo.CurrentVelocity = boneInfo.NextVelocity.Value;
                        boneInfo.NextVelocity = null;
                    }
                }

                // rotation

                Quaternion transformRotation = boneInfo.Transform.localRotation;
                float angle = Quaternion.Angle(transformRotation, boneInfo.CurrentRotation);
                float angleDelta = boneInfo.CurrentAngularVelocity * Time.deltaTime;
                if (angleDelta < angle && angleDelta > float.Epsilon)
                    boneInfo.Transform.localRotation = Quaternion.RotateTowards(transformRotation, boneInfo.CurrentRotation, angleDelta);
                else
                {
                    boneInfo.Transform.localRotation = boneInfo.CurrentRotation;
                    if (boneInfo.NextRotation.HasValue)
                    {
                        boneInfo.CurrentRotation = boneInfo.NextRotation.Value;
                        boneInfo.NextRotation = null;
                        boneInfo.CurrentAngularVelocity = boneInfo.NextAngularVelocity.Value;
                        boneInfo.NextAngularVelocity = null;
                    }
                }


                m_framesDict[boneId] = boneInfo;
            }
        }

        public void RefreshSyncRate()
        {
            this.syncInterval = 1.0f / PedManager.Instance.ragdollSyncRate;
        }

        private static Vector3 GetVelocityForSending(Rigidbody rb)
        {
            // it's better to send local velocity, because rotation of ragdoll can change very fast, and so
            // will the world velocity
            return rb.transform.InverseTransformVector(rb.velocity);
        }

        private static Vector3 GetReceivedVelocityAsLocal(Transform tr, Vector3 receivedVelocity)
        {
            return receivedVelocity;
        }

        private static Vector3 GetReceivedVelocityAsWorld(Transform tr, Vector3 receivedVelocity)
        {
            return tr.TransformVector(receivedVelocity);
        }

        private static void SetPosition(BoneInfo boneInfo, Vector3 receivedPosition)
        {
            // if (boneInfo.Rigidbody != null)
            //     boneInfo.Rigidbody.MovePosition(boneInfo.Transform.TransformVector(receivedPosition));
            // else
            //     boneInfo.Transform.localPosition = receivedPosition;

            boneInfo.Transform.localPosition = receivedPosition;
        }

        private static void SetRotation(BoneInfo boneInfo, Vector3 receivedRotation)
        {
            // Quaternion localRotation = Quaternion.Euler(receivedRotation);
            // if (boneInfo.Rigidbody != null)
            //     boneInfo.Rigidbody.MoveRotation(boneInfo.Transform.TransformRotation(localRotation));
            // else
            //     boneInfo.Transform.localRotation = localRotation;

            boneInfo.Transform.localRotation = Quaternion.Euler(receivedRotation);
        }

        private static void SetVelocity(BoneInfo boneInfo, Vector3 receivedVelocity)
        {
            boneInfo.Rigidbody.velocity = GetReceivedVelocityAsWorld(boneInfo.Transform, receivedVelocity);
        }
    }
}
