using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Placements;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class Water : MonoBehaviour
    {
        public static string LayerName => "Water";
        public static int LayerMask => 1 << UnityEngine.LayerMask.NameToLayer(LayerName);

        public GameObject WaterPrefab;
        [SerializeField] private GameObject m_waterCollisionPrefab;

        [SerializeField] private bool m_createCollisionObjects = false;
        [SerializeField] private float m_collisionHeight = 200f;
        [SerializeField] private float m_shallowCollisionHeight = 10f;
        [SerializeField] private bool m_createVisualsForCollisionObjects = false;

        [HideInInspector] [SerializeField] private List<Transform> m_renderingObjects = new List<Transform>();
        [HideInInspector] [SerializeField] private List<Transform> m_collisionObjects = new List<Transform>();


        public void Initialize(Vector2 worldSize)
        {
            if (this.WaterPrefab == null)
            {
                Debug.LogError("No water prefab set, skipping load!");
                return;
            }

            WaterFile file = new WaterFile(Importing.Archive.ArchiveManager.PathToCaseSensitivePath(Config.GetPath("water_path")));

            // TODO: what to do with faces that don't have WaterFlags.Visible flag ?

            var faces = file.Faces.Where(f => (f.Flags & WaterFlags.Visible) == WaterFlags.Visible);

            // we need total of 4 quads for "infinite" water:
            // - upper side (from upper world boundary to positive infinity)
            // - lower side (from lower world boundary to negative infinity)
            // - left side (between first 2 quads)
            // - right side (between first 2 quads)

            const int numQuadsForInfiniteWater = 4;

            var vertices = new List<Vector3>(1536);
            var normals = new List<Vector3>(1536);
            var indices = new List<int>(2048);

            int verticesIndex = 0;
            int indicesIndex = 0;

            foreach (WaterFace face in faces)
                this.ProcessFaceForRenderMesh(face, vertices, normals, indices);

            // add "infinite" water

            const float infiniteWaterOffset = 20000f;
            // upper quad
            CreateQuad(
                new Vector2( -worldSize.x / 2f - infiniteWaterOffset, worldSize.y / 2f),
                new Vector2(worldSize.x / 2f + infiniteWaterOffset, worldSize.y / 2f + infiniteWaterOffset),
                vertices,
                normals,
                indices);
            // lower quad
            CreateQuad(
                new Vector2( -worldSize.x / 2f - infiniteWaterOffset, - worldSize.y / 2f - infiniteWaterOffset),
                new Vector2(worldSize.x / 2f + infiniteWaterOffset, - worldSize.y / 2f),
                vertices,
                normals,
                indices);
            // left quad
            CreateQuad(
                new Vector2( -worldSize.x / 2f - infiniteWaterOffset, - worldSize.y / 2f),
                new Vector2(- worldSize.x / 2f, worldSize.y / 2f),
                vertices,
                normals,
                indices);
            // right quad
            CreateQuad(
                new Vector2( worldSize.x / 2f, - worldSize.y / 2f),
                new Vector2(worldSize.x / 2f + infiniteWaterOffset, worldSize.y / 2f),
                vertices,
                normals,
                indices);

            var mesh = new Mesh();

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            m_renderingObjects.RemoveDeadObjects();

            var availableObjects = m_renderingObjects.ToQueueWithCapacity(m_renderingObjects.Count);

            var go = availableObjects.Count > 0
                ? availableObjects.Dequeue().gameObject
                : Instantiate(this.WaterPrefab, this.transform);

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            go.name = "Water rendering mesh";
            mesh.name = go.name;

            var meshFilter = go.GetComponentOrThrow<MeshFilter>();
            if (meshFilter.sharedMesh != null && !EditorUtilityEx.IsAsset(meshFilter.sharedMesh))
                F.DestroyEvenInEditMode(meshFilter.sharedMesh);
            meshFilter.sharedMesh = mesh;

            foreach (var availableObject in availableObjects)
                F.DestroyEvenInEditMode(availableObject.gameObject);

            m_renderingObjects.Clear();
            m_renderingObjects.Add(go.transform);


            if (m_createCollisionObjects)
                CreateCollisionObjects(faces);

        }

        public (Vector3 center, Vector3 size) GetCenterAndSize(WaterFace face)
        {
            Vector3 min = Vector3.positiveInfinity;
            Vector3 max = Vector3.negativeInfinity;
            for (int v = 0; v < face.Vertices.Length; v++)
            {
                min = MathUtils.MinComponents(min, face.Vertices[v].Position);
                max = MathUtils.MaxComponents(max, face.Vertices[v].Position);
            }

            Vector3 center = (min + max) * 0.5f;
            if (this.IsInterior(center.y))
                center.y += Cell.Singleton.interiorHeightOffset;

            bool isShallow = (face.Flags & WaterFlags.Shallow) != 0;

            Vector3 size = max - min;
            size.y = isShallow ? m_shallowCollisionHeight : m_collisionHeight;

            return (center, size);
        }

        void ProcessFaceForRenderMesh(
            WaterFace face, List<Vector3> vertices, List<Vector3> normals, List<int> indices)
        {
            int verticesIndex = vertices.Count;
            int indicesIndex = indices.Count;

            bool isInterior = this.IsInterior(face);

            for (int v = 0; v < face.Vertices.Length; v++)
            {
                vertices.Add(isInterior
                    ? face.Vertices[v].Position.WithAddedY(Cell.Singleton.interiorHeightOffset)
                    : face.Vertices[v].Position);

                normals.Add(Vector3.up);
            }

            int numIndices = (face.Vertices.Length - 2) * 3;
            indices.AddMultiple(numIndices);

            for (int i = 0; i < face.Vertices.Length - 2; ++i)
            {
                int flip = i & 1;
                indices[indicesIndex + i * 3 + 0] = verticesIndex + i + 1 - flip;
                indices[indicesIndex + i * 3 + 1] = verticesIndex + i + 0 + flip;
                indices[indicesIndex + i * 3 + 2] = verticesIndex + i + 2;
            }
        }

        void CreateCollisionObjects(IEnumerable<WaterFace> faces)
        {
            if (null == m_waterCollisionPrefab)
            {
                Debug.LogError("Water collision prefab not set");
                return;
            }

            m_collisionObjects.RemoveDeadObjects();

            var availableObjects = m_collisionObjects.ToQueueWithCapacity(m_collisionObjects.Count);

            m_collisionObjects.Clear();

            int i = 0;
            foreach (var face in faces)
            {
                // create box collider based on vertices

                (Vector3 center, Vector3 size) = this.GetCenterAndSize(face);

                GameObject go = availableObjects.Count > 0
                    ? availableObjects.Dequeue().gameObject
                    : Instantiate(m_waterCollisionPrefab, this.transform);

                go.name = $"Water collision {i}";

                go.transform.localPosition = center.WithY(center.y - size.y * 0.5f);
                go.transform.localRotation = Quaternion.identity;

                var boxCollider = go.GetComponentOrThrow<BoxCollider>();
                boxCollider.size = size;

                /*var meshCollider = go.GetComponentOrThrow<MeshCollider>();
                if (meshCollider.sharedMesh != null && !EditorUtilityEx.IsAsset(meshCollider.sharedMesh))
                    F.DestroyEvenInEditMode(meshCollider.sharedMesh);
                meshCollider.sharedMesh = mesh;*/

                if (m_createVisualsForCollisionObjects)
                {
                    GameObject visualGo = go.transform.childCount > 0
                        ? go.transform.GetChild(0).gameObject
                        : GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visualGo.name = "visualization cube";
                    Destroy(visualGo.GetComponent<BoxCollider>());
                    visualGo.transform.SetParent(go.transform, false);
                    visualGo.transform.localPosition = Vector3.zero;
                    visualGo.transform.localRotation = Quaternion.identity;
                    visualGo.transform.localScale = size;
                }

                if (Application.isEditor) // only do it in Editor, no need to do it in a build
                {
                    var waterFaceInfo = go.GetOrAddComponent<WaterFaceInfo>();
                    waterFaceInfo.WaterFace = face;
                }

                m_collisionObjects.Add(go.transform);

                i++;
            }

            foreach (var availableObject in availableObjects)
                F.DestroyEvenInEditMode(availableObject.gameObject);

        }

        public bool IsInterior(float y)
        {
            return y > 500f;
        }

        public bool IsInterior(WaterFace face)
        {
            return this.IsInterior(this.GetCenterAndSize(face).center.y);
        }

        void CreateQuad(Vector2 min, Vector2 max, List<Vector3> vertexes, List<Vector3> normals, List<int> indexes)
        {
            int vertexIndex = vertexes.Count;
            int indexesIndex = indexes.Count;

            vertexes.AddMultiple(4);
            
            vertexes[vertexIndex++] = new Vector3(min.x, 0f, min.y); // low left
            vertexes[vertexIndex++] = new Vector3(max.x, 0f, min.y); // low right
            vertexes[vertexIndex++] = new Vector3(min.x, 0f, max.y); // up left
            vertexes[vertexIndex++] = new Vector3(max.x, 0f, max.y); // up right

            normals.AddMultiple(Vector3.up, 4);

            int lowLeft = vertexIndex - 4;
            int lowRight = vertexIndex - 3;
            int upLeft = vertexIndex - 2;
            int upRight = vertexIndex - 1;

            indexes.AddMultiple(6);

            indexes[indexesIndex++] = upRight;
            indexes[indexesIndex++] = lowRight;
            indexes[indexesIndex++] = lowLeft;
            indexes[indexesIndex++] = upLeft;
            indexes[indexesIndex++] = upRight;
            indexes[indexesIndex++] = lowLeft;
        }
    }
}