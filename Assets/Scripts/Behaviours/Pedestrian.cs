using System.IO;
using System.Linq;
using System.Collections.Generic;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SanAndreasUnity.Behaviours
{
    [ExecuteInEditMode]
    public class Pedestrian : MonoBehaviour
    {
        private int _loadedPedestrianId;
        private AnimType _loadedAnimType = AnimType.None;

        private Frame _root;

        private AnimationGroup _animGroup;
        public UnityEngine.Animation Anim { get; private set; }

        private FrameContainer _frames;

        public PedestrianDef Definition { get; private set; }

        public int PedestrianId = 7;

        public AnimType AnimType = AnimType.Idle;

        public bool Walking
        {
            set { AnimType = value ? AnimType.Walk : AnimType.Idle; }
            get { return AnimType == AnimType.Walk || Running; }
        }

        public bool Running
        {
            set { AnimType = value ? AnimType.Run : AnimType.Walk; }
            get { return AnimType == AnimType.Run || AnimType == AnimType.Panicked; }
        }

        public float Speed { get; private set; }

        public Vector3 Position
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        private void LateUpdate()
        {
            if (_root == null) return;

            var trans = _root.transform;
            
            Speed = _root.LocalVelocity.z;
            trans.parent.localPosition = new Vector3(0f, -trans.localPosition.y * .5f, -trans.localPosition.z);
        }

        private void Update()
        {
            if (!Loader.HasLoaded) return;
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying && !EditorApplication.isPaused) return;
#endif

            if (_loadedPedestrianId != PedestrianId)
            {
                _loadedPedestrianId = PedestrianId;
                _loadedAnimType = AnimType.None;

                Load(PedestrianId);
            }

            if (_loadedAnimType != AnimType)
            {
                _loadedAnimType = AnimType;

                LoadAnim(AnimType);
            }
        }

        private void OnValidate()
        {
            if (_frames != null) Update();
        }

        private void Load(int id)
        {
            Definition = Item.GetDefinition<PedestrianDef>(id);
            if (Definition == null) return;

            LoadModel(Definition.ModelName, Definition.TextureDictionaryName);

            _animGroup = AnimationGroup.Get(Definition.AnimGroupName);

            Anim = gameObject.GetComponent<UnityEngine.Animation>();

            if (Anim == null) {
                Anim = gameObject.AddComponent<UnityEngine.Animation>();
            }
        }

        private void LoadModel(string modelName, params string[] txds)
        {
            if (_frames != null)
            {
                Destroy(_frames.Root.gameObject);
                Destroy(_frames);
            }

            var geoms = Geometry.Load(modelName, txds);
            _frames = geoms.AttachFrames(transform, MaterialFlags.Default);

            _root = _frames.GetByName("Root");
        }

        private void LoadAnim(AnimType type)
        {
            if (type == AnimType.None)
            {
                Anim.Stop();

                return;
            }

            var animName = _animGroup[AnimType];

            LoadAnim(animName);
        }

        public void LoadAnim(string animName)
        {
            if (!Anim.GetClip(animName)) {
                var clip = Importing.Conversion.Animation.Load(_animGroup.FileName, animName, _frames);
                Anim.AddClip(clip, animName);
            }

            Anim.CrossFade(animName);
        }
    }
}
