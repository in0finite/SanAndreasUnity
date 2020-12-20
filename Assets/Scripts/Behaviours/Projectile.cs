using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

public class Projectile : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float explosionDamageAmount = 1000;
    public float explosionDamageRadius = 5;
    public LayerMask collisionLayerMask;
    public float speed = 10;
    public float lifeTime = 30;

    private bool m_alreadyExploded = false;
    private Rigidbody m_rigidBody;
    private AudioSource m_audioSource;


    public static Projectile Create(
        GameObject prefab, Vector3 position, Quaternion rotation, AudioClip audioClip)
    {
        var go = Instantiate(prefab, position, rotation);

        var projectile = go.GetComponentOrThrow<Projectile>();

        if (audioClip != null)
        {
            projectile.m_audioSource.clip = audioClip;
            projectile.m_audioSource.Play();
        }



        return projectile;
    }

    void Awake()
    {
        m_rigidBody = this.GetComponentOrThrow<Rigidbody>();
        m_audioSource = this.GetComponentOrThrow<AudioSource>();
    }

    private void Start()
    {
        Destroy(this.gameObject, this.lifeTime);
        m_rigidBody.velocity = this.transform.forward * this.speed;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (m_alreadyExploded)
            return;

        if (((1 << other.gameObject.layer) & this.collisionLayerMask.value) == 0)
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

        if (NetStatus.IsServer)
            NetManager.Spawn(explosionGo);

        // assign explosion sound
        F.RunExceptionSafe(() => Vehicle.AssignExplosionSound(explosionGo));

    }
}
