using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Mathematics;

namespace NOcean
{
    /// <summary>
    /// Handles effects that need to track the water surface. Feeds in wave data and disables rendering when
    /// not close to water.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class UnderwaterEffect : MonoBehaviour
    {
		private int _guid; // GUID for the height system
		
        const int GEOM_HORIZ_DIVISIONS = 64;

        Renderer _rend;

        private void Start()
        {
            _rend = GetComponent<Renderer>();
			
            GetComponent<MeshFilter>().mesh = Mesh2DGrid(0, 2, -0.5f, -0.5f, 1f, 1f, GEOM_HORIZ_DIVISIONS, 1);
        }
		
	    void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += ExecuteCheckUnder;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= ExecuteCheckUnder;
        }

		float3[] fofrustum = new float3[5];
        float3[] heights = new float3[5];
        public void ExecuteCheckUnder(ScriptableRenderContext context, Camera camera)
        {
            if (_rend == null)
                return;

            Matrix4x4 invviewproj = (camera.projectionMatrix * camera.worldToCameraMatrix).inverse;

            fofrustum[0] = invviewproj.MultiplyPoint(new Vector3(-1, -1, -1));
            fofrustum[1] = invviewproj.MultiplyPoint(new Vector3(+1, -1, -1));
            fofrustum[2] = invviewproj.MultiplyPoint(new Vector3(-1, +1, -1));
            fofrustum[3] = invviewproj.MultiplyPoint(new Vector3(+1, +1, -1));
            fofrustum[4] = camera.transform.position;

            NeoOcean.GetData(fofrustum, ref heights);

            _rend.enabled = false;

            int i = 0;
            for (; i < 5; i++)
            {
                Vector3 contactP = heights[i] + new float3(fofrustum[i].x, NeoOcean.oceanheight, fofrustum[i].z);
                if (contactP.y - fofrustum[i].y > 0)
                {
                    // Disable skirt when camera not close to water. In the first few frames collision may not be avail, in that case no choice
                    // but to assume enabled. In the future this could detect if camera is far enough under water, render a simple quad to avoid
                    // finding the intersection line.
                    _rend.enabled = true;
                    //break;
                }
            }

            bool bAll = false;
            if(i == 5)
                bAll = true;
			
            if (_rend.enabled)
            {
                //avoid culling
                this.transform.position = camera.transform.position + camera.transform.forward;

                _rend.sharedMaterial.SetFloat("_OceanHeight", NeoOcean.oceanheight);
                _rend.sharedMaterial.SetFloat("_HeightOffset", bAll ? -2 : 0);
            }
        }

        private void Update()
        {
            if (_rend == null)
                return;

            if (NeoOcean.instance == null)
                return;

            _rend.sharedMaterial.SetTexture("_Map0", NeoOcean.instance.WaveMap);
            _rend.sharedMaterial.SetFloat("_InvNeoScale", NeoOcean.mainScale);
        }

        static Mesh Mesh2DGrid(int dim0, int dim1, float start0, float start1, float width0, float width1, int divs0, int divs1)
        {
            Vector3[] verts = new Vector3[(divs1 + 1) * (divs0 + 1)];
            Vector2[] uvs = new Vector2[(divs1 + 1) * (divs0 + 1)];
            float dx0 = width0 / divs0, dx1 = width1 / divs1;
            for (int i1 = 0; i1 < divs1 + 1; i1++)
            {
                float v = i1 / (float)divs1;

                for (int i0 = 0; i0 < divs0 + 1; i0++)
                {
                    int i = (divs0 + 1) * i1 + i0;
                    verts[i][dim0] = start0 + i0 * dx0;
                    verts[i][dim1] = start1 + i1 * dx1;

                    uvs[i][0] = i0 / (float)divs0;
                    uvs[i][1] = v;
                }
            }

            int[] indices = new int[divs0 * divs1 * 2 * 3];
            for (int i1 = 0; i1 < divs1; i1++)
            {
                for (int i0 = 0; i0 < divs0; i0++)
                {
                    int i00 = (divs0 + 1) * (i1 + 0) + (i0 + 0);
                    int i01 = (divs0 + 1) * (i1 + 0) + (i0 + 1);
                    int i10 = (divs0 + 1) * (i1 + 1) + (i0 + 0);
                    int i11 = (divs0 + 1) * (i1 + 1) + (i0 + 1);

                    int tri;

                    tri = 0;
                    indices[(i1 * divs0 + i0) * 6 + tri * 3 + 0] = i00;
                    indices[(i1 * divs0 + i0) * 6 + tri * 3 + 1] = i11;
                    indices[(i1 * divs0 + i0) * 6 + tri * 3 + 2] = i01;
                    tri = 1;
                    indices[(i1 * divs0 + i0) * 6 + tri * 3 + 0] = i00;
                    indices[(i1 * divs0 + i0) * 6 + tri * 3 + 1] = i10;
                    indices[(i1 * divs0 + i0) * 6 + tri * 3 + 2] = i11;
                }
            }

            var mesh = new Mesh();
            mesh.name = "Grid2D_" + divs0 + "x" + divs1;
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1e2f);
            return mesh;
        }
    }
}
