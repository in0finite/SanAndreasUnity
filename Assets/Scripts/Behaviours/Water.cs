using SanAndreasUnity.Importing.Items;
using UnityEngine;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
    public class Water : MonoBehaviour
    {
        public GameObject WaterPrefab;

        private void AddFace(WaterFace face)
        {
            // TODO
            if ((face.Flags & WaterFlags.Visible) != WaterFlags.Visible) return;

            var obj = Instantiate(WaterPrefab);
            obj.transform.SetParent(transform);
            var mid = obj.transform.position = face.Vertices.Aggregate(Vector3.zero,
                (s, x) => s + x.Position) / face.Vertices.Length;

            obj.name = string.Format("WaterFace ({0})", mid);

            var mesh = new Mesh();

            mesh.vertices = face.Vertices.Select(x => x.Position - mid).ToArray();
            mesh.normals = face.Vertices.Select(x => Vector3.up).ToArray();

            var indices = new int[(face.Vertices.Length - 2) * 3];
            for (var i = 0; i < face.Vertices.Length - 2; ++i) {
                var flip = i & 1;
                indices[i * 3 + 0] = i + 1 - flip;
                indices[i * 3 + 1] = i + 0 + flip;
                indices[i * 3 + 2] = i + 2;
            }

            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        public void Initialize(WaterFile file)
        {
            if (WaterPrefab == null) return;

            foreach (var face in file.Faces) {
                AddFace(face);
            }
        }
    }
}
