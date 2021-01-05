using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using SanAndreasUnity.Importing.Conversion;
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

        private Dictionary<int, Transform> m_framesDict = new Dictionary<int, Transform>();

        private Dictionary<int, Rigidbody> m_rigidBodiesDict = new Dictionary<int, Rigidbody>();

        [SyncVar] private int m_net_modelId;

        private class SyncDictionaryIntVector3 : SyncDictionary<int, Vector3>
        {
        }

        private SyncDictionaryIntVector3 m_syncDictionaryBonePositions = new SyncDictionaryIntVector3();
        private SyncDictionaryIntVector3 m_syncDictionaryBoneRotations = new SyncDictionaryIntVector3();
        private SyncDictionaryIntVector3 m_syncDictionaryBoneVelocities = new SyncDictionaryIntVector3();



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
            // load ped model

            // apply initial bones sync data (because not all bones have rigid bodies)

            // apply initial ragdoll (rigid bodies) sync data - this should be done AFTER applying bones sync data




            var def = Item.GetDefinition<PedestrianDef>(m_net_modelId);
            if (null == def)
            {
                Debug.LogError($"Failed to initialize dead body: ped definition not found by id {m_net_modelId}");
                return;
            }

            this.gameObject.name = $"dead body {m_net_modelId} {def.ModelName}";

            var geoms = Geometry.Load(def.ModelName, def.TextureDictionaryName);
            var frames = geoms.AttachFrames(this.transform, MaterialFlags.Default);

            m_framesDict = frames.ToDictionary(f => f.BoneId, f => f.transform);

            // apply initial transformation data to bones (at the moment when ragdoll was detached) - this is needed because not all bones have rigid bodies
            // attached, and so will not be moved/rotated by physics engine

            foreach (var pair in m_framesDict)
            {
                if (m_syncDictionaryBonePositions.TryGetValue(pair.Key, out Vector3 pos))
                    pair.Value.localPosition = pos;
                if (m_syncDictionaryBoneRotations.TryGetValue(pair.Key, out Vector3 rotation))
                    pair.Value.localRotation = Quaternion.Euler(rotation);
            }

            // register to dictionary callbacks
            RegisterDictionaryCallback(
                m_syncDictionaryBonePositions,
                (tr, pos) => tr.localPosition = pos);
            RegisterDictionaryCallback(
                m_syncDictionaryBoneRotations,
                (tr, rotation) => tr.localRotation = Quaternion.Euler(rotation));
        }

        private void RegisterDictionaryCallback<T>(SyncDictionary<int, T> dict, System.Action<Transform, T> action)
        {
            dict.Callback += (op, key, item) => F.RunExceptionSafe(() =>
            {
                if (m_framesDict.TryGetValue(key, out Transform tr))
                    action(tr, item);
            });
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
                .ToDictionary(f => f.BoneId, f => f.transform);

            foreach (var pair in deadBody.m_framesDict)
            {
                var rb = pair.Value.GetComponent<Rigidbody>();
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

            // assign initial bones transformations
            foreach (var pair in m_framesDict)
            {
                m_syncDictionaryBonePositions.Add(pair.Key, pair.Value.localPosition);
                m_syncDictionaryBoneRotations.Add(pair.Key, pair.Value.localRotation.eulerAngles);
            }

            // assign initial velocities
            foreach (var pair in m_rigidBodiesDict)
            {
                m_syncDictionaryBoneVelocities.Add(pair.Key, pair.Value.velocity);
            }
        }

        private void Update()
        {
            if (NetStatus.IsServer)
            {
                foreach (var pair in m_framesDict)
                {
                    Vector3 pos = pair.Value.localPosition;
                    Vector3 rotation = pair.Value.localRotation.eulerAngles;

                    if (m_syncDictionaryBonePositions[pair.Key] != pos)
                        m_syncDictionaryBonePositions[pair.Key] = pos;
                    if (m_syncDictionaryBoneRotations[pair.Key] != rotation)
                        m_syncDictionaryBoneRotations[pair.Key] = rotation;
                }

                foreach (var pair in m_rigidBodiesDict)
                {
                    Vector3 velocity = pair.Value.velocity;
                    if (m_syncDictionaryBoneVelocities[pair.Key] != velocity)
                        m_syncDictionaryBoneVelocities[pair.Key] = velocity;
                }
            }
            else
            {
                // apply velocity on clients
                foreach (var pair in m_syncDictionaryBoneVelocities)
                {
                    int boneId = pair.Key;
                    if (m_framesDict.TryGetValue(boneId, out Transform tr))
                        tr.position += pair.Value * Time.deltaTime;
                }
            }
        }

        public void RefreshSyncRate()
        {
            this.syncInterval = 1.0f / PedManager.Instance.ragdollSyncRate;
        }
    }
}
