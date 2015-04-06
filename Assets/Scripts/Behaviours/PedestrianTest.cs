using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class PedestrianTest : MonoBehaviour
    {
        private int _loadedPedestrianId;

        public Pedestrian Definition { get; private set; }

        public int PedestrianId = 7;

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
            Definition = GameData.GetDefinition<Pedestrian>(id);
            if (Definition == null) return;

            LoadModel(Definition.ModelName, Definition.TextureDictionaryName);
        }

        private void LoadModel(string modelName, params string[] txds)
        {
            var mf = GetComponent<MeshFilter>();
            var mr = GetComponent<MeshRenderer>();

            var geoms = Geometry.Load(modelName, txds);

            mf.sharedMesh = geoms.Geometry[0].Mesh;
            mr.sharedMaterials = geoms.Geometry[0].GetMaterials(MaterialFlags.Default);
        }
    }
}
