using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
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
        public float rotationSpeed = 180;
        public float lifeTime = 30;
        [SerializeField] private Transform m_modelAttachTransform = null;

        private bool m_alreadyExploded = false;
        public Rigidbody RigidBody { get; private set; }
        public AudioSource AudioSource { get; private set; }


        public static Projectile Create(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            AudioClip audioClip,
            Geometry.GeometryParts model,
            Ped shooterPed)
        {
            NetStatus.ThrowIfNotOnServer();

            var go = Instantiate(prefab, position, rotation);

            var projectile = go.GetComponentOrThrow<Projectile>();

            if (audioClip != null)
            {
                projectile.AudioSource.clip = audioClip;
                projectile.AudioSource.Play();
            }

            if (shooterPed != null)
            {
                var projectileCollider = projectile.GetComponentOrThrow<Collider>();
                var pedColliders = shooterPed.GetComponentsInChildren<Collider>();
                foreach (var pedCollider in pedColliders)
                {
                    Physics.IgnoreCollision(pedCollider, projectileCollider);
                }
            }

            if (model != null)
            {
                model.AttachFrames(projectile.m_modelAttachTransform, MaterialFlags.Default);
            }

            return projectile;
        }

        void Awake()
        {
            this.RigidBody = this.GetComponentOrThrow<Rigidbody>();
            this.AudioSource = this.GetComponentOrThrow<AudioSource>();
        }

        private void Start()
        {
            if (NetStatus.IsServer)
                Destroy(this.gameObject, this.lifeTime);

            this.RigidBody.velocity = this.transform.forward * this.speed;
        }

        private void Update()
        {
            float delta = this.rotationSpeed * Time.deltaTime * Random.Range (0.75f, 1.25f);
            m_modelAttachTransform.rotation *= Quaternion.AngleAxis (delta, Vector3.forward);
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
                DamageType.Explosion);

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
