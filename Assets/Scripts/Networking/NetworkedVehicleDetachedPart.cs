using Mirror;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Net
{

    public class NetworkedVehicleDetachedPart : NetworkBehaviour
    {
        public NetworkRigidBody NetworkRigidBody { get; private set; }

        [SyncVar] uint m_net_vehicleId;
        [SyncVar] string m_net_frameName;
        [SyncVar] float m_net_mass;



        void Awake()
        {
            this.NetworkRigidBody = this.GetComponentOrThrow<NetworkRigidBody>();
        }

        public void InitializeOnServer(uint vehicleId, string frameName, float mass, Rigidbody rigidbody)
        {
            NetStatus.ThrowIfNotOnServer();

            m_net_vehicleId = vehicleId;
            m_net_frameName = frameName;
            m_net_mass = mass;

            this.NetworkRigidBody.Rigidbody = rigidbody;
            this.NetworkRigidBody.UpdateServer();
        }

        public override void OnStartClient()
        {
            if (NetStatus.IsServer)
                return;

            F.RunExceptionSafe(() => this.OnStartClientInternal());
        }

        void OnStartClientInternal()
        {
            GameObject vehicleGo = NetManager.GetNetworkObjectById(m_net_vehicleId);
            if (null == vehicleGo)
            {
                // this should not happen because vehicle must have been spawned before detached part
                Debug.LogError($"Can not find vehicle object with id {m_net_vehicleId} for detached part with id {this.netId}");
            }
            else
            {
                Vehicle vehicle = vehicleGo.GetComponentOrThrow<Vehicle>();
                vehicle.DetachFrameDuringExplosion(m_net_frameName, m_net_mass, this.gameObject);
                this.NetworkRigidBody.Rigidbody = this.GetComponentInChildren<Rigidbody>();
                this.NetworkRigidBody.UpdateClient();
            }
        }
    }

}
