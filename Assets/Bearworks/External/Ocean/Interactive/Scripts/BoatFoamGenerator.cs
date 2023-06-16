using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace NOcean
{
    public class BoatFoamGenerator : MonoBehaviour
    {
        public Transform boatTransform;
        private ParticleSystem.MainModule module;
        public ParticleSystem ps;

        private Vector3 offset;

        float3[] poses = new float3[1];
        float3[] heights = new float3[1];

        private void Start()
        {
            module = ps.main;
            offset = transform.localPosition;
        }

        // Update is called once per frame
        void Update()
        {
            var pos = boatTransform.TransformPoint(offset);

            poses[0] = pos;
            NeoOcean.GetData(poses, ref heights);

            pos.y = NeoOcean.oceanheight + heights[0].y;
            transform.position = pos;

            var fwd = boatTransform.forward;
            fwd.y = 0;
            var angle = Vector3.Angle(fwd.normalized, Vector3.forward);
            if(ps != null)
               module.startRotation = angle * Mathf.Deg2Rad;
        }


    }
}
