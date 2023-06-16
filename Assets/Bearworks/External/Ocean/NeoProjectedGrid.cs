using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace NOcean
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class NeoProjectedGrid : NeoNormalGrid
    {
        [Range(32, 254)]
        public int usedGridSize = 254;

        public float usedOceanHeight = 0f;

        public Vector4 minBias = new Vector4(1, 1, 0.3f, 15f);

        protected float m_gridsize;

        private Vector4 foCorners0, foCorners1, foCorners2, foCorners3;

        private Plane basePlane;

        private Camera projectorCamera = null;

        protected override void Init()
        {
            if (NeoOcean.instance == null)
                return;

            GetComponent<Renderer>().sharedMaterial = oceanMaterial;
            GetComponent<Renderer>().enabled = true;

            GameObject go = new GameObject();
            projectorCamera = go.AddComponent<Camera>();
            projectorCamera.transform.parent = this.transform.parent.transform;
            projectorCamera.enabled = false;
            projectorCamera.cullingMask = 0;
            projectorCamera.targetTexture = null;
            go.hideFlags = HideFlags.HideAndDontSave;

            GenMesh();

            NeoOcean.instance.AddPG(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RenderPipelineManager.beginCameraRendering += ExecuteProjectedGrid;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            RenderPipelineManager.beginCameraRendering -= ExecuteProjectedGrid;
        }

        public void ExecuteProjectedGrid(ScriptableRenderContext context, Camera camera)
        {
            Matrix4x4 m_Range = Matrix4x4.identity;
            //make new pvMat
            GetComponent<Renderer>().enabled = GetMinMax(ref m_Range, camera);

            if (!GetComponent<Renderer>().enabled)
            {
                return;
            }

            if (!oceanMaterial)
            {
                return;
            }
            
            Vector2 cornertmp = Vector2.zero;
            foCorners0 = CalculeLocalPosition(ref cornertmp, ref m_Range);
            cornertmp = new Vector2(+1.0f, 0.0f);
            foCorners1 = CalculeLocalPosition(ref cornertmp, ref m_Range);
            cornertmp = new Vector2(0.0f, +1.0f);
            foCorners2 = CalculeLocalPosition(ref cornertmp, ref m_Range);
            cornertmp = new Vector2(+1.0f, +1.0f);
            foCorners3 = CalculeLocalPosition(ref cornertmp, ref m_Range);

            oceanMaterial.SetVector("_FoCenter", new Vector4(camera.transform.position.x, 0f, camera.transform.position.z, 0f));

            oceanMaterial.EnableKeyword("_PROJECTED_ON");

            oceanMaterial.SetVector("_FoCorners0", foCorners0);
            oceanMaterial.SetVector("_FoCorners1", foCorners1);
            oceanMaterial.SetVector("_FoCorners2", foCorners2);
            oceanMaterial.SetVector("_FoCorners3", foCorners3);
        }

        void GenMesh()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if (meshFilter.sharedMesh != null)
                DestroyImmediate(meshFilter.sharedMesh);

            GetComponent<Renderer>().receiveShadows = false;
            GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;

            meshFilter.sharedMesh = mesh;

            m_gridsize = usedGridSize;

            int numEle = 6 * usedGridSize * usedGridSize;
            int numVert = (usedGridSize + 1) * (usedGridSize + 1);

            int[] indices = new int[numEle];

            int i = 0;
            for (int v = 0; v < usedGridSize; v++)
            {
                for (int u = 0; u < usedGridSize; u++)
                {
                    if (u % 2 == 0)
                    {
                        // face 1 |/
                        indices[i++] = v * (usedGridSize + 1) + u;
                        indices[i++] = (v + 1) * (usedGridSize + 1) + u;
                        indices[i++] = v * (usedGridSize + 1) + u + 1;

                        // face 2 /|
                        indices[i++] = (v + 1) * (usedGridSize + 1) + u;
                        indices[i++] = (v + 1) * (usedGridSize + 1) + u + 1;
                        indices[i++] = v * (usedGridSize + 1) + u + 1;
                    }
                    else
                    {
                        // face 1 |\                                               //
                        indices[i++] = v * (usedGridSize + 1) + u;
                        indices[i++] = (v + 1) * (usedGridSize + 1) + u + 1;
                        indices[i++] = v * (usedGridSize + 1) + u + 1;

                        // face 2 \|
                        indices[i++] = (v + 1) * (usedGridSize + 1) + u + 1;
                        indices[i++] = v * (usedGridSize + 1) + u;
                        indices[i++] = (v + 1) * (usedGridSize + 1) + u;
                    }
                }
            }

            Vector3[] vertices = new Vector3[numVert];

            float du = 1.0f / (usedGridSize);
            float dv = 1.0f / (usedGridSize);

            float cv = 0.0f;
            for (int v = 0; v < (usedGridSize + 1); v++)
            {
                float cu = 0.0f;
                for (int u = 0; u < (usedGridSize + 1); u++)
                {
                    i = (usedGridSize + 1) * v + u;

                    vertices[i].x = cv;
                    vertices[i].z = cu;

                    vertices[i].y = 0;

                    cu += du;
                }
                cv += dv;
            }

            mesh.vertices = vertices;
            mesh.triangles = indices;

            const float maxBound = 1e5f; //far close to infi
            mesh.bounds = new Bounds(Vector3.zero, maxBound * Vector3.one);

            meshFilter.sharedMesh = mesh;
        }

        protected override void OnDestroy()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                DestroyImmediate(meshFilter.sharedMesh);
            }

            GetComponent<Renderer>().enabled = false;

            if (NeoOcean.instance != null)
                NeoOcean.instance.RemovePG(this);

        }

        public override void LateUpdate()
        {
            if (NeoOcean.instance == null)
                return;

            NeoOcean.oceanheight = usedOceanHeight;

            basePlane = new Plane(Vector3.up, usedOceanHeight * Vector3.up);

            if (m_gridsize != usedGridSize)
            {
                GenMesh();
            }
        }

        // Check the point of intersection with the plane (0,y,0,0) and return the position in homogenous coordinates 
        Vector4 CalculeLocalPosition(ref Vector2 uv, ref Matrix4x4 m)
        {
            Vector4 ori = new Vector4(uv.x, uv.y, -1, 1);
            Vector4 dir = new Vector4(uv.x, uv.y, 1, 1);
            Vector4 localPos = Vector4.zero;

            ori = m * ori;
            dir = m * dir;
            float wh = ori.w * basePlane.distance;
            dir -= ori;
            float dwh = dir.w * basePlane.distance;
            float l = -(wh + ori.y) / (dir.y + dwh);
            localPos = ori + dir * l;

            return localPos;
        }

        public void ProjectToWorld(out Vector3 vert, float u, float v)
        {
            float _1_u = 1 - u;
            float _1_v = 1 - v;
            vert.x = _1_v * (_1_u * foCorners0.x + u * foCorners1.x) + v * (_1_u * foCorners2.x + u * foCorners3.x);
            vert.y = _1_v * (_1_u * foCorners0.y + u * foCorners1.y) + v * (_1_u * foCorners2.y + u * foCorners3.y);
            vert.z = _1_v * (_1_u * foCorners0.z + u * foCorners1.z) + v * (_1_u * foCorners2.z + u * foCorners3.z);

            float w = _1_v * (_1_u * foCorners0.w + u * foCorners1.w) + v * (_1_u * foCorners2.w + u * foCorners3.w);
            vert /= w;
        }

        [NonSerialized]
        public float offsetToGridPlane = 0.0f;

        Vector3[] frustum = new Vector3[8];
        Vector3[] proj_points = new Vector3[24];
        static int[] cube =
        {
            0, 1, 0, 2, 2, 3, 1, 3,
            0, 4, 2, 6, 3, 7, 1, 5,
            4, 6, 4, 5, 5, 7, 6, 7
        };

        public float UpdateCameraPlane(float fAdd, Camera cam)
        {
            return Mathf.Max(cam.nearClipPlane, cam.farClipPlane + fAdd);
        }

        private bool GetMinMax(ref Matrix4x4 range, Camera cam)
        {
            if (!projectorCamera)
                return false;

            int i, n_points = 0, src, dst;

            Vector3 testLine;
            float dist;

            const float yError = -0.00001f;
            if (cam.transform.forward.y == 0.0f) cam.transform.forward += new Vector3(0f, yError, 0f);

            // Set temporal rendering camera parameters
            projectorCamera.CopyFrom(cam);
            projectorCamera.enabled = false;
            projectorCamera.cullingMask = 0;
            projectorCamera.depth = -1;
            projectorCamera.targetTexture = null;

            Vector3 localCamerapos = new Vector3(0f, cam.transform.position.y, 0f);
            projectorCamera.transform.position = localCamerapos;
            gameObject.transform.position = new Vector3(cam.transform.position.x, 0, cam.transform.position.z);
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            float height_in_plane = basePlane.GetDistanceToPoint(projectorCamera.transform.position);
            offsetToGridPlane = Mathf.Abs(height_in_plane);

            Vector3 up = (minBias.w + usedOceanHeight) * Vector3.up;
            Vector3 down = (-minBias.w + usedOceanHeight) * Vector3.up;

            float farPlane;

            //todo : check complete under
            if (cam.transform.position.y - usedOceanHeight < -1f)
            {
                Vector3 euler = projectorCamera.transform.eulerAngles;
                projectorCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

                projectorCamera.transform.position = new Vector3(projectorCamera.transform.position.x, usedOceanHeight * 2f - projectorCamera.transform.position.y, projectorCamera.transform.position.z);
            }

            if (Mathf.Abs(height_in_plane) < minBias.w)
            {
                projectorCamera.transform.position += Vector3.up * (minBias.w - offsetToGridPlane);

                farPlane = UpdateCameraPlane(0, cam);
            }
            else
            {
                float viewLayerRatio = Mathf.Cos(Mathf.Deg2Rad * cam.fieldOfView);
                float viewLayer = offsetToGridPlane / viewLayerRatio;

                projectorCamera.transform.position += Vector3.up * (viewLayer - offsetToGridPlane);

                up = (viewLayer + usedOceanHeight) * Vector3.up;
                down = (-viewLayer + usedOceanHeight) * Vector3.up;
                farPlane = UpdateCameraPlane(viewLayer + usedOceanHeight, cam);
            }

            projectorCamera.farClipPlane = farPlane * minBias.z;

            Plane UpperBoundPlane = new Plane(Vector3.up, up);
            Plane LowerBoundPlane = new Plane(Vector3.up, down);

            Matrix4x4 invviewproj = (projectorCamera.projectionMatrix * projectorCamera.worldToCameraMatrix).inverse;
            //into world space w = 1  
            frustum[0] = invviewproj.MultiplyPoint(new Vector3(-1, -1, -1));
            frustum[1] = invviewproj.MultiplyPoint(new Vector3(+1, -1, -1));
            frustum[2] = invviewproj.MultiplyPoint(new Vector3(-1, +1, -1));
            frustum[3] = invviewproj.MultiplyPoint(new Vector3(+1, +1, -1));
            frustum[4] = invviewproj.MultiplyPoint(new Vector3(-1, -1, +1));
            frustum[5] = invviewproj.MultiplyPoint(new Vector3(+1, -1, +1));
            frustum[6] = invviewproj.MultiplyPoint(new Vector3(-1, +1, +1));
            frustum[7] = invviewproj.MultiplyPoint(new Vector3(+1, +1, +1));

            for (i = 0; i < 12; i++)
            {
                src = cube[i * 2];
                dst = cube[i * 2 + 1];
                testLine = frustum[dst] - frustum[src];
                dist = testLine.magnitude;
                testLine.Normalize();
                Ray ray = new Ray(frustum[src], testLine);
                float interactdis = 0.0f;
                bool result = UpperBoundPlane.Raycast(ray, out interactdis);
                if (result && (interactdis < dist + 0.00001))
                {
                    proj_points[n_points++] = frustum[src] + interactdis * testLine;
                }
                result = LowerBoundPlane.Raycast(ray, out interactdis);
                if (result && (interactdis < dist + 0.00001))
                {
                    proj_points[n_points++] = frustum[src] + interactdis * testLine;
                }
            }

            // Check if any of the frustums vertices lie between the upper_bound and lower_bound planes
            for (i = 0; i < 8; i++)
            {
                if (UpperBoundPlane.GetDistanceToPoint(frustum[i]) * LowerBoundPlane.GetDistanceToPoint(frustum[i]) < 0)
                {
                    proj_points[n_points++] = frustum[i];
                }
            }

            if (n_points == 0)
                return false;

            Vector3 aimpoint, aimpoint2;

            // Aim the projector at the point where the camera view-vector intersects the plane
            // if the camera is aimed away from the plane, mirror it's view-vector against the plane
            float forwardy = projectorCamera.transform.forward.y;
            if (forwardy < 0.0f)
            {
                Ray ray = new Ray(projectorCamera.transform.position, projectorCamera.transform.forward);
                float interactdis = 0.0f;
                bool _result = basePlane.Raycast(ray, out interactdis);

                if (false == _result)
                {
                    return false;
                }

                aimpoint = projectorCamera.transform.position + interactdis * projectorCamera.transform.forward;
            }
            else
            {
                Vector3 flipped = projectorCamera.transform.forward -
                    2 * Vector3.up * projectorCamera.transform.forward.y;
                flipped.Normalize();
                float interactdis = 0.0f;
                Ray ray = new Ray(projectorCamera.transform.position, flipped);
                bool _result = basePlane.Raycast(ray, out interactdis);

                if (false == _result)
                {
                    return false;
                }

                aimpoint = projectorCamera.transform.position + interactdis * flipped;
            }

            // Force the point the camera is looking at in a plane, and have the projector look at it
            // works well against horizon, even when camera is looking upwards
            // doesn't work straight down/up
            float af;
            af = Mathf.Abs(cam.transform.forward.y);
            aimpoint2 = localCamerapos + Mathf.Abs(cam.transform.position.y - usedOceanHeight) * cam.transform.forward;

            aimpoint2 -= Vector3.up * (aimpoint2.y - usedOceanHeight);

            // Fade between aimpoint & aimpoint2 depending on view angle
            aimpoint = aimpoint * af + aimpoint2 * (1.0f - af);


	        projectorCamera.transform.forward = (aimpoint - projectorCamera.transform.position);

            Matrix4x4 pvMat = projectorCamera.projectionMatrix * projectorCamera.worldToCameraMatrix;
            for (i = 0; i < n_points; i++)
            {
                // Project the point onto the surface plane
                proj_points[i] -= Vector3.up * (proj_points[i].y - usedOceanHeight);
                proj_points[i] = pvMat.MultiplyPoint(proj_points[i]);
            }

            //x,y minmax coordinates in projectorCamera clip space
            float x_min, y_min, x_max, y_max;

            // Get max/min x & y-values to determine how big the "projection window" must be
            x_min = proj_points[0].x;
            x_max = proj_points[0].x;
            y_min = proj_points[0].y;
            y_max = proj_points[0].y;

            for (i = 1; i < n_points; i++)
            {
                if (proj_points[i].x > x_max) x_max = proj_points[i].x;
                if (proj_points[i].x < x_min) x_min = proj_points[i].x;
                if (proj_points[i].y > y_max) y_max = proj_points[i].y;
                if (proj_points[i].y < y_min) y_min = proj_points[i].y;
            }

            // Build the packing matrix that spreads the grid across the "projection window"

            range.SetRow(0, new Vector4(x_max - x_min, 0, 0, x_min) * minBias.x);
            range.SetRow(1, new Vector4(0, y_max - y_min + minBias.y, 0, y_min - minBias.y));
            range.SetRow(2, new Vector4(0, 0, 1, 0));
            range.SetRow(3, new Vector4(0, 0, 0, 1));

            range = pvMat.inverse * range;

            return true;
        }
        
    }
}