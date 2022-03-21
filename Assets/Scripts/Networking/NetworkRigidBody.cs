using Mirror;
using UnityEngine;

namespace SanAndreasUnity.Net
{

    public class NetworkRigidBody : NetworkBehaviour
    {
        private Rigidbody _rigidbody;
        public Rigidbody Rigidbody
        {
            get => _rigidbody;
            set
            {
                if (_rigidbody == value)
                    return;

                _rigidbody = value;

                if (NetStatus.IsClientOnly)
                {
                    this.UpdateAllPropertiesOnClient();
                }
            }
        }

        [SyncVar(hook=nameof(OnNetPositionChanged))] Vector3 m_net_position = Vector3.zero;
        [SyncVar(hook=nameof(OnNetRotationChanged))] Vector3 m_net_rotation = Vector3.zero;
        [SyncVar] Vector3 m_net_velocity = Vector3.zero;
        [SyncVar] Vector3 m_net_angularVelocity = Vector3.zero;

        public bool disableCollisionDetectionOnClients = true;
        public bool disableGravityOnClients = true;


        void Awake()
        {
            this.Rigidbody = this.GetComponent<Rigidbody>();

            if (NetStatus.IsServer)
            {
                // assign syncvars before the object is spawned on network
                this.UpdateServer();
            }

            // suggest that interpolation should be changed
            if (!NetStatus.IsServer)
            {
                if (this.Rigidbody != null && this.Rigidbody.interpolation == RigidbodyInterpolation.None)
                    Debug.LogWarning($"For better sync, interpolation should be changed, rigid body: {this.Rigidbody.name}");
            }
        }

        public override void OnStartClient()
        {
            if (NetStatus.IsServer)
                return;

            // need to apply initial syncvar values, because otherwise the object may stay on the place where it
            // was originally spawned on the server (if object doesn't change position, syncvars will not be updated)

            this.UpdateAllPropertiesOnClient();
        }

        void Update()
        {
            if (NetStatus.IsServer)
                this.UpdateServer();
            else
                this.UpdateClient();
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

        private void UpdateClient()
        {
            if (null == this.Rigidbody)
                return;

            if (this.disableCollisionDetectionOnClients)
                this.Rigidbody.detectCollisions = false;

            if (this.disableGravityOnClients)
                this.Rigidbody.useGravity = false;

            // position and rotation are updated in syncvar hooks

            this.Rigidbody.velocity = m_net_velocity;
            this.Rigidbody.angularVelocity = m_net_angularVelocity;
        }

        private void UpdateAllPropertiesOnClient()
        {
            if (null == this.Rigidbody)
                return;

            this.Rigidbody.MovePosition(m_net_position);
            this.Rigidbody.MoveRotation(Quaternion.Euler(m_net_rotation));
            this.UpdateClient();
        }

        void OnNetPositionChanged(Vector3 oldPos, Vector3 newPos)
        {
            if (NetStatus.IsServer)
                return;

            if (null == this.Rigidbody)
                return;

            this.Rigidbody.MovePosition(newPos);
        }

        void OnNetRotationChanged(Vector3 oldEulers, Vector3 newEulers)
        {
            if (NetStatus.IsServer)
                return;

            if (null == this.Rigidbody)
                return;

            this.Rigidbody.MoveRotation(Quaternion.Euler(newEulers));
        }

    }

}
