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
            var arr = new int[(indices.Count - 2) * 3];

            var j = 0;
            for (var i = 0; i < indices.Count - 2; ++i) {
                arr[j++] = indices[i];
                arr[j++] = indices[i + 2 - (i & 1)];
                arr[j++] = indices[i + 1 + (i & 1)];
            }

            return arr;
        }

        private static Shader _sStandardShader;

        private static UnityEngine.Material Convert(Sections.Material src, TextureDictionary txd)
        {
            var shader = _sStandardShader ?? (_sStandardShader = Shader.Find("Standard"));

            var mat = new UnityEngine.Material(shader);

            mat.color = Convert(src.Colour);

            if (src.TextureCount > 0) {
                var texName = src.Textures[0].TextureName;

                mat.mainTexture = txd.GetDiffuse(texName);
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

            mesh.subMeshCount = src.MaterialSplits.Length;

            var subMesh = 0;
            foreach (var split in src.MaterialSplits) {
                mesh.SetIndices(FromTriangleStrip(split.FaceIndices), MeshTopology.Triangles, subMesh++);
            }

            if (src.Normals == null) {
                mesh.RecalculateNormals();
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
