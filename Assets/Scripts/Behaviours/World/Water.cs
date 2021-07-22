using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Placements;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class Water : MonoBehaviour
    {
        public GameObject WaterPrefab;


        public void Initialize(WaterFile file)
        {
            if (this.WaterPrefab == null)
            {
                Debug.LogError("No water prefab set, skipping load!");
                return;
            }

            // TODO: what to do with faces that don't have WaterFlags.Visible flag ?

            var faces = file.Faces.Where(f => (f.Flags & WaterFlags.Visible) == WaterFlags.Visible);

            int totalNumVertexes = faces.Sum(f => f.Vertices.Length);
            int totalNumIndexes = faces.Sum(f => (f.Vertices.Length - 2) * 3);

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

            var obj = Instantiate(this.WaterPrefab);
            obj.transform.SetParent(this.transform);
            var mid = obj.transform.position = Vector3.zero;

            obj.name = $"WaterFace ({mid})";

            var mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }
}