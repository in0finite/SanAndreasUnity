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

            var geoms = Geometry.Load(modelName, txds);
            geoms.AttachFrames(transform, MaterialFlags.Default);

            var animation = new SanAndreasUnity.Importing.Animation.AnimationPackage(new BinaryReader(ArchiveManager.ReadFile("colt45.ifp")));
        }

        void OnDrawGizmos()
        {
            foreach (var bone in GetComponentsInChildren<Transform>())
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(bone.position, 0.02f);

                if (bone.parent != null)
                {
                    Gizmos.DrawLine(bone.position, bone.parent.position);
                }
            }
        }
    }
}
