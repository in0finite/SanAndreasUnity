using Mirror;
using SanAndreasUnity.Behaviours.Vehicles;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Net
{

    public class NetworkedExplosion : NetworkBehaviour
    {

        void Awake()
        {
            if (!NetStatus.IsServer)
            {
                // these scripts should not run on client
                this.gameObject.DestroyComponent<ExplosionForce>();
                this.gameObject.DestroyComponent<DestroyWhenParticleSystemsFinish>();
            }
        }

        public override void OnStartClient()
        {
            if (NetStatus.IsServer)
                return;

            F.RunExceptionSafe(() => Vehicle.AssignExplosionSound(this.gameObject));
        }

    }

}
