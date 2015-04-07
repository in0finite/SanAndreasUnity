using System.IO;
using System.Linq;
using System.Collections.Generic;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(MeshFilter), typeof(SkinnedMeshRenderer))]
    [ExecuteInEditMode]
    public class PedestrianTest : MonoBehaviour
    {
        private int _loadedPedestrianId;

        public Pedestrian Definition { get; private set; }

        public int PedestrianId = 7;

        private List<Transform> _bones = new List<Transform>();
        private List<Matrix4x4> _bindPoses = new List<Matrix4x4>();

        private void Update()
        {
            if (!Loader.HasLoaded) return;
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying && !EditorApplication.isPaused) return;
#endif

            if (_loadedPedestrianId != PedestrianId) {
                _loadedPedestrianId = PedestrianId;

                Load(PedestrianId);
            }
        }
        
        private void OnValidate()
        {
            Update();
        }

        private void Load(int id)
        {
            Definition = Item.GetDefinition<Pedestrian>(id);
            if (Definition == null) return;

            LoadModel(Definition.ModelName, Definition.TextureDictionaryName);
        }

        private void LoadModel(string modelName, params string[] txds)
        {
            _bones.ForEach(x => GameObject.Destroy(x.gameObject));

            _bones.Clear();
            _bindPoses.Clear();

            var mf = GetComponent<MeshFilter>();
            var mr = GetComponent<SkinnedMeshRenderer>();

            var geoms = Geometry.Load(modelName, txds);

            mf.sharedMesh = geoms.Geometry[0].Mesh;
            mr.sharedMaterials = geoms.Geometry[0].GetMaterials(MaterialFlags.Default);

            mr.sharedMesh = mf.sharedMesh;

            for (int i = 0; i < geoms.Frames.Length; ++i)
            {
                var frame = geoms.Frames[i];

                Transform parent;
                var parentIndex = frame.ParentIndex;

                if (parentIndex < 0) parent = transform;
                else parent = _bones[parentIndex];

                AddBone(frame, parent);
            }

            mf.sharedMesh.bindposes = _bindPoses.ToArray();
            mr.bones = _bones.ToArray();

            var animation = new SanAndreasUnity.Importing.Animation.AnimationPackage(new BinaryReader(ArchiveManager.ReadFile("colt45.ifp")));
        }

        private Transform AddBone(Geometry.GeometryFrame frame, Transform parent)
        {
            var child = new GameObject();
            child.name = frame.Name;
            child.transform.SetParent(parent, false);

            child.transform.localPosition = frame.Position;
            child.transform.localRotation = frame.Rotation;

            _bones.Add(child.transform);

            _bindPoses.Add(child.transform.worldToLocalMatrix * transform.localToWorldMatrix);


            return child.transform;
        }
    }
}
