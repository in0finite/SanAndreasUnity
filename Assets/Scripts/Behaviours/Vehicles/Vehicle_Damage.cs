using System.Linq;
using SanAndreasUnity.Utilities;
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

        public float TimeWhenBecameUnderFlame { get; private set; } = float.NegativeInfinity;

        GameObject m_smokeGameObject;



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
            var damageInfo = this.Damageable.LastDamageInfo;

            if (this.Health <= 0)
                return;

            this.Health -= damageInfo.amount;

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
                    this.TimeWhenBecameUnderFlame = Time.time;
                // update vfx

            }

            if (this.IsUnderFlame && Time.time - this.TimeWhenBecameUnderFlame >= 5)
            {
                // enough time passed since vehicle flamed - explode it
                this.Explode();
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

            // detach vehicle parts and apply explosion force on them

            string[] startingNames = new string[] { "door_", "wheel_", "bonnet_", "boot_", "windscreen_", "exhaust_" };
            Vector3 explosionCenter = this.transform.position;
            float explosionForce = Mathf.Sqrt(this.HandlingData.Mass) * VehicleManager.Instance.explosionForceMultiplier;
            float explosionRadius = 10f;

            foreach (var frame in _frames)
            {
                if (!frame.gameObject.activeInHierarchy)
                    continue;

                if (!startingNames.Any(n => frame.gameObject.name.StartsWith(n)))
                    continue;

                DetachFrameDuringExplosion(
                    frame, explosionCenter, explosionForce, explosionRadius, VehicleManager.Instance.explosionLeftoverPartsMass);
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
                DetachFrameDuringExplosion(
                    chassisFrame,
                    explosionCenter,
                    Mathf.Sqrt(this.HandlingData.Mass) * VehicleManager.Instance.explosionChassisForceMultiplier,
                    explosionRadius,
                    this.HandlingData.Mass * 0.9f);
            }

            // create explosion effect


        }

        void DetachFrameDuringExplosion(
            Frame frame, Vector3 explosionCenter, float explosionForce, float explosionRadius, float mass)
        {
            var meshFilter = frame.GetComponentInChildren<MeshFilter>();
            if (null == meshFilter)
                return;

            if (!meshFilter.gameObject.activeInHierarchy)
                return;

            meshFilter.transform.SetParent(null, true);
            meshFilter.gameObject.name = "vehicle_part_" + meshFilter.gameObject.name;
            meshFilter.gameObject.layer = UnityEngine.LayerMask.NameToLayer("Default");
            var meshCollider = meshFilter.gameObject.GetOrAddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            var rigidBody = meshFilter.gameObject.GetOrAddComponent<Rigidbody>();
            rigidBody.mass = mass;
            rigidBody.drag = 0.05f;
            rigidBody.maxDepenetrationVelocity = VehicleManager.Instance.explosionLeftoverPartsMaxDepenetrationVelocity;
            rigidBody.AddExplosionForce(explosionForce, explosionCenter, explosionRadius);

            Object.Destroy(meshFilter.gameObject, VehicleManager.Instance.explosionLeftoverPartsLifetime * Random.Range(0.8f, 1.2f));
        }

    }

}
