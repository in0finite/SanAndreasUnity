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


        void Awake()
        {
            this.Rigidbody = this.GetComponent<Rigidbody>();
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (null == this.Rigidbody)
                return;

            byte bitfield = reader.ReadByte();

            if ((bitfield & 1) != 0)
            {
                this.Rigidbody.MovePosition(reader.ReadVector3());
            }

            if ((bitfield & 2) != 0)
            {
                this.Rigidbody.MoveRotation(Quaternion.Euler(reader.ReadVector3()));
            }

            if ((bitfield & 4) != 0)
            {
                this.Rigidbody.velocity = reader.ReadVector3();
            }

            if ((bitfield & 8) != 0)
            {
                this.Rigidbody.angularVelocity = reader.ReadVector3();
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

    }

}
