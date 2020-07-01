using Mirror;
using UnityEngine;

namespace SanAndreasUnity.Net
{

    public class NetworkRigidBody : NetworkBehaviour
    {
        public Rigidbody Rigidbody { get; set; }

        Vector3 m_lastPosition = Vector3.zero;
        Vector3 m_lastRotation = Vector3.zero;
        Vector3 m_lastVelocity = Vector3.zero;
        Vector3 m_lastAngularVelocity = Vector3.zero;

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

        /*
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            bool hasRigidBody = this.Rigidbody != null;

            byte bitfield = reader.ReadByte();

            //Vector3 pos = Vector3.zero;
            //Vector3 rot = Vector3.zero;
            //Vector3 vel = Vector3.zero;
            //Vector3 angVel = Vector3.zero;

            if ((bitfield & 1) != 0)
            {
                Vector3 pos = reader.ReadVector3();
                if (hasRigidBody)
                    this.Rigidbody.MovePosition(pos);
            }

            if ((bitfield & 2) != 0)
            {
                Vector3 rot = reader.ReadVector3();
                if (hasRigidBody)
                    this.Rigidbody.MoveRotation(Quaternion.Euler(rot));
            }

            if ((bitfield & 4) != 0)
            {
                Vector3 vel = reader.ReadVector3();
                if (hasRigidBody)
                    this.Rigidbody.velocity = vel;
            }

            if ((bitfield & 8) != 0)
            {
                Vector3 angVel = reader.ReadVector3();
                if (hasRigidBody)
                    this.Rigidbody.angularVelocity = angVel;
            }

        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (null == this.Rigidbody)
                return false;

            Vector3 pos = this.Rigidbody.position;
            Vector3 rot = this.Rigidbody.rotation.eulerAngles;
            Vector3 vel = this.Rigidbody.velocity;
            Vector3 angVel = this.Rigidbody.angularVelocity;

            if (initialState)
            {
                writer.Write((byte) byte.MaxValue);
                writer.Write(pos);
                writer.Write(rot);
                writer.Write(vel);
                writer.Write(angVel);

                m_lastPosition = pos;
                m_lastRotation = rot;
                m_lastVelocity = vel;
                m_lastAngularVelocity = angVel;

                return true;
            }

            int startingWriterPosition = writer.Position;
            writer.Write((byte)0);

            byte bitfield = 0;

            if (pos != m_lastPosition)
            {
                m_lastPosition = pos;
                writer.Write(pos);
                bitfield |= 1;
            }

            if (rot != m_lastRotation)
            {
                m_lastRotation = rot;
                writer.Write(rot);
                bitfield |= 2;
            }

            if (vel != m_lastVelocity)
            {
                m_lastVelocity = vel;
                writer.Write(vel);
                bitfield |= 4;
            }

            if (angVel != m_lastAngularVelocity)
            {
                m_lastAngularVelocity = angVel;
                writer.Write(angVel);
                bitfield |= 8;
            }

            int endWriterPosition = writer.Position;

            writer.Position = startingWriterPosition;
            writer.Write(bitfield);

            writer.Position = endWriterPosition;

            return bitfield != 0;    // is dirty
        }
        */

    }

}
