using System.Collections.Generic;
using System.Linq;
using Mirror;
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

        public PushableByDamage PushableByDamage { get; private set; }

        private struct BoneInfo
        {
            public BoneInfo(Transform transform)
            {
                this.Transform = transform;
                this.Rigidbody = transform.GetComponent<Rigidbody>();
            }

            public Transform Transform { get; set; }
            public Rigidbody Rigidbody { get; set; }
        }

        private Dictionary<int, BoneInfo> m_framesDict = new Dictionary<int, BoneInfo>();
        public int NumBones => m_framesDict.Count;

        private Dictionary<int, Rigidbody> m_rigidBodiesDict = new Dictionary<int, Rigidbody>();
        public int NumRigidBodies => m_rigidBodiesDict.Count;

        private int m_net_modelId;

        private struct BoneSyncData
        {
            public byte boneId;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 velocity;
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

            // add rigid bodies - syncing looks smoother with them
            model.RagdollBuilder.BuildBodies();
            foreach (var rb in this.transform.GetComponentsInChildren<Rigidbody>())
            {
                rb.useGravity = false;
                rb.detectCollisions = false;
                rb.maxAngularVelocity = 0;
                rb.interpolation = PedManager.Instance.ragdollInterpolationMode;
            }

            m_framesDict = model.Frames.ToDictionary(f => f.BoneId, f => new BoneInfo(f.transform));

            Object.Destroy(model.AnimComponent);
            Object.Destroy(model);

            // apply initial sync data
            // first sync should've been done before calling this function
            this.UpdateBonesAfterDeserialization((byte)m_bonesSyncData.Count);

        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
                writer.Write(m_net_modelId);

            writer.Write((byte)m_framesDict.Count);

            foreach (var pair in m_framesDict)
            {
                int boneId = pair.Key;
                Transform tr = pair.Value.Transform;

                var boneSyncData = new BoneSyncData();
                boneSyncData.boneId = (byte)boneId;
                boneSyncData.position = tr.localPosition;
                boneSyncData.rotation = tr.localRotation.eulerAngles;
                boneSyncData.velocity = m_rigidBodiesDict.TryGetValue(boneId, out Rigidbody rb) ? GetVelocityForSending(rb) : Vector3.zero;

                writer.Write(boneSyncData.boneId);
                writer.Write(boneSyncData.position);
                writer.Write(boneSyncData.rotation);
                writer.Write(boneSyncData.velocity);
            }

            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
                m_net_modelId = reader.ReadInt32();

            byte count = reader.ReadByte();

            m_bonesSyncData.EnsureCount(count);

            for (int i = 0; i < count; i++)
            {
                var boneSyncData = new BoneSyncData();
                boneSyncData.boneId = reader.ReadByte();
                boneSyncData.position = reader.ReadVector3();
                boneSyncData.rotation = reader.ReadVector3();
                boneSyncData.velocity = reader.ReadVector3();
                m_bonesSyncData[i] = boneSyncData;
            }

            F.RunExceptionSafe(() => UpdateBonesAfterDeserialization(count));
        }

        private void UpdateBonesAfterDeserialization(byte count)
        {
            for (int i = 0; i < count; i++)
            {
                var boneSyncData = m_bonesSyncData[i];
                if (m_framesDict.TryGetValue(boneSyncData.boneId, out BoneInfo boneInfo))
                {
                    SetPosition(boneInfo, boneSyncData.position);
                    SetRotation(boneInfo, boneSyncData.rotation);
                    if (boneInfo.Rigidbody != null)
                        SetVelocity(boneInfo, boneSyncData.velocity);
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
                .ToDictionary(f => f.BoneId, f => new BoneInfo(f.transform));

            foreach (var pair in deadBody.m_framesDict)
            {
                var rb = pair.Value.Rigidbody;
                if (rb != null)
                    deadBody.m_rigidBodiesDict.Add(pair.Key, rb);
            }

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
                this.SetDirtyBit(1);
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
