using System.IO;
using System.Linq;
using System.Collections.Generic;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Animation;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class PedestrianTest : MonoBehaviour
    {
        private int _loadedPedestrianId;

        public Pedestrian Definition { get; private set; }

        public int PedestrianId = 7;

        private AnimationPackage _anim;

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
            var geoms = Geometry.Load(modelName, txds);
            geoms.AttachFrames(transform, MaterialFlags.Default);

            _anim = new AnimationPackage(new BinaryReader(ArchiveManager.ReadFile("ped.ifp")));//Definition.AnimFileName + ".ifp")));

            gameObject.AddComponent<UnityEngine.Animation>();

            var curve = UnityEngine.AnimationCurve.Linear(0, 1, 2, 3);
            var clip = new UnityEngine.AnimationClip();
            clip.legacy = true;
            clip.SetCurve("unnamed/Root/ Pelvis/ Spine/ Spine1/ Neck/ Head", typeof(Transform), "localPosition.x", curve);

            GetComponent<UnityEngine.Animation>().AddClip(clip, "test");
            GetComponent<UnityEngine.Animation>().Play("test");
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
