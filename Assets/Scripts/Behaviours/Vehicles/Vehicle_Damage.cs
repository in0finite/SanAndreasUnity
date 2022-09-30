using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Behaviours.Audio;
using SanAndreasUnity.Net;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{

    public partial class Vehicle
    {

        public Damageable Damageable { get; private set; }

        public float Health { get; set; } = 1000;

        public float MaxHealth { get; set; } = 1000;

        public bool IsUnderFlame { get; private set; } = false;
        public bool IsUnderSmoke { get; private set; } = false;

        bool m_alreadyExploded = false;
        public bool ExplodedThisFrame => m_alreadyExploded;

        public double TimeWhenBecameUnderFlame { get; private set; } = double.NegativeInfinity;
        public double TimeSinceBecameUnderFlame => Time.timeAsDouble - this.TimeWhenBecameUnderFlame;

        GameObject m_smokeGameObject;
        GameObject m_flameGameObject;

        public static AudioClip ExplosionSound { get; private set; }

        public static event System.Action<Vehicle, DamageInfo> onDamaged = delegate {};



        void Awake_Damage()
        {

        }

        void SetupDamagable()
        {
            this.Damageable = this.HighDetailMeshesParent.gameObject.AddComponent<Damageable>();
            this.Damageable.OnDamageEvent.AddListener(() => this.OnDamaged());
        }

        void OnDamaged()
        {
            if (!NetStatus.IsServer)
                return;

            var damageInfo = this.Damageable.LastDamageInfo;

            if (this.Health <= 0)
                return;

            this.Health -= damageInfo.amount;

            F.InvokeEventExceptionSafe(onDamaged, this, damageInfo);

            if (this.Health <= 0)
            {
                this.Explode();
            }

        }

        void Update_Damage()
        {

            bool shouldBeUnderSmoke = this.MaxHealth * 0.33f >= this.Health;
            if (shouldBeUnderSmoke != this.IsUnderSmoke)
            {
                // smoke status changed
                this.IsUnderSmoke = shouldBeUnderSmoke;
                // update vfx
                this.UpdateSmokeVfx();
            }

            bool shouldBeUnderFlame = this.MaxHealth * 0.1f >= this.Health;
            if (shouldBeUnderFlame != this.IsUnderFlame)
            {
                // flame status changed
                this.IsUnderFlame = shouldBeUnderFlame;
                if (this.IsUnderFlame)
                    this.TimeWhenBecameUnderFlame = Time.timeAsDouble;
                // update vfx
                this.UpdateFlameVfx();
            }

            if (this.IsUnderFlame && Time.timeAsDouble - this.TimeWhenBecameUnderFlame >= 5)
            {
                // enough time passed since vehicle flamed - explode it
                if (NetStatus.IsServer)
                {
                    this.Explode();
                }
            }

        }

        void UpdateSmokeVfx()
        {
            if (this.IsUnderSmoke)
            {
                if (null == m_smokeGameObject)
                {
                    Transform parent = this.EngineTransform != null ? this.EngineTransform : this.transform;
                    m_smokeGameObject = Object.Instantiate(
                        VehicleManager.Instance.smokePrefab, parent.position, parent.rotation, parent);
                }
            }
            else
            {
                if (null != m_smokeGameObject)
                {
                    Object.Destroy(m_smokeGameObject);
                    m_smokeGameObject = null;
                }
            }
        }

        void UpdateFlameVfx()
        {
            if (this.IsUnderFlame)
            {
                if (null == m_flameGameObject)
                {
                    Transform parent = this.EngineTransform != null ? this.EngineTransform : this.transform;
                    m_flameGameObject = Object.Instantiate(
                        VehicleManager.Instance.flamePrefab, parent.position, parent.rotation, parent);
                }
            }
            else
            {
                if (null != m_flameGameObject)
                {
                    Object.Destroy(m_flameGameObject);
                    m_flameGameObject = null;
                }
            }
        }

        public void Explode()
        {
            F.RunExceptionSafe(() => this.ExplodeInternal());
        }

        private void ExplodeInternal()
        {
            if (m_alreadyExploded)
                return;

            m_alreadyExploded = true;

            // destroy this game object before doing anything else

            Object.Destroy(this.gameObject);

            // detach vehicle parts

            string[] startingNames = new string[] { "door_", "wheel_", "bonnet_", "boot_", "windscreen_", "exhaust_" };
            
            foreach (var frame in _frames)
            {
                if (!frame.gameObject.activeInHierarchy)
                    continue;

                if (!startingNames.Any(n => frame.gameObject.name.StartsWith(n)))
                    continue;

                DetachFrameDuringExplosion(frame, VehicleManager.Instance.explosionLeftoverPartsMass, null);
            }

            // chassis need to be handled after all other objects are detached, because chassis can sometimes
            // have other objects as children

            Frame chassisFrame = _frames.FirstOrDefault(f => f.Name == "chassis");

            if (null == chassisFrame)
            {
                Debug.LogError($"Chassis object not found on vehicle {this.DescriptionForLogging}");
            }
            else
            {
                DetachFrameDuringExplosion(chassisFrame, this.HandlingData.Mass * 0.8f, null);
            }

            // inflict damage to nearby objects
            
            Damageable.InflictDamageToObjectsInArea(
                this.transform.position,
                VehicleManager.Instance.explosionDamageRadius,
                Mathf.Pow(this.HandlingData.Mass, VehicleManager.Instance.explosionMassToDamageExponent),
                VehicleManager.Instance.explosionDamageOverDistanceCurve,
                DamageType.Explosion);

            // create explosion - this includes effects, physics force, sound

            GameObject explosionGo = Object.Instantiate(VehicleManager.Instance.explosionPrefab, this.transform.position, this.transform.rotation);

            if (NetStatus.IsServer)
                NetManager.Spawn(explosionGo);

            // modify strength of explosion based on vehicle mass
            float forceFactor = Mathf.Sqrt(this.HandlingData.Mass) / Mathf.Sqrt(1500f);
            var physicsForce = explosionGo.GetComponentOrThrow<ExplosionForce>();
            physicsForce.explosionForce *= forceFactor;
            physicsForce.upwardsModifier *= forceFactor;

            // assign explosion sound
            F.RunExceptionSafe(() => AssignExplosionSound(explosionGo));

            // kill all peds inside
            foreach (Ped ped in this.Seats
                .Select(s => s.OccupyingPed)
                .Where(p => p != null)
                .ToList())
            {
                ped.Kill();
            }
        }

        public void DetachFrameDuringExplosion(string frameName, float mass, GameObject parentGo)
        {
            Frame frame = this.Frames.FirstOrDefault(f => f.gameObject.name == frameName);
            if (null == frame)
            {
                Debug.LogError($"Failed to find frame by name: {frameName}");
            }
            else
            {
                DetachFrameDuringExplosion(frame, mass, parentGo);
            }
        }

        void DetachFrameDuringExplosion(Frame frame, float mass, GameObject parentGo)
        {
            DetachFrameFromTransformDuringExplosion(
                this.transform,
                frame,
                mass,
                parentGo,
                this.NetIdentity.netId,
                this.Definition.Id,
                this.Colors,
                this.RigidBody != null ? this.RigidBody.velocity : (Vector3?)null,
                this.RigidBody != null ? this.RigidBody.angularVelocity : (Vector3?)null);
        }

        public static void DetachFrameFromTransformDuringExplosion(
            Transform tr,
            Frame frame,
            float mass,
            GameObject parentGo,
            uint vehicleNetId,
            int vehicleModelId,
            IReadOnlyList<Color32> vehicleColors,
            Vector3? initialVelocity = null,
            Vector3? initialAngularVelocity = null)
        {
            if (! tr.IsParentOf(frame.transform))   // already detached ?
                return;

            var meshFilter = frame.GetComponentInChildren<MeshFilter>();
            if (null == meshFilter)
                return;

            if (!meshFilter.gameObject.activeInHierarchy)
                return;

            if (null == parentGo)
                parentGo = Object.Instantiate(VehicleManager.Instance.explosionLeftoverPartPrefab, meshFilter.transform.position, meshFilter.transform.rotation);
            
            parentGo.name = "vehicle_part_" + meshFilter.gameObject.name;

            meshFilter.transform.SetParent(parentGo.transform, true);
            meshFilter.gameObject.layer = UnityEngine.LayerMask.NameToLayer("Default");
            var meshCollider = meshFilter.gameObject.GetOrAddComponent<MeshCollider>();
            meshCollider.cookingOptions = MeshColliderCookingOptions.None;
            meshCollider.convex = true;
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            var rigidBody = meshFilter.gameObject.GetOrAddComponent<Rigidbody>();
            rigidBody.mass = mass;
            rigidBody.drag = 0.05f;
            rigidBody.maxDepenetrationVelocity = VehicleManager.Instance.explosionLeftoverPartsMaxDepenetrationVelocity;
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (initialVelocity.HasValue)
                rigidBody.velocity = initialVelocity.Value;
            if (initialAngularVelocity.HasValue)
                rigidBody.angularVelocity = initialAngularVelocity.Value;
            
            if (NetStatus.IsServer)
            {
                Object.Destroy(parentGo, VehicleManager.Instance.explosionLeftoverPartsLifetime * Random.Range(0.8f, 1.2f));

                var netScript = parentGo.GetComponentOrThrow<NetworkedVehicleDetachedPart>();
                netScript.InitializeOnServer(vehicleNetId, vehicleModelId, vehicleColors, frame.gameObject.name, mass, rigidBody);

                NetManager.Spawn(parentGo);
            }
        }

        public static void AssignExplosionSound(GameObject explosionGo)
        {
            if (null == ExplosionSound)
                ExplosionSound = AudioManager.CreateAudioClipFromSfx("GENRL", 45, 1);

            var audioSource = explosionGo.GetComponentOrThrow<AudioSource>();
            audioSource.clip = ExplosionSound;
            audioSource.Play();
        }

        void OnDrawGizmosSelected()
        {
            // draw sphere indicating explosion damage radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, VehicleManager.Instance.explosionDamageRadius);
        }

    }

}
