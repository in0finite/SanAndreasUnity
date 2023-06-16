using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace NOcean
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody))]
    public class FBuoyancyBody : MonoBehaviour
    {
        public float maxAngularVelocity = 0.05f;

        public FBuoyancyPart[] m_buoyancy;
        
        private Rigidbody body = null;

        private float3[] heights; // water height array offset to water levels
        private float3[] normals; // water normal array 
        private float3[] samplePoints; // sample points for height calc
        private int _guid; // GUID for the height system

        public void Start()
        {
            m_buoyancy = GetComponentsInChildren<FBuoyancyPart>();
            body = GetComponent<Rigidbody>();

            heights = new float3[m_buoyancy.Length];// new NativeSlice<float3>();
            normals = new float3[m_buoyancy.Length];//new NativeSlice<float3>();
            samplePoints = new float3[m_buoyancy.Length];

        }

        void Update()
        {
            int count = m_buoyancy.Length;

            for (int i = 0; i < count; i++)
            {
                FBuoyancyPart buoyancy = m_buoyancy[i];
                if (buoyancy == null) continue;
                if (!buoyancy.enabled) continue;

                samplePoints[i] = buoyancy.transform.position;
            }

            NeoOcean.GetData(samplePoints, ref heights, ref normals);
        }

        void FixedUpdate()
        {
            if (body == null)
                body = gameObject.AddComponent<Rigidbody>();
            
            Vector3 force = Vector3.zero;
            Vector3 torque = Vector3.zero;

            int count = m_buoyancy.Length;

            if (count == 0)
            {
                body.Sleep();
                return;
            }
            
            for (int i = 0; i < count; i++)
            {
                FBuoyancyPart buoyancy = m_buoyancy[i];
                if (buoyancy == null) continue;
                if (!buoyancy.enabled) continue;

                buoyancy.usingGravity = body.useGravity;
                buoyancy.UpdateForces(heights[i] + new float3(samplePoints[i].x, NeoOcean.oceanheight, samplePoints[i].z), normals[i], body);

                force += buoyancy.force;
                torque += buoyancy.torque;
            }
            
            body.maxAngularVelocity = maxAngularVelocity;
            body.AddForce(force);
            body.AddTorque(torque);

        }

    }
}