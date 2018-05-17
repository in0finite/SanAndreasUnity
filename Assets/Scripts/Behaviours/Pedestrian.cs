using System.Collections.Generic;
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
    using Anim = SanAndreasUnity.Importing.Conversion.Animation;

    [ExecuteInEditMode]
    public class Pedestrian : MonoBehaviour
    {
        private int _curPedestrianId;
        private AnimGroup _curAnimGroup = AnimGroup.None;
        private AnimIndex _curAnim = AnimIndex.None;
        //	private	string	_curAnim = "" ;

        public UnityEngine.Animation _anim { get; private set; }

        private FrameContainer _frames;
        public FrameContainer Frames { get { return this._frames; } }
        private Frame _root;

        private readonly Dictionary<string, Anim> _loadedAnims
            = new Dictionary<string, Anim>();

        public PedestrianDef Definition { get; private set; }

        public int PedestrianId = 7;

        // have we loaded model since the Loader has finished loading
        private bool loadedModelOnStartup = false;

        public AnimGroup AnimGroup = AnimGroup.WalkCycle;
        public AnimIndex animIndex = AnimIndex.Idle;
        //	public	string	animIndex = "" ;

        public bool IsInVehicle { get; set; }

        public Vector3 VehicleParentOffset { get; set; }

        public Transform weapon = null;
        private Transform m_leftFinger = null;
        private Transform m_rightFinger = null;

        private Player _player;

        public bool Walking
        {
            set
            {
                AnimGroup = AnimGroup.WalkCycle;
                animIndex = value ? AnimIndex.Walk : AnimIndex.Idle;
                this.PlayAnim(AnimGroup, animIndex, PlayMode.StopAll);
            }
            get
            {
                return AnimGroup == AnimGroup.WalkCycle
                    && (animIndex == AnimIndex.Walk || Running);
            }
        }

        public bool WalkingArmed
        {
            set
            {
                AnimGroup = AnimGroup.MyWalkCycle;
                animIndex = value ? AnimIndex.Walk : AnimIndex.IdleArmed;
            }
        }

        public bool Running
        {
            set
            {
                AnimGroup = AnimGroup.WalkCycle;
                animIndex = value ? AnimIndex.Run : AnimIndex.Walk;
                this.PlayAnim(AnimGroup, animIndex, PlayMode.StopAll);
            }
            get
            {
                return AnimGroup == AnimGroup.WalkCycle
                    && (animIndex == AnimIndex.Run || animIndex == AnimIndex.Panicked);
            }
        }

        public float Speed { get; private set; }

        public Vector3 Position
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        private void Start()
        {
            // can not use these functions because Loader has not finished loading

            //	Load (PedestrianId);
            //	PlayAnim (AnimGroup.WalkCycle, AnimIndex.Idle, PlayMode.StopAll);

            _player = transform.parent.gameObject.GetComponent<Player>();
        }

        private void LateUpdate()
        {
            if (_root == null) return;

            var trans = _root.transform;

            if (IsInVehicle)
            {
                // 'Anchor' pedestrian model into the vehicle
                Speed = 0.0f;
                trans.parent.localPosition = VehicleParentOffset;
            }
            else
            {
                // Store movement defined by animation for pedestrian model
                Speed = _root.LocalVelocity.z;
                trans.parent.localPosition = new Vector3(0f, -trans.localPosition.y * .5f, -trans.localPosition.z);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                return;
#endif

            if (Loader.HasLoaded)
            {
                if (!loadedModelOnStartup)
                {
                    _player.OnSpawn();

                    // load model on startup
                    Debug.Log("Loading pedestrian model after startup.");
                    Load(PedestrianId);
                    // and play animation
                    PlayAnim(AnimGroup.WalkCycle, AnimIndex.Idle, PlayMode.StopAll);

                    loadedModelOnStartup = true;
                }
            }

            // update transform of weapon
            if (weapon != null && m_rightFinger != null && m_leftFinger != null)
            {
                weapon.transform.position = m_rightFinger.transform.position;
                Vector3 dir = (m_leftFinger.transform.position - m_rightFinger.transform.position).normalized;
                Quaternion q = Quaternion.LookRotation(dir, transform.up);
                Vector3 upNow = q * Vector3.up;
                dir = Quaternion.AngleAxis(-90, upNow) * dir;
                weapon.transform.rotation = Quaternion.LookRotation(dir, transform.up);
            }
        }

        private void OnValidate()
        {
            //	if (null == _frames)
            //		return;

            if (!Loader.HasLoaded) return;

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying && !EditorApplication.isPaused)
                return;
#endif

            if (_curPedestrianId != PedestrianId)
            {
                Load(PedestrianId);
            }

            if (_curAnim != animIndex || _curAnimGroup != AnimGroup)
            {
#if UNITY_EDITOR
                PlayAnim(AnimGroup, animIndex, PlayMode.StopAll);
#else
				CrossFadeAnim(AnimGroup, animIndex, 0.3f, PlayMode.StopAll);
#endif
            }
        }

        private void OnDrawGizmosSelected()
        {
            float size = 0.1f;
            Gizmos.color = Color.red;

            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smr in renderers)
            {
                foreach (Transform tr in smr.bones)
                {
#if UNITY_EDITOR
                    //	Handles.Label(tr.position, tr.name);
#endif
                    Gizmos.DrawWireCube(tr.position, new Vector3(size, size, size));
                }
            }
        }

        public void Load(int id)
        {
            _curPedestrianId = PedestrianId = id;

            Definition = Item.GetDefinition<PedestrianDef>(id);
            if (Definition == null) return;

            LoadModel(Definition.ModelName, Definition.TextureDictionaryName);

            _curAnim = AnimIndex.None;
            //	_curAnim = "" ;
            _curAnimGroup = AnimGroup.None;

            _anim = gameObject.GetComponent<UnityEngine.Animation>();

            if (_anim == null)
            {
                _anim = gameObject.AddComponent<UnityEngine.Animation>();
            }

            LoadAnim(AnimGroup.WalkCycle, AnimIndex.Walk);
            LoadAnim(AnimGroup.WalkCycle, AnimIndex.Run);
            LoadAnim(AnimGroup.WalkCycle, AnimIndex.Panicked);
            LoadAnim(AnimGroup.WalkCycle, AnimIndex.Idle);
            LoadAnim(AnimGroup.WalkCycle, AnimIndex.RoadCross);
            LoadAnim(AnimGroup.WalkCycle, AnimIndex.WalkStart);

            LoadAnim(AnimGroup.Car, AnimIndex.Sit);
            LoadAnim(AnimGroup.Car, AnimIndex.DriveLeft);
            LoadAnim(AnimGroup.Car, AnimIndex.DriveRight);
            LoadAnim(AnimGroup.Car, AnimIndex.GetInLeft);
            LoadAnim(AnimGroup.Car, AnimIndex.GetInRight);
            LoadAnim(AnimGroup.Car, AnimIndex.GetOutLeft);
            LoadAnim(AnimGroup.Car, AnimIndex.GetOutRight);

            LoadAnim(AnimGroup.MyWalkCycle, AnimIndex.IdleArmed);
            LoadAnim(AnimGroup.MyWalkCycle, AnimIndex.GUN_STAND);

            //	LoadAllAnimations ();
        }

        private void LoadModel(string modelName, params string[] txds)
        {
            if (_frames != null)
            {
                Destroy(_frames.Root.gameObject);
                Destroy(_frames);
                _loadedAnims.Clear();
            }

            var geoms = Geometry.Load(modelName, txds);
            _frames = geoms.AttachFrames(transform, MaterialFlags.Default);

            _root = _frames.GetByName("Root");

            m_rightFinger = _frames.GetByName(" R Finger").transform;
            m_leftFinger = _frames.GetByName(" L Finger").transform;
        }

        public AnimationState PlayAnim(AnimGroup group, AnimIndex anim, PlayMode playMode)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            _anim.Play(animState.name, playMode);

            return animState;
        }

        public AnimationState CrossFadeAnim(AnimGroup group, AnimIndex anim, float duration, PlayMode playMode)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            _anim.CrossFade(animState.name, duration, playMode);

            return animState;
        }

        public AnimationState CrossFadeAnimQueued(AnimGroup group, AnimIndex anim, float duration, QueueMode queueMode, PlayMode playMode)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            _anim.CrossFadeQueued(animState.name, duration, queueMode, playMode);

            return animState;
        }

        public Anim GetAnim(AnimGroup group, AnimIndex anim)
        {
            var animGroup = AnimationGroup.Get(Definition.AnimGroupName, group);

            var animName = animGroup[anim];

            Anim result;
            return _loadedAnims.TryGetValue(animName, out result) ? result : null;
        }

        public string GetAnimName(AnimGroup group, AnimIndex anim)
        {
            var animGroup = AnimationGroup.Get(Definition.AnimGroupName, group);

            return animGroup[anim];
        }

        private AnimationState LoadAnim(AnimGroup group, AnimIndex anim)
        {
            if (anim == AnimIndex.None)
            {
                return null;
            }
            //	if ("" == anim)
            //		return null;

            if (group == AnimGroup.None)
            {
                return null;
            }

            var animGroup = AnimationGroup.Get(Definition.AnimGroupName, group);
            if (null == animGroup)
                return null;
            var animName = animGroup[anim];
            //	var animName = anim ;
            //	if (!animGroup.HasAnimation (animName))
            //		return null;

            AnimationState state;

            if (!_loadedAnims.ContainsKey(animName))
            {
                var clip = Anim.Load(animGroup.FileName, animName, _frames);
                if (clip != null)
                {
                    _loadedAnims.Add(animName, clip);
                    _anim.AddClip(clip.Clip, animName);
                    state = _anim[animName];
                }
                else
                {
                    state = null;
                    Debug.LogWarning(string.Format("File '{0}' doesn't exists!", animGroup.FileName));
                }
            }
            else
            {
                state = _anim[animName];
            }

            return state;
        }

        public void LoadAllAnimations()
        {
            foreach (Dictionary<AnimGroup, AnimationGroup> dict in AnimationGroup._sGroups.Values)
            {
                foreach (AnimationGroup animGroup in dict.Values)
                {
                    foreach (var animName in animGroup.Animations)
                    {
                        if (!_loadedAnims.ContainsKey(animName))
                        {
                            var clip = Importing.Conversion.Animation.Load(animGroup.FileName, animName, _frames);
                            _loadedAnims.Add(animName, clip);
                            _anim.AddClip(clip.Clip, animName);
                            //	state = _anim [animName];
                        }
                    }
                }
            }
        }
    }
}