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
        public FrameContainer Frames { get { return _frames; } }
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

        private Transform m_leftFinger = null;
		public Transform LeftFinger { get { return m_leftFinger; } }

        private Transform m_rightFinger = null;
		public Transform RightFinger { get { return m_rightFinger; } }
        
		private Transform m_leftHand;
		public Transform LeftHand { get { return m_leftHand; } }

		private Transform m_rightHand;
		public Transform RightHand { get { return m_rightHand; } }

		public Transform LeftUpperArm { get; private set; }
		public Transform RightUpperArm { get; private set; }

		public Transform LeftForeArm { get; private set; }
		public Transform RightForeArm { get; private set; }

		public Transform Head { get; private set; }

        public Transform Spine { get; private set; }
        public Transform R_Thigh { get; private set; }
        public Transform L_Thigh { get; private set; }

        private Player _player;


        public bool Walking
        {
            set
            {
                AnimGroup = AnimGroup.WalkCycle;
                animIndex = value ? AnimIndex.Walk : AnimIndex.Idle;
                PlayAnim(AnimGroup, animIndex, PlayMode.StopAll);
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
                PlayAnim(AnimGroup, animIndex, PlayMode.StopAll);
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

        public void ChangeSpineRotation(Vector3 bulletdir, Vector3 adir, float rotationSpeed, ref Vector3 tempSpineLocalEulerAngles, ref Quaternion targetRot, ref Quaternion spineRotationLastFrame)
        {
            //Rotate the spine bone so the gun (roughly) aims at the target
            Spine.rotation = Quaternion.FromToRotation(bulletdir, adir) * Spine.rotation;

            tempSpineLocalEulerAngles = Spine.localEulerAngles;


            //Stop our agent from breaking their back by rotating too far
            tempSpineLocalEulerAngles = new Vector3(ResetIfTooHigh(tempSpineLocalEulerAngles.x, 90),
                                                    ResetIfTooHigh(tempSpineLocalEulerAngles.y, 90),
                                                    ResetIfTooHigh(tempSpineLocalEulerAngles.z, 90));

            Spine.localEulerAngles = tempSpineLocalEulerAngles;
            targetRot = Spine.rotation;

            //Smoothly rotate to the new position.  
            Spine.rotation = Quaternion.Slerp(spineRotationLastFrame, targetRot, Time.deltaTime * rotationSpeed);
            spineRotationLastFrame = Spine.rotation;
        }
        
        public static float ResetIfTooHigh(float r, float lim)
        {

            if (r > 180)
                r -= 360;

            if (r < -lim || r > lim)
            {
                return 0;
            }
            else
                return r;
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
			m_rightHand = _frames.GetByName (" R Hand").transform;
			m_leftHand = _frames.GetByName (" L Hand").transform;
			RightUpperArm = _frames.GetByName (" R UpperArm").transform;
			LeftUpperArm = _frames.GetByName (" L UpperArm").transform;
			RightForeArm = _frames.GetByName (" R ForeArm").transform;
			LeftForeArm = _frames.GetByName (" L ForeArm").transform;
			Head = _frames.GetByName (" Head").transform;
			Spine = _frames.GetByName(" Spine").transform;
            R_Thigh = _frames.GetByName(" R Thigh").transform;
            L_Thigh = _frames.GetByName(" L Thigh").transform;
        }

		public AnimationState PlayAnim(AnimGroup group, AnimIndex anim, PlayMode playMode = PlayMode.StopAll)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            _anim.Play(animState.name, playMode);

            return animState;
        }

        public AnimationState AddMixingTransform(AnimGroup group, AnimIndex anim, Transform mix)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            animState.AddMixingTransform(mix);

            return animState;
        }

        public AnimationState RemoveMixingTransform(AnimGroup group, AnimIndex anim, Transform mix)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            animState.RemoveMixingTransform(mix);

            return animState;
        }

        public void PlayUpperLayerAnimations(
           AnimGroup upperLayerGroup, AnimGroup group, AnimIndex upperLayerIndex, AnimIndex animIndex)
        {
            LoadAnim(upperLayerGroup, upperLayerIndex);

            _anim[GetAnimName(upperLayerGroup, upperLayerIndex)].layer = 1;

            AnimationState state = PlayAnim(upperLayerGroup, upperLayerIndex, PlayMode.StopSameLayer);

            state.normalizedTime = 1;

            //state.AddMixingTransform(Spine, true);

            //foreach (Transform t in Spine.GetComponentInChildren<Transform>())
            //{
            //    //	runState.wrapMode = WrapMode.Loop;
            //}

            LoadAnim(group, animIndex);

            _anim[GetAnimName(group, animIndex)].layer = 0;

            state = PlayAnim(group, animIndex, PlayMode.StopSameLayer);

			state.AddMixingTransform(_root.transform, false);

            state.AddMixingTransform(L_Thigh, true);

            //foreach (Transform t in L_Thigh.GetComponentInChildren<Transform>())
            //{
            //    //	state.RemoveMixingTransform(f.transform);
            //    //	state.wrapMode = WrapMode.Loop;
            //}

            state.AddMixingTransform(R_Thigh, true);

            //foreach (Transform t in R_Thigh.GetComponentInChildren<Transform>())
            //{
            //    //	state.RemoveMixingTransform(f.transform);
            //    //	state.wrapMode = WrapMode.Loop;
            //}
            //state.weight = animationBlendWeight;

            //	PlayerModel._anim.Blend( );
        }

        public AnimationState AddMixingTransform(AnimGroup group, AnimIndex anim, Transform mix, bool recursive)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            animState.AddMixingTransform(mix, recursive);

            return animState;
        }

        public AnimationState Blend(AnimGroup group, AnimIndex anim)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            animState.AddMixingTransform(Spine);

            _anim.Blend(animState.name);

            return animState;
        }

        public AnimationState Blend(AnimGroup group, AnimIndex anim, float targetWeight)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            animState.AddMixingTransform(Spine);

            _anim.Blend(animState.name, targetWeight);

            return animState;
        }

        public AnimationState Blend(AnimGroup group, AnimIndex anim, float targetWeight, float fadeLength)
        {
            var animState = LoadAnim(group, anim);
            if (null == animState)
                return null;

            _curAnimGroup = AnimGroup = group;
            _curAnim = animIndex = anim;

            animState.AddMixingTransform(Spine);

            _anim.Blend(animState.name, targetWeight, fadeLength);

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

            if (Definition == null || (Definition != null && string.IsNullOrEmpty(Definition.AnimGroupName))) return null;
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
                if (clip.Clip != null)
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