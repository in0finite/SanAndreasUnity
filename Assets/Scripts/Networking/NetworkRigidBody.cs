using Mirror;
using UnityEngine;

namespace SanAndreasUnity.Net
{

    public class NetworkRigidBody : NetworkBehaviour
    {
        public Rigidbody Rigidbody { get; set; }

        [SyncVar] Vector3 m_net_position = Vector3.zero;
        [SyncVar] Vector3 m_net_rotation = Vector3.zero;
        [SyncVar] Vector3 m_net_velocity = Vector3.zero;
        [SyncVar] Vector3 m_net_angularVelocity = Vector3.zero;


        void Awake()
        {
            this.Rigidbody = this.GetComponent<Rigidbody>();

            if (NetStatus.IsServer)
            {
                this.UpdateServer();
                this.InvokeRepeating(nameof(this.UpdateServer), 0.001f, this.syncInterval);
            }
            else
            {
                this.InvokeRepeating(nameof(this.UpdateClient), 0.001f, this.syncInterval);
            }
        }

        public void UpdateServer()
        {
            if (null == this.Rigidbody)
                return;

            Vector3 pos = this.Rigidbody.position;
            Vector3 rot = this.Rigidbody.rotation.eulerAngles;
            Vector3 vel = this.Rigidbody.velocity;
            Vector3 angVel = this.Rigidbody.angularVelocity;

            if (pos != m_net_position)
            {
                m_net_position = pos;
            }

            if (rot != m_net_rotation)
            {
                m_net_rotation = rot;
            }

            if (vel != m_net_velocity)
            {
                m_net_velocity = vel;
            }

            if (angVel != m_net_angularVelocity)
            {
                m_net_angularVelocity = angVel;
            }

        }

        public void UpdateClient()
        {
            if (null == this.Rigidbody)
                return;

            this.Rigidbody.MovePosition(m_net_position);
            this.Rigidbody.MoveRotation(Quaternion.Euler(m_net_rotation));
            this.Rigidbody.velocity = m_net_velocity;
            this.Rigidbody.angularVelocity = m_net_angularVelocity;
        }

    }

}
