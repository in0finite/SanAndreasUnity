using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public class PushableByDamage : MonoBehaviour
    {
        public float forceMultiplier = 1;
        private Damageable _damageable;


        private void Awake()
        {
            _damageable = this.GetComponentOrThrow<Damageable>();
            _damageable.OnDamageEvent.AddListener(this.OnDamaged);
        }

        void OnDamaged()
        {
            if (!NetUtils.IsServer)
                return;

            DamageInfo damageInfo = _damageable.LastDamageInfo;

            if (damageInfo.damageType != DamageType.Bullet)
                return;

            if (null == damageInfo.raycastHitTransform)
                return;

            if (!damageInfo.raycastHitTransform.TryGetComponent<Collider>(out var c))
                return;

            var rb = c.attachedRigidbody;
            if (null == rb)
                return;

            rb.AddForceAtPosition(
                damageInfo.amount.SqrtOrZero() * this.forceMultiplier * damageInfo.hitDirection,
                damageInfo.hitPoint,
                ForceMode.Impulse);
        }
    }
}
