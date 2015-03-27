using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Sections;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    public class Geometry
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
                norms[i] = UnityEngine.Vector3.up; // norms[i].normalized;
            }

            return norms;
        }

        private static readonly Dictionary<ObjectFlag, Shader> _sShaders
            = new Dictionary<ObjectFlag,Shader>();

        private static Shader GetShaderNoCache(ObjectFlag flags)
        {
            var noBackCull = (flags & ObjectFlag.NoBackCull) == ObjectFlag.NoBackCull;
            var alpha1 = false; // (flags & ObjectFlag.Alpha1) == ObjectFlag.Alpha1;

            if (noBackCull && alpha1) {
                return Shader.Find("SanAndreasUnity/TransparentNoBackCull");
            }

            if (noBackCull) {
                return Shader.Find("SanAndreasUnity/NoBackCull");
            }

            if (alpha1) {
                return Shader.Find("SanAndreasUnity/Transparent");
            }

            return Shader.Find("SanAndreasUnity/Default");
        }

        private static Shader GetShader(ObjectFlag flags)
        {
            if (_sShaders.ContainsKey(flags)) return _sShaders[flags];

            var shader = GetShaderNoCache(flags);
            _sShaders.Add(flags, shader);
            return shader;
        }

        private static UnityEngine.Material Convert(Sections.Material src, TextureDictionary txd, ObjectFlag flags)
        {
            var shader = GetShader(flags);

            var mat = new UnityEngine.Material(shader);

            mat.color = Convert(src.Colour);

            if (src.TextureCount > 0) {
                var tex = src.Textures[0];
                var diffuse = mat.mainTexture = txd.GetDiffuse(tex.TextureName);

                if (!string.IsNullOrEmpty(tex.MaskName)) {
                    mat.SetTexture("_MaskTex", txd.GetAlpha(tex.MaskName) ?? diffuse);
                }
            }

            return mat;
        }

        private static Mesh Convert(Sections.Geometry src)
        {
            var mesh = new Mesh();

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

        private static readonly Dictionary<string, Geometry> _sLoaded
            = new Dictionary<string, Geometry>();

        public static void Load(string modelName, string texDictName, ObjectFlag flags, out Mesh mesh, out UnityEngine.Material[] materials)
        {
            modelName = modelName.ToLower();

            Geometry loaded;

            if (_sLoaded.ContainsKey(modelName)) {
                loaded = _sLoaded[modelName];
                mesh = loaded.Mesh;
                materials = loaded.GetMaterials(flags);
                return;
            }

            var clump = ResourceManager.ReadFile<Clump>(modelName + ".dff");

            if (clump.GeometryList == null) {
                throw new Exception("Invalid mesh");
            }

            var txd = TextureDictionary.Load(texDictName);
            var geom = clump.GeometryList.Geometry[0];
            var mats = geom.Materials;

            mesh = Convert(geom);

            loaded = new Geometry(mesh, mats, txd);

            materials = loaded.GetMaterials(flags);

            _sLoaded.Add(modelName, loaded);
        }

        public readonly Mesh Mesh;

        private readonly Sections.Material[] _materialSources;
        public readonly TextureDictionary _textureDictionary;
        private readonly Dictionary<ObjectFlag, UnityEngine.Material[]> _materials;

        private Geometry(Mesh mesh, Sections.Material[] materialSources, TextureDictionary textureDictionary)
        {
            Mesh = mesh;

            _materialSources = materialSources;
            _textureDictionary = textureDictionary;
            _materials = new Dictionary<ObjectFlag,UnityEngine.Material[]>();
        }

        private UnityEngine.Material[] GetMaterials(ObjectFlag flags)
        {
            var distinguishing = flags & (ObjectFlag.Alpha1 | ObjectFlag.NoBackCull);

            if (_materials.ContainsKey(distinguishing)) {
                return _materials[distinguishing];
            }

            var mats = _materialSources.Select(x => Convert(x, _textureDictionary, distinguishing)).ToArray();
            _materials.Add(distinguishing, mats);

            return mats;
        }
    }
}
