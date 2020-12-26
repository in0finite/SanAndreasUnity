using Mirror;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Net
{
    public class NetworkedProjectile : NetworkBehaviour
    {
        private Projectile m_projectile;

        private void Awake()
        {
            m_projectile = this.GetComponentOrThrow<Projectile>();

            if (!NetStatus.IsServer)
            {
                F.RunExceptionSafe(() => Vehicle.AssignExplosionSound(this.gameObject));

                // disable all colliders
                var colliders = this.gameObject.GetComponentsInChildren<Collider>();
                foreach (var c in colliders)
                {
                    c.enabled = false;
                }
            }
        }
    }
}
