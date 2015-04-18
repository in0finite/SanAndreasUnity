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
        private int _curPedestrianId;
        private AnimGroup _curAnimGroup = AnimGroup.None;
        private AnimIndex _curAnim = AnimIndex.None;

        private UnityEngine.Animation _anim;

        private FrameContainer _frames;
        private Frame _root;

        public PedestrianDef Definition { get; private set; }

        public int PedestrianId = 7;

        public AnimGroup AnimGroup = AnimGroup.WalkCycle;
        public AnimIndex AnimIndex = AnimIndex.Idle;

        public bool Walking
        {
            set
            {
                AnimGroup = AnimGroup.WalkCycle;
                AnimIndex = value ? AnimIndex.Walk : AnimIndex.Idle;
            }
            get
            {
                return AnimGroup == AnimGroup.WalkCycle
                    && (AnimIndex == AnimIndex.Walk || Running);
            }
        }

        public bool Running
        {
            set
            {
                AnimGroup = AnimGroup.WalkCycle;
                AnimIndex = value ? AnimIndex.Run : AnimIndex.Walk;
            }
            get
            {
                return AnimGroup == AnimGroup.WalkCycle
                    && (AnimIndex == AnimIndex.Run || AnimIndex == AnimIndex.Panicked);
            }
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

            if (_curPedestrianId != PedestrianId)
            {
                Load(PedestrianId);
            }

            if (_curAnim != AnimIndex || _curAnimGroup != AnimGroup)
            {
                CrossFadeAnim(AnimGroup, AnimIndex, 0.3f, PlayMode.StopAll);
            }
        }

        private void OnValidate()
        {
            if (_frames != null) Update();
        }

        private void Load(int id)
        {
            _curPedestrianId = PedestrianId;

            Definition = Item.GetDefinition<PedestrianDef>(id);
            if (Definition == null) return;

            LoadModel(Definition.ModelName, Definition.TextureDictionaryName);

            _curAnim = AnimIndex.None;
            _curAnimGroup = AnimGroup.None;

            _anim = gameObject.GetComponent<UnityEngine.Animation>();

            if (_anim == null) {
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

            _root = _frames.GetByName("Root");
        }

        public AnimationState LoadAnim(AnimGroup group, AnimIndex anim)
        {
            return LoadAnim(group, anim, false);
        }

        public AnimationState PlayAnim(AnimGroup group, AnimIndex anim, PlayMode playMode)
        {
            return LoadAnim(group, anim, true, playMode);
        }

        public AnimationState CrossFadeAnim(AnimGroup group, AnimIndex anim, float duration, PlayMode playMode)
        {
            return LoadAnim(group, anim, true, playMode, duration);
        }

        public AnimationClip GetAnim(AnimGroup group, AnimIndex anim)
        {
            var animGroup = AnimationGroup.Get(Definition.AnimGroupName, group);
            return _anim.GetClip(animGroup[anim]);
        }

        private AnimationState LoadAnim(AnimGroup group, AnimIndex anim,
            bool play, PlayMode playMode = PlayMode.StopAll, float crossFadeDuration = 0f)
        {
            if (anim == AnimIndex.None) {
                if (play) _anim.Stop();
                return null;
            }

            var animGroup = AnimationGroup.Get(Definition.AnimGroupName, group);
            var animName = animGroup[anim];

            if (play) {
                _curAnimGroup = AnimGroup = group;
                _curAnim = AnimIndex = anim;
            }

            AnimationState state;

            if (!_anim.GetClip(animName)) {
                var clip = Importing.Conversion.Animation.Load(animGroup.FileName, animName, _frames);
                _anim.AddClip(clip, animName);
                state = _anim[animName];
            } else {
                state = _anim[animName];
            }

            if (!play) return state;

            if (crossFadeDuration > 0f) {
                _anim.CrossFade(animName, crossFadeDuration, playMode);
            } else {
                _anim.Play(animName, playMode);
            }

            return state;
        }
    }
}
