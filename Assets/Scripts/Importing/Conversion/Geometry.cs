using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.RenderWareStream;
using System;
using System.Collections.Generic;
using System.Linq;
using UGameCore.Utilities;
using UnityEngine;
using Profiler = UnityEngine.Profiling.Profiler;

namespace SanAndreasUnity.Importing.Conversion
{
    [Flags]
    public enum MaterialFlags
    {
        Default = 0,
        NoBackCull = 1,
        Alpha = 2,
        Vehicle = 4,
        OverrideAlpha = 8,
    }

    public class Geometry
    {
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

        private static int _sMetallicId = -1;

        protected static int MetallicId
        {
            get { return _sMetallicId == -1 ? _sMetallicId = Shader.PropertyToID("_Metallic") : _sMetallicId; }
        }

        private static int _sSmoothnessId = -1;

        protected static int SmoothnessId
        {
            get { return _sSmoothnessId == -1 ? _sSmoothnessId = Shader.PropertyToID("_Smoothness") : _sSmoothnessId; }
        }

        private static int _sCarColorIndexId = -1;

        public static int CarColorIndexId
        {
            get { return _sCarColorIndexId == -1 ? _sCarColorIndexId = Shader.PropertyToID("_CarColorIndex") : _sCarColorIndexId; }
        }

        private static int _sHasNightColorsPropertyId = -1;

        public static int HasNightColorsPropertyId => _sHasNightColorsPropertyId == -1 ? _sHasNightColorsPropertyId = Shader.PropertyToID("_HasNightColors") : _sHasNightColorsPropertyId;

        private static int[] FromTriangleStrip(IList<int> indices)
        {
            var dst = new List<int>((indices.Count - 2) * 3);

            for (var i = 0; i < indices.Count - 2; ++i)
            {
                var a = indices[i];
                var b = indices[i + 1 + (i & 1)];
                var c = indices[i + 2 - (i & 1)];

                if (a == b || b == c || a == c) continue;

                dst.Add(a);
                dst.Add(b);
                dst.Add(c);
            }

            return dst.ToArray();
        }

        private static UnityEngine.Vector3[] CalculateNormals(RenderWareStream.Geometry src, UnityEngine.Vector3[] verts)
        {
            var norms = new UnityEngine.Vector3[src.VertexCount];

            for (var i = 0; i < src.FaceCount; ++i)
            {
                var face = src.Faces[i];

                var a = verts[face.Vertex0];
                var b = verts[face.Vertex1];
                var c = verts[face.Vertex2];

                var v = b - a;
                var w = c - b;

                var norm = new UnityEngine.Vector3(
                    v.y * w.z - v.z * w.y,
                    v.z * w.x - v.x * w.z,
                    v.x * w.y - v.y * w.x).normalized;

                norms[face.Vertex0] -= norm;
                norms[face.Vertex1] -= norm;
                norms[face.Vertex2] -= norm;
            }

            for (var i = 0; i < src.VertexCount; ++i)
            {
                if (norms[i].sqrMagnitude <= 0f)
                {
                    norms[i] = UnityEngine.Vector3.up;
                }
                else
                {
                    norms[i] = norms[i].normalized;
                }
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

            if (vehicle && alpha)
            {
                return Shader.Find("SanAndreasUnity/VehicleTransparent");
            }

            if (vehicle)
            {
                return Shader.Find("SanAndreasUnity/Vehicle");
            }

            if (noBackCull && alpha)
            {
                return Shader.Find("SanAndreasUnity/TransparentNoBackCull");
            }

            if (noBackCull)
            {
                return Shader.Find("SanAndreasUnity/NoBackCull");
            }

            if (alpha)
            {
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

        private static readonly Color32[] _sKeyColors = new[] {
            new Color32(255, 255, 255, 255),

            // Paint job
            new Color32(60, 255, 0, 255),
            new Color32(255, 0, 175, 255),
            new Color32(60, 255, 0, 255),  // TODO
            new Color32(255, 0, 175, 255), // TODO

            // Head lights
            new Color32(255, 175, 0, 255),   // L
            new Color32(0, 255, 200, 255), // R

            // Tail lights
            new Color32(185, 255, 0, 255), // L
            new Color32(255, 60, 0, 255)   // R
        };

        private static LoadedTexture _sWhiteTex;

        private static LoadedTexture WhiteTex
        {
            get { return _sWhiteTex ?? (_sWhiteTex = new LoadedTexture(Texture2D.whiteTexture, false)); }
        }

        private static UnityEngine.Material Convert(
            RenderWareStream.Material src,
            RenderWareStream.Geometry geometry,
            TextureDictionary[] txds,
            MaterialFlags flags)
        {
            LoadedTexture diffuse;
            LoadedTexture mask = null;

            var overrideAlpha = (flags & MaterialFlags.OverrideAlpha) == MaterialFlags.OverrideAlpha;
            var vehicle = (flags & MaterialFlags.Vehicle) == MaterialFlags.Vehicle;

            if (!overrideAlpha && src.Colour.A != 255)
            {
                flags |= MaterialFlags.Alpha;
            }

            if (src.TextureCount > 0)
            {
                var tex = src.Textures[0];
                diffuse = txds.GetDiffuse(tex.TextureName);

                if (src.TextureCount > 1)
                {
                    Debug.LogFormat("Something has {0} textures!", src.TextureCount);
                }

                if (diffuse == null)
                {
                    Debug.LogWarningFormat("Unable to find texture {0}", tex.TextureName);
                }

                if (!string.IsNullOrEmpty(tex.MaskName))
                {
                    mask = txds.GetAlpha(tex.MaskName) ?? diffuse;
                }
                else if (vehicle)
                {
                    mask = diffuse;
                }

                if (!overrideAlpha && mask != null && mask.HasAlpha)
                {
                    flags |= MaterialFlags.Alpha;
                }
            }
            else
            {
                diffuse = WhiteTex;
            }

            var shader = GetShader(flags);
            var mat = new UnityEngine.Material(shader);

            var clr = Types.Convert(src.Colour);

            if (vehicle)
            {
                var found = false;
                for (var i = 1; i < _sKeyColors.Length; ++i)
                {
                    var key = _sKeyColors[i];
                    if (key.r != clr.r || key.g != clr.g || key.b != clr.b) continue;
                    mat.SetInt(CarColorIndexId, i);
                    found = true;
                    break;
                }

                if (found)
                {
                    mat.color = Color.white;
                }
                else
                {
                    mat.color = clr;
                }
            }
            else
            {
                mat.color = clr;
            }

            if (diffuse != null) mat.SetTexture(MainTexId, diffuse.Texture);
            if (mask != null) mat.SetTexture(MaskTexId, mask.Texture);

            mat.SetFloat(MetallicId, src.Specular);
            mat.SetFloat(SmoothnessId, src.Smoothness);

            if (geometry.ExtraVertColor != null && geometry.ExtraVertColor.Colors != null)
            {
                mat.SetFloat(HasNightColorsPropertyId, 1);
            }

            return mat;
        }

        private static Mesh Convert(RenderWareStream.Geometry src)
        {
            var mesh = new Mesh();

            var meshVertices = src.Vertices;
            mesh.vertices = meshVertices;

            if (src.Normals != null)
            {
                mesh.normals = src.Normals;
            }

            if (src.Colours != null)
            {
                mesh.colors32 = src.Colours;
            }

            if (src.ExtraVertColor != null && src.ExtraVertColor.Colors != null)
            {
                // store night colors in UV coordinates, because Unity mesh can not hold multiple colors per vertex
                mesh.uv2 = src.ExtraVertColor.Colors;
                mesh.uv3 = src.ExtraVertColor.Colors2;
            }

            if (src.TexCoords != null && src.TexCoords.Length > 0)
            {
                mesh.uv = src.TexCoords[0];
            }

            if (src.Normals == null)
            {
                mesh.normals = CalculateNormals(src, meshVertices);
            }

            mesh.subMeshCount = src.MaterialSplits.Length;

            var isTriangleStrip = (src.Flags & GeometryFlag.TriangleStrips) == GeometryFlag.TriangleStrips;

            var subMesh = 0;
            foreach (var split in src.MaterialSplits)
            {
                var indices = isTriangleStrip
                    ? FromTriangleStrip(split.FaceIndices)
                    : split.FaceIndices;
                mesh.SetIndices(indices, MeshTopology.Triangles, subMesh++);
            }

            mesh.RecalculateBounds();

            return mesh;
        }

        private static GeometryFrame Convert(RenderWareStream.Frame src, IEnumerable<Atomic> atomics)
        {
            var atomic = atomics.FirstOrDefault(x => x.FrameIndex == src.Index);

            return new GeometryFrame(src, atomic);
        }

        private static Transform[] Convert(HierarchyAnimation hAnim, Dictionary<GeometryFrame, Transform> transforms, IEnumerable<GeometryFrame> frames)
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
			public CollisionFile Collisions { get { return _collisions; } }

            public readonly string Name;
            public readonly Geometry[] Geometry;
            public readonly GeometryFrame[] Frames;

            public GeometryParts(string name, Clump clump, TextureDictionary[] txds)
            {
                Profiler.BeginSample("GeometryParts()");

                Name = name;

                Geometry = clump.GeometryList.Geometry
                    .Select(x => new Geometry(x, Convert(x), txds))
                    .ToArray();

                Frames = clump.FrameList.Frames
                    .Select(x => Convert(x, clump.Atomics))
                    .ToArray();

                _collisions = clump.Collision;

                Profiler.EndSample();
            }

            public void AttachCollisionModel(Transform destParent, bool forceConvex = false)
            {
				Profiler.BeginSample ("AttachCollisionModel");
                if (_collisions != null)
                {
                    CollisionModel.Load(_collisions, destParent, forceConvex);
                }
                else
                {
                    CollisionModel.Load(Name, destParent, forceConvex);
                }
				Profiler.EndSample ();
            }

            public FrameContainer AttachFrames(Transform destParent, MaterialFlags flags)
            {
                var transforms = Frames.ToDictionary(x => x, x =>
                {
                    var trans = new GameObject(x.Name).transform;
                    trans.localPosition = x.Position;
                    trans.localRotation = x.Rotation;
                    return trans;
                });

                var container = destParent.gameObject.AddComponent<FrameContainer>();

                foreach (var frame in Frames)
                {
                    if (frame.ParentIndex != -1)
                    {
                        transforms[frame].SetParent(transforms[Frames[frame.ParentIndex]], false);
                    }
                    else
                    {
                        transforms[frame].SetParent(destParent, false);
                    }

                    if (frame.GeometryIndex != -1)
                    {
                        var gobj = transforms[frame].gameObject;
                        var geometry = Geometry[frame.GeometryIndex];

                        HierarchyAnimation hAnim = null;
                        var parent = frame;
                        while ((hAnim = parent.Source.HAnim) == null || hAnim.NodeCount == 0)
                        {
                            if (parent.ParentIndex == -1)
                            {
                                hAnim = null;
                                break;
                            }
                            parent = Frames[parent.ParentIndex];
                        }

                        Renderer renderer;
                        if (hAnim != null)
                        {
                            var smr = gobj.AddComponent<SkinnedMeshRenderer>();

                            var bones = Convert(hAnim, transforms, Frames);

                            smr.rootBone = bones[0];
                            smr.bones = bones;

                            smr.sharedMesh = geometry.Mesh;

                            if (smr.sharedMesh != null)
                            {
                                smr.sharedMesh.bindposes = geometry.SkinToBoneMatrices
                                    .Select(x => x.transpose)
                                    .ToArray();
                            }

                            renderer = smr;
                        }
                        else
                        {
                            var mf = gobj.AddComponent<MeshFilter>();
                            mf.sharedMesh = geometry.Mesh;

                            renderer = gobj.AddComponent<MeshRenderer>();
                        }

                        renderer.sharedMaterials = geometry.GetMaterials(flags);

                        // filter these out for now
                        if (frame.Name.EndsWith("_vlo") ||
                            frame.Name.EndsWith("_dam"))
                        {
                            gobj.SetActive(false);
                        }
                    }
                }

                container.Initialize(Frames, transforms);

                return container;
            }
        }


		private static AsyncLoader<string, GeometryParts> s_asyncLoader = new AsyncLoader<string, GeometryParts> ();

		public static int NumGeometryPartsLoaded { get { return s_asyncLoader.GetNumObjectsLoaded (); } }


        public static GeometryParts Load(string modelName, params string[] texDictNames)
        {
            return Load(modelName, texDictNames.Select(x => TextureDictionary.Load(x)).ToArray());
        }

		public static void LoadAsync(string modelName, string[] texDictNames, float loadPriority, System.Action<GeometryParts> onFinish)
		{
            // copy array to local variable
			texDictNames = texDictNames.Length > 0 ? texDictNames.ToArray() : Array.Empty<string>();

			if (0 == texDictNames.Length)
			{
				LoadAsync( modelName, Array.Empty<TextureDictionary>(), loadPriority, onFinish );
				return;
			}

            // requesting a load for both clump and TXD on the same frame will not give much better performance (probably),
            // so no need to do it

			var loadedTextDicts = new List<TextureDictionary> ();

			for (int i = 0; i < texDictNames.Length; i++)
			{
                TextureDictionary.LoadAsync (texDictNames [i], loadPriority, (texDict) => {
					
					loadedTextDicts.Add (texDict);

					if (loadedTextDicts.Count == texDictNames.Length)
					{
						// finished loading all tex dicts
						LoadAsync (modelName, loadedTextDicts.ToArray (), loadPriority, onFinish);
					}
				});
			}

		}

        public static GeometryParts Load(string modelName, params TextureDictionary[] txds)
        {
            modelName = modelName.ToLowerIfNotLower();

			if (s_asyncLoader.TryGetLoadedObject(modelName, out GeometryParts alreadyLoadedObject))
            {
                return alreadyLoadedObject;
            }

			Profiler.BeginSample ("ReadClump");
            var clump = ArchiveManager.ReadFile<Clump>(modelName + ".dff");
			Profiler.EndSample ();

            if (clump.GeometryList == null)
            {
                throw new Exception("Invalid mesh");
            }

			Profiler.BeginSample ("Create GeometryParts");
            var loaded = new GeometryParts(modelName, clump, txds);
			Profiler.EndSample ();

			s_asyncLoader.OnObjectFinishedLoading(modelName, loaded, true);

            return loaded;
        }

		public static void LoadAsync(string modelName, TextureDictionary[] txds, float loadPriority, System.Action<GeometryParts> onFinish)
		{
			modelName = modelName.ToLowerIfNotLower();

			if (!s_asyncLoader.TryLoadObject (modelName, onFinish))
			{
				// callback is either called or registered
				return;
			}
			

			GeometryParts loadedGeoms = null;

			LoadingThread.RegisterJob (new BackgroundJobRunner.Job<Clump> () {
                priority = loadPriority,
				action = () => {
					// read archive file in background thread
					var clump = ArchiveManager.ReadFile<Clump>(modelName + ".dff");
					return clump;
				},
				callbackSuccess = (Clump clump) => {
					if (clump.GeometryList == null)
					{
						throw new Exception("Invalid mesh");
					}

					// create geometry parts in main thread
					loadedGeoms = new GeometryParts(modelName, clump, txds);

				},
				callbackFinish = (result) => {
					s_asyncLoader.OnObjectFinishedLoading( modelName, loadedGeoms, loadedGeoms != null );
				}
			});

		}

        public readonly Mesh Mesh;

        private readonly RenderWareStream.Geometry _geom;
        public RenderWareStream.Geometry RwGeometry => _geom;

        public readonly TextureDictionary[] _textureDictionaries;

        private readonly Dictionary<MaterialFlags, UnityEngine.Material[]> _materials;

        public readonly UnityEngine.Matrix4x4[] SkinToBoneMatrices;

        private Geometry(RenderWareStream.Geometry geom, Mesh mesh, TextureDictionary[] textureDictionaries)
        {
            Mesh = mesh;

            if (geom.Skinning != null)
            {
                Mesh.boneWeights = Types.Convert(geom.Skinning.VertexBoneIndices, geom.Skinning.VertexBoneWeights);
                SkinToBoneMatrices = Types.Convert(geom.Skinning.SkinToBoneMatrices);
            }

            _geom = geom;
            _textureDictionaries = textureDictionaries;
            _materials = new Dictionary<MaterialFlags, UnityEngine.Material[]>();
        }

        public UnityEngine.Material[] GetMaterials(
            ObjectFlag flags,
            Action<UnityEngine.Material> setupMaterial)
        {
            var matFlags = MaterialFlags.Default | MaterialFlags.OverrideAlpha;

            if ((flags & ObjectFlag.NoBackCull) == ObjectFlag.NoBackCull)
            {
                matFlags |= MaterialFlags.NoBackCull;
            }

            if ((flags & (ObjectFlag.DrawLast | ObjectFlag.Additive)) != 0
                && (flags & ObjectFlag.NoZBufferWrite) == ObjectFlag.NoZBufferWrite)
            {
                matFlags |= MaterialFlags.Alpha;
            }

            return GetMaterials(matFlags, setupMaterial);
        }

        public UnityEngine.Material[] GetMaterials(MaterialFlags flags)
        {
            return GetMaterials(flags, x => { });
        }

        public UnityEngine.Material[] GetMaterials(MaterialFlags flags,
            Action<UnityEngine.Material> setupMaterial)
        {
            if (_materials.ContainsKey(flags))
            {
                return _materials[flags];
            }

            var mats = _geom.Materials.Select(x =>
            {
                var mat = Convert(x, _geom, _textureDictionaries, flags);
                setupMaterial(mat);
                return mat;
            }).ToArray();

            mats = _geom.MaterialSplits.Select(x => mats[x.MaterialIndex]).ToArray();

            _materials.Add(flags, mats);

            return mats;
        }
    }
}