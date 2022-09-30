using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Placements;
using UGameCore.Utilities;
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
        public bool CreateCollisionObjects { get => m_createCollisionObjects; set => m_createCollisionObjects = value; }
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
            // - it seems that those faces are transparent, usually used for very shallow waters

            var faces = file.Faces.Where(f => (f.Flags & WaterFlags.Visible) == WaterFlags.Visible);

            // we need total of 4 quads for "infinite" water:
            // - upper side (from upper world boundary to positive infinity)
            // - lower side (from lower world boundary to negative infinity)
            // - left side (between first 2 quads)
            // - right side (between first 2 quads)

            var vertices = new List<Vector3>(1536);
            var normals = new List<Vector3>(1536);
            var indices = new List<int>(2048);

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
                CreateCollision(faces);

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

        private static Vector3 GetIncenterOfLastTriangle(
            List<Vector3> vertexes,
            List<int> indexes)
        {
            Vector3 a = vertexes[indexes[indexes.Count - 3]];
            Vector3 b = vertexes[indexes[indexes.Count - 2]];
            Vector3 c = vertexes[indexes[indexes.Count - 1]];

            Vector3 sideC = b - a;
            Vector3 sideB = c - a;
            Vector3 sideA = c - b;

            float lengthA = sideA.magnitude;
            float lengthB = sideB.magnitude;
            float lengthC = sideC.magnitude;

            float perimeter = lengthA + lengthB + lengthC;

            Vector3 incenter;
            incenter.x = lengthA * a.x + lengthB * b.x + lengthC * c.x;
            incenter.y = lengthA * a.y + lengthB * b.y + lengthC * c.y;
            incenter.z = lengthA * a.z + lengthB * b.z + lengthC * c.z;
            incenter /= perimeter;

            return incenter;
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

            for (int i = 0; i < face.Vertices.Length - 2; ++i)
            {
                indices.AddMultiple(3);
                int flip = i & 1;
                indices[indicesIndex++] = verticesIndex + i + 1 - flip;
                indices[indicesIndex++] = verticesIndex + i + 0 + flip;
                indices[indicesIndex++] = verticesIndex + i + 2;
                ReverseTriangleIfNeeded(vertices, indices, Vector3.up);
            }
        }

        void CreateCollision(IEnumerable<WaterFace> faces)
        {
            if (null == m_waterCollisionPrefab)
            {
                Debug.LogError("Water collision prefab not set");
                return;
            }

            m_collisionObjects.RemoveDeadObjects();

            var availableObjects = m_collisionObjects.ToQueueWithCapacity(m_collisionObjects.Count);

            m_collisionObjects.Clear();

            var vertexes = new List<Vector3>();
            var normals = new List<Vector3>();
            var indexes = new List<int>();

            int i = 0;
            foreach (var face in faces)
            {
                // create box/mesh collider based on vertexes

                (Vector3 center, Vector3 size) = this.GetCenterAndSize(face);

                GameObject go = availableObjects.Count > 0
                    ? availableObjects.Dequeue().gameObject
                    : Instantiate(m_waterCollisionPrefab, this.transform);

                go.name = $"Water collision {i}";

                go.transform.localPosition = center.WithAddedY(- size.y * 0.5f);
                go.transform.localRotation = Quaternion.identity;

                var meshCollider = go.GetComponent<MeshCollider>();
                if (meshCollider != null && meshCollider.sharedMesh != null && !EditorUtilityEx.IsAsset(meshCollider.sharedMesh))
                    F.DestroyEvenInEditMode(meshCollider.sharedMesh);

                if (face.Vertices.Length == 4)
                {
                    if (meshCollider != null)
                        F.DestroyEvenInEditMode(meshCollider);
                    
                    var boxCollider = go.GetOrAddComponent<BoxCollider>();
                    boxCollider.size = size;
                    boxCollider.isTrigger = true;
                }
                else if (face.Vertices.Length == 3)
                {
                    var boxCollider = go.GetComponent<BoxCollider>();
                    if (boxCollider != null)
                        F.DestroyEvenInEditMode(boxCollider);

                    meshCollider = go.GetOrAddComponent<MeshCollider>();
                    meshCollider.cookingOptions = MeshColliderCookingOptions.None;
                    meshCollider.convex = true;
                    meshCollider.isTrigger = true;

                    vertexes.Clear();
                    normals.Clear();
                    indexes.Clear();
                    CreateCollisionMeshFor3VertexFace(face, vertexes, normals, indexes, size.y);

                    // modify vertexes based on center of game object
                    Vector3 centerToSubstract = go.transform.localPosition;
                    for (int v = 0; v < vertexes.Count; v++)
                        vertexes[v] = vertexes[v] - centerToSubstract;
                    
                    var mesh = new Mesh();
                    mesh.name = go.name;
                    mesh.SetVertices(vertexes);
                    //mesh.SetNormals(normals);
                    mesh.SetIndices(indexes, MeshTopology.Triangles, 0, calculateBounds: true);

                    meshCollider.sharedMesh = mesh;
                }
                else
                {
                    Debug.LogError($"Only water faces with 3 or 4 vertices are supported, found {face.Vertices.Length}");
                }

                if (m_createVisualsForCollisionObjects)
                {
                    GameObject visualGo = go.transform.childCount > 0
                        ? go.transform.GetChild(0).gameObject
                        : GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visualGo.name = "visualization cube";
                    F.DestroyEvenInEditMode(visualGo.GetComponent<BoxCollider>());
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

        void CreateCollisionMeshFor3VertexFace(
            WaterFace face, List<Vector3> vertexes, List<Vector3> normals, List<int> indexes, float height)
        {
            if (face.Vertices.Length != 3)
                throw new System.ArgumentException("Face must contain 3 vertices");

            // create top triangle
            int topSideVertexIndex = vertexes.Count;
            CreateTriangle(face, vertexes, normals, indexes, 0f, Vector3.up);

            // create bottom triangle
            int bottomSideVertexIndex = vertexes.Count;
            CreateTriangle(face, vertexes, normals, indexes, -height, - Vector3.up);

            // create side quads
            Vector3 coordinateCenter = GetIncenterOfLastTriangle(vertexes, indexes);
            for (int i = 0; i < 3; i++)
            {
                int otherIndex = (i + 1) % 3;

                // 2 top, 1 bottom
                indexes.Add(topSideVertexIndex + i);
                indexes.Add(topSideVertexIndex + otherIndex);
                indexes.Add(bottomSideVertexIndex + i);

                Vector3 lastTriangleCenter = GetIncenterOfLastTriangle(vertexes, indexes);
                ReverseTriangleIfNeeded(vertexes, indexes, lastTriangleCenter - coordinateCenter);

                // 2 bottom, 1 top
                indexes.Add(topSideVertexIndex + otherIndex);
                indexes.Add(bottomSideVertexIndex + otherIndex);
                indexes.Add(bottomSideVertexIndex + i);

                lastTriangleCenter = GetIncenterOfLastTriangle(vertexes, indexes);
                ReverseTriangleIfNeeded(vertexes, indexes, lastTriangleCenter - coordinateCenter);
            }
        }

        void CreateTriangle(
            WaterFace face,
            List<Vector3> vertexes,
            List<Vector3> normals,
            List<int> indexes,
            float heightOffset,
            Vector3 lookDirection)
        {
            if (face.Vertices.Length != 3)
                throw new System.ArgumentException("Face must contain 3 vertices");

            int vertexIndex = vertexes.Count;

            for (int i = 0; i < 3; i++)
            {
                vertexes.Add(face.Vertices[i].Position.WithAddedY(heightOffset));
                normals.Add(Vector3.up);
            }

            indexes.Add(vertexIndex);
            indexes.Add(vertexIndex + 1);
            indexes.Add(vertexIndex + 2);

            ReverseTriangleIfNeeded(vertexes, indexes, lookDirection);
        }

        void ReverseTriangleIfNeeded(
            List<Vector3> vertexes,
            List<int> indexes,
            Vector3 lookDirection)
        {
            Vector3 a = vertexes[indexes[indexes.Count - 3]];
            Vector3 b = vertexes[indexes[indexes.Count - 2]];
            Vector3 c = vertexes[indexes[indexes.Count - 1]];

            Vector3 side1 = b - a;
            Vector3 side2 = c - a;

            Vector3 normalNonNormalized = Vector3.Cross(side1, side2);

            if (Vector3.Angle(normalNonNormalized, lookDirection) < 90f)
                return;

            // reverse triangle
            int firstIndexValue = indexes[indexes.Count - 3];
            int lastIndexValue = indexes[indexes.Count - 1];
            indexes[indexes.Count - 3] = lastIndexValue;
            indexes[indexes.Count - 1] = firstIndexValue;

        }
    }
}