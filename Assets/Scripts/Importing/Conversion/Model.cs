using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Sections;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    internal static class Model
    {
        public static UnityEngine.Vector2 Convert(this Sections.Vector2 vec)
        {
            return new UnityEngine.Vector2(vec.X, vec.Y);
        }

        public static UnityEngine.Vector3 Convert(this Sections.Vector3 vec)
        {
            return new UnityEngine.Vector3(-vec.X, vec.Z, vec.Y);
        }

        public static UnityEngine.Color32 Convert(this Sections.Color4 clr)
        {
            return new UnityEngine.Color32(clr.R, clr.G, clr.B, clr.A);
        }

        public static int[] FromTriangleStrip(this int[] indices)
        {
            var arr = new int[(indices.Length - 2) * 3];

            var j = 0;
            for (var i = 0; i < indices.Length - 2; ++i) {
                arr[j++] = indices[i];
                arr[j++] = indices[i + 1 + (i & 1)];
                arr[j++] = indices[i + 2 - (i & 1)];
            }

            return arr;
        }

        public static Mesh CreateMesh(this Geometry geom)
        {
            var mesh = new Mesh();

            mesh.vertices = geom.Vertices.Select(x => x.Convert()).ToArray();

            if (geom.Normals != null) {
                mesh.normals = geom.Normals.Select(x => x.Convert()).ToArray();
            }

            if (geom.Colours != null) {
                mesh.colors32 = geom.Colours.Select(x => x.Convert()).ToArray();
            }

            if (geom.TexCoords != null) {
                mesh.uv = geom.TexCoords.Select(x => x.Convert()).ToArray();
            }

            mesh.subMeshCount = geom.MaterialSplits.Length;

            var subMesh = 0;
            foreach (var split in geom.MaterialSplits) {
                mesh.SetIndices(split.FaceIndices.FromTriangleStrip(), MeshTopology.Triangles, subMesh++);
            }

            if (geom.Normals == null) {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();

            return mesh;
        }

        public static void LoadMesh(this MonoBehaviour behaviour, string name)
        {
            Clump clump;

            using (var stream = ResourceManager.ReadFile(name)) {
                var section = Section<SectionData>.Read(stream);
                if (section.Type != Clump.TypeId) return;

                clump = (Clump) section.Data;
            }

            if (clump.GeometryList == null) return;

            var mesh = CreateMesh(clump.GeometryList.Geometry[0]);

            var meshFilter = behaviour.GetComponent<MeshFilter>();
            var meshRenderer = behaviour.GetComponent<MeshRenderer>();

            meshFilter.mesh = mesh;
            meshRenderer.materials = Enumerable.Repeat(meshRenderer.material, mesh.subMeshCount).ToArray();
        }
    }
}
