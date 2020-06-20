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

        public void Explode()
        {
            if (m_alreadyExploded)
                return;

            m_alreadyExploded = true;

            // destroy this game object

            Object.Destroy(this.gameObject);

            // detach the following parts:
            // - doors
            // - wheels
            // - bonnet
            // - boot
            // - windscreen
            // - exhaust

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

                var meshFilter = frame.GetComponentInChildren<MeshFilter>();
                if (null == meshFilter)
                    continue;

                if (!meshFilter.gameObject.activeInHierarchy)
                    continue;

                meshFilter.transform.SetParent(null, true);
                meshFilter.gameObject.name = "vehicle_part_" + meshFilter.gameObject.name;
                meshFilter.gameObject.layer = UnityEngine.LayerMask.NameToLayer("Default");
                var meshCollider = meshFilter.gameObject.GetOrAddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                var rigidBody = meshFilter.gameObject.GetOrAddComponent<Rigidbody>();
                rigidBody.mass = VehicleManager.Instance.explosionLeftoverPartsMass;
                rigidBody.drag = 0.05f;
                rigidBody.maxDepenetrationVelocity = VehicleManager.Instance.explosionLeftoverPartsMaxDepenetrationVelocity;
                rigidBody.AddExplosionForce(explosionForce, explosionCenter, explosionRadius);

                Object.Destroy(meshFilter.gameObject, VehicleManager.Instance.explosionLeftoverPartsLifetime);
            }

            // add rigid body to them and apply force

            // create explosion effect


        }

    }

}
