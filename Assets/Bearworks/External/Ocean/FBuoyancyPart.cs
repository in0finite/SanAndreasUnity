using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace NOcean
{
    [DisallowMultipleComponent]
    public class FBuoyancyPart : MonoBehaviour
    {
        public float radius = 0.3f;

        [Range(0.0f, 10.0f)]
        public float stickyness = 5f;

        [Range(0.0f, 10.0f)]
        public float slope = 5f;
        
        [Range(0.0f, 1.0f)]
        public float drag = 0.1f;

        private float volume = 0f;
        
        private Vector3 outpos;
        
        private Vector3 normal;

        [NonSerialized]
        public bool usingGravity = false;

        [NonSerialized]
        public Vector3 force;

        [NonSerialized]
        public Vector3 torque;

        void Start()
        {
        }

        public void UpdateForces(Vector3 Pos, Vector3 Normal, Rigidbody body)
        {
            volume = (4.0f / 3.0f) * Mathf.PI * Mathf.Pow(radius, 3);

            Vector3 pos = transform.position;

            if (stickyness > 0f || slope > 0f)
            {
                outpos = Pos;
                normal = Normal;
            }
            else
            {
                outpos = Pos;
                normal = Vector3.up;
            }

            Vector3 delta = outpos - pos;
            float mergedVolume = CalculateMersionVolume(radius, delta.y);

            //assume that waterDensity = 1
            float Fb = mergedVolume;
            if(usingGravity)
            {
                Fb *= 9.80665f;
            }
            
            //make upwards for stable
            force = Vector3.up * Fb;
            
            Vector3 r = pos - body.worldCenterOfMass;

            torque = Vector3.Cross(r, force);

            if (drag > 0f && mergedVolume > 0f)
            {
                Vector3 velocity = body.velocity;// - grid.GetDrift();

                float vm = velocity.magnitude;
                velocity = -velocity.normalized * vm * vm;

                force += 0.5f * mergedVolume * velocity * drag;
            }

            if (stickyness > 0f)
            {
                force += Vector3.Scale(normal, delta) * stickyness;
            }

            if (slope > 0f)
            {
                torque += Vector3.Cross(body.transform.up, normal) * slope;
            }
        }

        float CalculateMersionVolume(float r, float dy)
        {
            float h = dy + radius;

            float d = 2.0f * r - h;

            if (d <= 0.0f)
            {
                return volume;
            }
            else if (d > 2.0f * r)
            {
                return 0.0f;
            }

            float c = Mathf.Sqrt(h * d);

            return Mathf.PI / 6.0f * h * ((3.0f * c * c) + (h * h));
        }

        void OnDrawGizmos()
        {
            if (!enabled) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawLine(outpos, outpos + normal * radius);
        }


    }
}
