using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.RenderWareStream;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    public enum MaterialFlags
    {
        Default = 0,
        NoBackCull = 1,
        Alpha = 2,
        Vehicle = 4
    }

    public class Geometry
    {
        private static Texture2D _sBlackTex;

        protected static Texture2D BlackTex
        {
            get
            {
                if (_sBlackTex != null) return _sBlackTex;

                _sBlackTex = new Texture2D(1, 1);
                _sBlackTex.SetPixel(0, 0, Color.black);
                _sBlackTex.Apply();

                return _sBlackTex;
            }
        }

        private static int _sMainTexId = -1;
        protected static int MainTexId
        {
            get { return _sMainTexId == -1 ? _sMainTexId = Shader.PropertyToID("_MainTex") : _sMainTexId; }
        }

        private static int _sMaskTexId = -1;
        protected static int MaskTexId
        {
            get { return _sMaskTexId == -1 ? _sMaskTexId = Shader.PropertyToID("_MaskTex") : _sMaskTexId; }
        }

        private static int _sSpecularId = -1;
        protected static int SpecularId
        {
            get { return _sSpecularId == -1 ? _sSpecularId = Shader.PropertyToID("_Specular") : _sSpecularId; }
        }

        private static int _sSmoothnessId = -1;
        protected static int SmoothnessId
        {
            get { return _sSmoothnessId == -1 ? _sSmoothnessId = Shader.PropertyToID("_Smoothness") : _sSmoothnessId; }
        }

        private static int[] FromTriangleStrip(IList<int> indices)
        {
            var dst = new List<int>((indices.Count - 2) * 3);

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

        private static int[] ReverseFaces(int[] indices)
        {
            for (var i = 0; i < indices.Length - 2; i += 3) {
                var temp = indices[i];
                indices[i] = indices[i + 1];
                indices[i + 1] = temp;
            }
            return indices;
        }

        private static UnityEngine.Vector3[] CalculateNormals(RenderWareStream.Geometry src, UnityEngine.Vector3[] verts)
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

        private static readonly Dictionary<MaterialFlags, Shader> _sShaders
            = new Dictionary<MaterialFlags, Shader>();

        private static Shader GetShaderNoCache(MaterialFlags flags)
        {
            var noBackCull = (flags & MaterialFlags.NoBackCull) == MaterialFlags.NoBackCull;
            var alpha = (flags & MaterialFlags.Alpha) == MaterialFlags.Alpha;
            var vehicle = (flags & MaterialFlags.Vehicle) == MaterialFlags.Vehicle;

            if (vehicle) {
                return Shader.Find("SanAndreasUnity/Vehicle");
            }

            if (noBackCull && alpha) {
                return Shader.Find("SanAndreasUnity/TransparentNoBackCull");
            }

            if (noBackCull) {
                return Shader.Find("SanAndreasUnity/NoBackCull");
            }

            if (alpha) {
                return Shader.Find("SanAndreasUnity/Transparent");
            }

            return Shader.Find("SanAndreasUnity/Default");
        }

        private static Shader GetShader(MaterialFlags flags)
        {
            if (_sShaders.ContainsKey(flags)) return _sShaders[flags];

            var shader = GetShaderNoCache(flags);
            _sShaders.Add(flags, shader);
            return shader;
        }

        private static UnityEngine.Material Convert(RenderWareStream.Material src, TextureDictionary[] txds, MaterialFlags flags)
        {
            var shader = GetShader(flags);
            var mat = new UnityEngine.Material(shader);

            mat.color = Types.Convert(src.Colour);

            if (src.TextureCount > 0) {
                var tex = src.Textures[0];
                var diffuse = txds.GetDiffuse(tex.TextureName);

                if (src.TextureCount > 1) {
                    Debug.LogFormat("Something has {0} textures!", src.TextureCount);
                }

                if (diffuse != null) {
                    mat.SetTexture(MainTexId, diffuse);
                } else {
                    Debug.LogWarningFormat("Unable to find texture {0}", tex.TextureName);
                }

                if (!string.IsNullOrEmpty(tex.MaskName)) {
                    mat.SetTexture(MaskTexId, txds.GetAlpha(tex.MaskName) ?? diffuse);
                }
            } else {
                mat.SetTexture(MainTexId, BlackTex);
            }


            return mat;
        }

        private static Mesh Convert(RenderWareStream.Geometry src)
        {
            var mesh = new Mesh();

            mesh.vertices = src.Vertices.Select(x => Types.Convert(x)).ToArray();

            if (src.Normals != null) {
                mesh.normals = src.Normals.Select(x => Types.Convert(x)).ToArray();
            }

            if (src.Colours != null) {
                mesh.colors32 = src.Colours.Select(x => Types.Convert(x)).ToArray();
            }

            if (src.TexCoords != null && src.TexCoords.Length > 0) {
                mesh.uv = src.TexCoords[0].Select(x => Types.Convert(x)).ToArray();
            }

            if (src.Normals == null) {
                mesh.normals = CalculateNormals(src, mesh.vertices);
            }
            
            mesh.subMeshCount = src.MaterialSplits.Length;

            var isTriangleStrip = (src.Flags & GeometryFlag.TriangleStrips) == GeometryFlag.TriangleStrips;

            var subMesh = 0;
            foreach (var split in src.MaterialSplits) {
                var indices = isTriangleStrip
                    ? FromTriangleStrip(split.FaceIndices)
                    : ReverseFaces(split.FaceIndices);
                mesh.SetIndices(indices, MeshTopology.Triangles, subMesh++);
            }

            mesh.RecalculateBounds();

            return mesh;
        }

        private static GeometryFrame Convert(RenderWareStream.Frame src, RenderWareStream.Atomic[] atomics)
        {
            var atomic = atomics.FirstOrDefault(x => x.FrameIndex == src.Index);

            return new GeometryFrame(src, atomic);
        }

        private static Transform[] Convert(HierarchyAnimation hAnim, Dictionary<GeometryFrame, Transform> transforms, GeometryFrame[] frames)
        {
            var dict = frames.Where(x => x.Source.HAnim != null)
                .ToDictionary(x => x.Source.HAnim.NodeId, x => transforms[x]);

            return hAnim.Nodes.Select(x => dict[x.NodeId]).ToArray();
        }

        public class GeometryFrame
        {
            public const string DefaultName = "unnamed";

            public readonly RenderWareStream.Frame Source;

            public readonly string Name;
            public readonly UnityEngine.Vector3 Position;
            public readonly UnityEngine.Quaternion Rotation;

            public readonly int ParentIndex;
            public readonly int GeometryIndex;

            public GeometryFrame(RenderWareStream.Frame src, RenderWareStream.Atomic atomic)
            {
                Source = src;

                Name = src.Name != null ? src.Name.Value : DefaultName;
                ParentIndex = src.ParentIndex;
                GeometryIndex = atomic == null ? -1 : (int)atomic.GeometryIndex;

                Position = Types.Convert(src.Position);
                Rotation = UnityEngine.Quaternion.LookRotation(Types.Convert(src.MatrixForward), Types.Convert(src.MatrixUp));
            }

            public override int GetHashCode()
            {
                return Source.Index;
            }
        }

        public class GeometryParts
        {
            private readonly CollisionFile _collisions;

            public readonly string Name;
            public readonly Geometry[] Geometry;
            public readonly GeometryFrame[] Frames;

            public GeometryParts(string name, Clump clump, TextureDictionary[] txds)
            {
                Name = name;

                Geometry = clump.GeometryList.Geometry
                    .Select(x => new Geometry(x, Convert(x), txds))
                    .ToArray();
                
                Frames = clump.FrameList.Frames
                    .Select(x => Convert(x, clump.Atomics))
                    .ToArray();

                _collisions = clump.Collision;
            }

            public void AttachCollisionModel(Transform destParent, bool forceConvex = false)
            {
                if (_collisions != null) {
                    CollisionModel.Load(_collisions, destParent, forceConvex);
                } else {
                    CollisionModel.Load(Name, destParent, forceConvex);
                }
            }

            public Dictionary<GeometryFrame, Transform> AttachFrames(Transform destParent, MaterialFlags flags)
            {
                var transforms = Frames.ToDictionary(x => x, x => {
                    var trans = new GameObject(x.Name).transform;
                    trans.localPosition = x.Position;
                    trans.localRotation = x.Rotation;
                    return trans;
                });

                foreach (var frame in Frames) {
                    if (frame.ParentIndex != -1) {
                        transforms[frame].SetParent(transforms[Frames[frame.ParentIndex]], false);
                    } else {
                        transforms[frame].SetParent(destParent, false);
                    }

                    if (frame.GeometryIndex != -1) {
                        var gobj = transforms[frame].gameObject;
                        var geometry = Geometry[frame.GeometryIndex];

                        HierarchyAnimation hAnim = null;
                        var parent = frame;
                        while ((hAnim = parent.Source.HAnim) == null || hAnim.NodeCount == 0) {
                            if (parent.ParentIndex == -1) {
                                hAnim = null;
                                break;
                            }
                            parent = Frames[parent.ParentIndex];
                        }

                        Renderer renderer;
                        if (hAnim != null) {
                            var smr = gobj.AddComponent<SkinnedMeshRenderer>();

                            var bones = Convert(hAnim, transforms, Frames);

                            smr.rootBone = bones[0];
                            smr.bones = bones;

                            smr.sharedMesh = geometry.Mesh;
                            smr.sharedMesh.bindposes = geometry.SkinToBoneMatrices
                                .Select((x, i) => UnityEngine.Matrix4x4.Transpose(x))
                                .ToArray();

                            renderer = smr;
                        } else {
                            var mf = gobj.AddComponent<MeshFilter>();
                            mf.sharedMesh = geometry.Mesh;

                            renderer = gobj.AddComponent<MeshRenderer>();
                        }

                        renderer.sharedMaterials = geometry.GetMaterials(flags);

                        // filter these out for now
                        if (frame.Name.EndsWith("_vlo") ||
                            frame.Name.EndsWith("_dam")) {
                            gobj.SetActive(false);
                        }
                    }
                }

                return transforms;
            }
        }

        private static readonly Dictionary<string, GeometryParts> _sLoaded
            = new Dictionary<string, GeometryParts>();

        public static GeometryParts Load(string modelName, params string[] texDictNames)
        {
            return Load(modelName, texDictNames.Select(x => TextureDictionary.Load(x)).ToArray());
        }

        public static GeometryParts Load(string modelName, params TextureDictionary[] txds)
        {
            modelName = modelName.ToLower();

            if (_sLoaded.ContainsKey(modelName)) {
                return _sLoaded[modelName];
            }

            var clump = ArchiveManager.ReadFile<Clump>(modelName + ".dff");

            if (clump.GeometryList == null) {
                throw new Exception("Invalid mesh");
            }

            var loaded = new GeometryParts(modelName, clump, txds);

            _sLoaded.Add(modelName, loaded);

            return loaded;
        }

        public readonly Mesh Mesh;

        private readonly RenderWareStream.Geometry _geom;
        public readonly TextureDictionary[] _textureDictionaries;
        private readonly Dictionary<MaterialFlags, UnityEngine.Material[]> _materials;

        public readonly UnityEngine.Matrix4x4[] SkinToBoneMatrices;

        private Geometry(RenderWareStream.Geometry geom, Mesh mesh, TextureDictionary[] textureDictionaries)
        {
            Mesh = mesh;

            if (geom.Skinning != null) {
                Mesh.boneWeights = Types.Convert(geom.Skinning.VertexBoneIndices, geom.Skinning.VertexBoneWeights);
                SkinToBoneMatrices = Types.Convert(geom.Skinning.SkinToBoneMatrices);
            }

            _geom = geom;
            _textureDictionaries = textureDictionaries;
            _materials = new Dictionary<MaterialFlags, UnityEngine.Material[]>();
        }

        public UnityEngine.Material[] GetMaterials(ObjectFlag flags)
        {
            return GetMaterials(flags, x => {});
        }

        public UnityEngine.Material[] GetMaterials(ObjectFlag flags,
            Action<UnityEngine.Material> setupMaterial)
        {
            var matFlags = MaterialFlags.Default;

            if ((flags & ObjectFlag.NoBackCull) == ObjectFlag.NoBackCull) {
                matFlags |= MaterialFlags.NoBackCull;
            }

            if ((flags & (ObjectFlag.Alpha1 | ObjectFlag.Alpha2)) != 0
                && (flags & ObjectFlag.DisableShadowMesh) == ObjectFlag.DisableShadowMesh) {
                matFlags |= MaterialFlags.Alpha;
            }

            return GetMaterials(matFlags, setupMaterial);
        }

        public UnityEngine.Material[] GetMaterials(MaterialFlags flags)
        {
            return GetMaterials(flags, x => {});
        }

        public UnityEngine.Material[] GetMaterials(MaterialFlags flags,
            Action<UnityEngine.Material> setupMaterial)
        {
            if (_materials.ContainsKey(flags)) {
                return _materials[flags];
            }

            var mats = _geom.Materials.Select(x => {
                var mat = Convert(x, _textureDictionaries, flags);
                setupMaterial(mat);
                return mat;
            }).ToArray();

            mats = _geom.MaterialSplits.Select(x => mats[x.MaterialIndex]).ToArray();

            _materials.Add(flags, mats);

            return mats;
        }
    }
}
