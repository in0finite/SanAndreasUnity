using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Net;
using UGameCore.Utilities;
using UnityEngine;
using UnityStandardAssets.Effects;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SanAndreasUnity.Behaviours
{
    public class Projectile : MonoBehaviour
    {
        public GameObject explosionPrefab;
        public float explosionDamageAmount = 1000;
        public float explosionDamageRadius = 5;
        public float particleSystemMultiplier = 1;
        public float speed = 10;
        public float acceleration = 4;
        public float maxSpeed = 50;
        public float rotationSpeed = 180;
        public float lifeTime = 30;
        [SerializeField] private Transform m_modelAttachTransform = null;

        private bool m_alreadyExploded = false;
        public Rigidbody RigidBody { get; private set; }
        public AudioSource AudioSource { get; private set; }

        public Ped ShooterPed { get; private set; }
        public Player ShooterPlayer { get; private set; }


        public static Projectile Create(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Ped shooterPed)
        {
            NetStatus.ThrowIfNotOnServer();

            var go = Instantiate(prefab, position, rotation);

            var projectile = go.GetComponentOrThrow<Projectile>();
            projectile.ShooterPed = shooterPed;
            projectile.ShooterPlayer = shooterPed != null ? shooterPed.PlayerOwner : null;

            if (shooterPed != null)
            {
                var projectileCollider = projectile.GetComponentOrThrow<Collider>();
                var pedColliders = shooterPed.GetComponentsInChildren<Collider>();
                foreach (var pedCollider in pedColliders)
                {
                    Physics.IgnoreCollision(pedCollider, projectileCollider);
                }
            }

            NetManager.Spawn(go);

            return projectile;
        }

        void Awake()
        {
            this.RigidBody = this.GetComponentOrThrow<Rigidbody>();
            this.AudioSource = this.GetComponentOrThrow<AudioSource>();

            if (NetStatus.IsServer)
                Object.Destroy(this.gameObject, this.lifeTime);

            if (Weapon.ProjectileSound != null)
            {
                this.AudioSource.clip = Weapon.ProjectileSound;
                this.AudioSource.Play();
            }

            if (Weapon.ProjectileModel != null)
            {
                Weapon.ProjectileModel.AttachFrames(m_modelAttachTransform, MaterialFlags.Default);
            }

            this.RigidBody.velocity = this.transform.forward * this.speed;

            if (!NetStatus.IsServer)
            {
                // disable all colliders
                var colliders = this.gameObject.GetComponentsInChildren<Collider>();
                foreach (var c in colliders)
                {
                    c.enabled = false;
                }
            }

        }

        private void Update()
        {
            // rotate model
            float delta = this.rotationSpeed * Time.deltaTime * Random.Range (0.75f, 1.25f);
            m_modelAttachTransform.rotation *= Quaternion.AngleAxis (delta, Vector3.forward);

            // accelerate projectile
            Vector3 currentVectorSpeed = this.RigidBody.velocity;
            float currentSpeed = currentVectorSpeed.magnitude;
            if (currentSpeed < this.maxSpeed)
            {
                float newSpeed = currentSpeed + this.acceleration * Time.deltaTime;
                this.RigidBody.velocity = currentVectorSpeed.normalized * newSpeed;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!NetStatus.IsServer)
                return;

            if (m_alreadyExploded)
                return;

            m_alreadyExploded = true;

            Object.Destroy(this.gameObject);

            Vector3 contactPoint = other.contacts[0].point;

            // inflict damage to nearby objects
            Damageable.InflictDamageToObjectsInArea(
                contactPoint,
                this.explosionDamageRadius,
                this.explosionDamageAmount,
                VehicleManager.Instance.explosionDamageOverDistanceCurve,
                DamageType.Explosion,
                this.ShooterPed,
                this.ShooterPlayer);

            // create explosion - this includes effects, physics force, sound

            GameObject explosionGo = Object.Instantiate(
                this.explosionPrefab,
                contactPoint,
                this.transform.rotation);

            var psm = explosionGo.GetComponentOrThrow<ParticleSystemMultiplier>();
            psm.multiplier = this.particleSystemMultiplier;

            NetManager.Spawn(explosionGo);

            // assign explosion sound
            F.RunExceptionSafe(() => Vehicle.AssignExplosionSound(explosionGo));

        }
    }
}
