using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Placements;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class Water : MonoBehaviour
    {
        public GameObject WaterPrefab;


        public void Initialize(WaterFile file, Vector2 worldSize)
        {
            if (this.WaterPrefab == null)
            {
                Debug.LogError("No water prefab set, skipping load!");
                return;
            }

            // TODO: what to do with faces that don't have WaterFlags.Visible flag ?

            var faces = file.Faces.Where(f => (f.Flags & WaterFlags.Visible) == WaterFlags.Visible);

            // we need total of 4 quads for "infinite" water:
            // - upper side (from upper world boundary to positive infinity)
            // - lower side (from lower world boundary to negative infinity)
            // - left side (between first 2 quads)
            // - right side (between first 2 quads)

            const int numQuadsForInfiniteWater = 4;

            int totalNumVertexes = faces.Sum(f => f.Vertices.Length) + numQuadsForInfiniteWater * GetNumVertexesForQuad();
            int totalNumIndexes = faces.Sum(f => (f.Vertices.Length - 2) * 3) + numQuadsForInfiniteWater * GetNumIndexesForQuad();

            Vector3[] vertices = new Vector3[totalNumVertexes];
            Vector3[] normals = new Vector3[totalNumVertexes];
            int[] indices = new int[totalNumIndexes];

            int verticesIndex = 0;
            int indicesIndex = 0;

            foreach (var face in faces)
            {
                for (int j = 0; j < face.Vertices.Length; j++)
                {
                    vertices[verticesIndex + j] = face.Vertices[j].Position;
                    normals[verticesIndex + j] = Vector3.up;
                }

                for (var j = 0; j < face.Vertices.Length - 2; ++j)
                {
                    var flip = j & 1;
                    indices[indicesIndex + j * 3 + 0] = verticesIndex + j + 1 - flip;
                    indices[indicesIndex + j * 3 + 1] = verticesIndex + j + 0 + flip;
                    indices[indicesIndex + j * 3 + 2] = verticesIndex + j + 2;
                }

                verticesIndex += face.Vertices.Length;
                indicesIndex += (face.Vertices.Length - 2) * 3;
            }

            // add "infinite" water

            const float infiniteWaterOffset = 20000f;
            // upper quad
            CreateQuad(
                new Vector2( -worldSize.x / 2f - infiniteWaterOffset, worldSize.y / 2f),
                new Vector2(worldSize.x / 2f + infiniteWaterOffset, worldSize.y / 2f + infiniteWaterOffset),
                vertices,
                normals,
                ref verticesIndex,
                indices,
                ref indicesIndex);
            // lower quad
            CreateQuad(
                new Vector2( -worldSize.x / 2f - infiniteWaterOffset, - worldSize.y / 2f - infiniteWaterOffset),
                new Vector2(worldSize.x / 2f + infiniteWaterOffset, - worldSize.y / 2f),
                vertices,
                normals,
                ref verticesIndex,
                indices,
                ref indicesIndex);
            // left quad
            CreateQuad(
                new Vector2( -worldSize.x / 2f - infiniteWaterOffset, - worldSize.y / 2f),
                new Vector2(- worldSize.x / 2f, worldSize.y / 2f),
                vertices,
                normals,
                ref verticesIndex,
                indices,
                ref indicesIndex);
            // right quad
            CreateQuad(
                new Vector2( worldSize.x / 2f, - worldSize.y / 2f),
                new Vector2(worldSize.x / 2f + infiniteWaterOffset, worldSize.y / 2f),
                vertices,
                normals,
                ref verticesIndex,
                indices,
                ref indicesIndex);

            var mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            var go = Instantiate(this.WaterPrefab, this.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            go.name = "Water mesh";

            go.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        void CreateQuad(Vector2 min, Vector2 max, Vector3[] vertexes, Vector3[] normals, ref int vertexIndex, int[] indexes, ref int indexesIndex)
        {
            vertexes[vertexIndex++] = new Vector3(min.x, 0f, min.y); // low left
            vertexes[vertexIndex++] = new Vector3(max.x, 0f, min.y); // low right
            vertexes[vertexIndex++] = new Vector3(min.x, 0f, max.y); // up left
            vertexes[vertexIndex++] = new Vector3(max.x, 0f, max.y); // up right

            normals[vertexIndex - 4] = Vector3.up;
            normals[vertexIndex - 3] = Vector3.up;
            normals[vertexIndex - 2] = Vector3.up;
            normals[vertexIndex - 1] = Vector3.up;

            // triangle 1: low left, low right, up right
            // triangle 2: low left, up right, up left

            int lowLeft = vertexIndex - 4;
            int lowRight = vertexIndex - 3;
            int upLeft = vertexIndex - 2;
            int upRight = vertexIndex - 1;

            indexes[indexesIndex++] = lowLeft;
            indexes[indexesIndex++] = lowRight;
            indexes[indexesIndex++] = upRight;
            indexes[indexesIndex++] = lowLeft;
            indexes[indexesIndex++] = upRight;
            indexes[indexesIndex++] = upLeft;
        }

        int GetNumVertexesForQuad() => 4;

        int GetNumIndexesForQuad() => 6;
    }
}