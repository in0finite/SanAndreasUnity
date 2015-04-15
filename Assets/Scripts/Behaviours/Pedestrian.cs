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

        private AnimationGroup _animGroup;
        private UnityEngine.Animation _anim;

        private FrameContainer _frames;

        public PedestrianDef Definition { get; private set; }

        public int PedestrianId = 3;

        public AnimType Anim = AnimType.Idle;

        public bool Walking
        {
            set { Anim = value ? AnimType.Walk : AnimType.Idle; }
            get { return Anim == AnimType.Walk || Running; }
        }

        public bool Running
        {
            set { Anim = value ? AnimType.Run : AnimType.Walk; }
            get { return Anim == AnimType.Run || Anim == AnimType.Panicked; }
        }

        public Vector3 Position
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
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

            if (_loadedAnimType != Anim)
            {
                _loadedAnimType = Anim;

                LoadAnim(Anim);
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

            _anim = gameObject.GetComponent<UnityEngine.Animation>();

            if (_anim == null)
            {
                _anim = gameObject.AddComponent<UnityEngine.Animation>();
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
        }

        private void LoadAnim(AnimType type)
        {
            if (type == AnimType.None)
            {
                _anim.Stop();

                return;
            }

            var animName = _animGroup[Anim];

            LoadAnim(animName);
        }

        public void LoadAnim(string animName)
        {
            var clip = Importing.Conversion.Animation.Load(_animGroup.FileName, animName, _frames);

            _anim.AddClip(clip, animName);
            _anim.CrossFade(animName);
        }
    }
}
