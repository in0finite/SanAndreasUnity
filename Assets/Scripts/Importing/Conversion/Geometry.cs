using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Sections;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    internal class Geometry
    {
        private static UnityEngine.Vector2 Convert(Sections.Vector2 vec)
        {
            return new UnityEngine.Vector2(vec.X, vec.Y);
        }

        private static UnityEngine.Vector3 Convert(Sections.Vector3 vec)
        {
            return new UnityEngine.Vector3(vec.X, vec.Z, vec.Y);
        }

        private static Color32 Convert(Color4 clr)
        {
            return new Color32(clr.R, clr.G, clr.B, clr.A);
        }

        private static int[] FromTriangleStrip(IList<int> indices)
        {
            var dst = new List<int>();

            for (var i = 0; i < indices.Count - 2; ++i) {
                var a = indices[i];
                var b = indices[i + 2 - (i & 1)];
                var c = indices[i + 1 + (i & 1)];

                if (a == b || b == c || a == c) continue;

                dst.Add(a);
                dst.Add(b);
                dst.Add(c);
            }

            return dst.ToArray();
        }

        private static UnityEngine.Vector3[] CalculateNormals(Sections.Geometry src, UnityEngine.Vector3[] verts)
        {
            var norms = new UnityEngine.Vector3[src.VertexCount];

            //for (var i = 0; i < src.FaceCount; ++i) {
            //    var face = src.Faces[i];

            //    var a = verts[face.Vertex0];
            //    var b = verts[face.Vertex1];
            //    var c = verts[face.Vertex2];

            //    var v = b - a;
            //    var w = c - b;

            //    var norm = new UnityEngine.Vector3(
            //        v.y * w.z - v.z * w.y,
            //        v.z * w.x - v.x * w.z,
            //        v.x * w.y - v.y * w.x).normalized;

            //    norms[face.Vertex0] -= norm;
            //    norms[face.Vertex1] -= norm;
            //    norms[face.Vertex2] -= norm;
            //}

            for (var i = 0; i < src.VertexCount; ++i) {
                norms[i] = new UnityEngine.Vector3(0f, 1f, 0f); // norms[i].normalized;
            }

            return norms;
        }

        private static Shader _sStandardShader;

        private static UnityEngine.Material Convert(Sections.Material src, TextureDictionary txd)
        {
            var shader = _sStandardShader ?? (_sStandardShader = Shader.Find("SanAndreasUnity/Default"));

            var mat = new UnityEngine.Material(shader);

            mat.color = Convert(src.Colour);

            if (src.TextureCount > 0) {
                var tex = src.Textures[0];

                mat.mainTexture = txd.GetDiffuse(tex.TextureName);

                if (!string.IsNullOrEmpty(tex.MaskName)) {
                    mat.SetTexture("_MaskTex", txd.GetAlpha(tex.MaskName));
                }
            }

            return mat;
        }

        private static Mesh Convert(Sections.Geometry src, TextureDictionary txd, out UnityEngine.Material[] materials)
        {
            var mesh = new Mesh();

            materials = src.Materials.Select(x => Convert(x, txd)).ToArray();

            mesh.vertices = src.Vertices.Select(x => Convert(x)).ToArray();

            if (src.Normals != null) {
                mesh.normals = src.Normals.Select(x => Convert(x)).ToArray();
            }

            if (src.Colours != null) {
                mesh.colors32 = src.Colours.Select(x => Convert(x)).ToArray();
            }

            if (src.TexCoords != null) {
                mesh.uv = src.TexCoords.Select(x => Convert(x)).ToArray();
            }

            if (src.Normals == null) {
                mesh.normals = CalculateNormals(src, mesh.vertices);
            }
            
            mesh.subMeshCount = src.MaterialSplits.Length;

            var subMesh = 0;
            foreach (var split in src.MaterialSplits) {
                mesh.SetIndices(FromTriangleStrip(split.FaceIndices), MeshTopology.Triangles, subMesh++);
            }

            mesh.RecalculateBounds();

            return mesh;
        }

        private static readonly Dictionary<string, Geometry> _sLoaded = new Dictionary<string, Geometry>();

        public static void Load(string modelName, string texDictName, out Mesh mesh, out UnityEngine.Material[] materials)
        {
            modelName = modelName.ToLower();

            if (_sLoaded.ContainsKey(modelName)) {
                var loaded = _sLoaded[modelName];
                mesh = loaded.Mesh;
                materials = loaded.Materials;
                return;
            }

            var clump = ResourceManager.ReadFile<Clump>(modelName + ".dff");

            if (clump.GeometryList == null) {
                throw new Exception("Invalid mesh");
            }

            var txd = TextureDictionary.Load(texDictName);

            mesh = Convert(clump.GeometryList.Geometry[0], txd, out materials);

            _sLoaded.Add(modelName, new Geometry(mesh, materials));
        }

        public readonly Mesh Mesh;
        public readonly UnityEngine.Material[] Materials;

        private Geometry(Mesh mesh, UnityEngine.Material[] materials)
        {
            Mesh = mesh;
            Materials = materials;
        }
    }
}
