using System.Collections.Generic;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    using Anim = SanAndreasUnity.Importing.Conversion.Animation;

    public class Pedestrian : MonoBehaviour
    {
        
		/// <summary> State of last played anim. </summary>
		public AnimationState LastAnimState { get; private set; }

		public AnimationState LastSecondaryAnimState { get; private set; }

		/// <summary> Last played anim. </summary>
		public AnimId LastAnimId { get; private set; }

		public AnimId LastSecondaryAnimId { get; private set; }

		private UnityEngine.Animation _anim { get; set; }
		public UnityEngine.Animation AnimComponent { get { return _anim; } }

        private FrameContainer _frames;
        public FrameContainer Frames { get { return _frames; } }
        private Frame _root;
		public Frame RootFrame { get { return _root; } }

        private readonly Dictionary<string, Anim> _loadedAnims
            = new Dictionary<string, Anim>();

        public PedestrianDef Definition { get; private set; }

		[SerializeField] private int m_startingPedId = 167;
		public int StartingPedId { get { return m_startingPedId; } set { m_startingPedId = value; } }

		public int PedestrianId { get; private set; }

        // have we loaded model since the Loader has finished loading
        private bool loadedModelOnStartup = false;

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

		public Transform Neck { get; private set; }

        public Transform Spine { get; private set; }

        public Transform R_Thigh { get; private set; }
        public Transform L_Thigh { get; private set; }

        private Player _player;

		/// <summary> Speed of the model extracted from animation. </summary>
        public float Speed { get; private set; }

		private Dictionary<AnimationState, List<Transform>> m_mixedTransforms = new Dictionary<AnimationState, List<Transform>>();



        private void Awake()
        {
			_player = this.GetComponentInParent<Player> ();
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

            if (Loader.HasLoaded)
            {
                if (!loadedModelOnStartup)
                {
                    _player.OnSpawn();

                    // load model on startup
                    Debug.Log("Loading pedestrian model after startup.");
					Load (m_startingPedId);
                    // and play animation
                    PlayAnim(AnimGroup.WalkCycle, AnimIndex.Idle, PlayMode.StopAll);

                    loadedModelOnStartup = true;
                }
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
                    Gizmos.DrawWireCube(tr.position, new Vector3(size, size, size));
                }
            }
        }


        public void Load(int id)
        {
            
			var newDefinition = Item.GetDefinition<PedestrianDef>(id);
			if (null == newDefinition)
				return;

			PedestrianId = id;
			Definition = newDefinition;

            LoadModel(Definition.ModelName, Definition.TextureDictionaryName);

            LastAnimId = default (AnimId);
			LastSecondaryAnimId = default (AnimId);
			LastAnimState = null;
			LastSecondaryAnimState = null;

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
			m_rightHand = _frames.GetByName (" R Hand").transform;
			m_leftHand = _frames.GetByName (" L Hand").transform;
			RightUpperArm = _frames.GetByName (" R UpperArm").transform;
			LeftUpperArm = _frames.GetByName (" L UpperArm").transform;
			RightForeArm = _frames.GetByName (" R ForeArm").transform;
			LeftForeArm = _frames.GetByName (" L ForeArm").transform;
			Head = _frames.GetByName (" Head").transform;
			Neck = _frames.GetByName (" Neck").transform;
			Spine = _frames.GetByName(" Spine").transform;
            R_Thigh = _frames.GetByName(" R Thigh").transform;
            L_Thigh = _frames.GetByName(" L Thigh").transform;
        }

		/// <summary>
		/// Resets the state of the model. This includes position, rotation, and velocity of every frame (bone).
		/// It does it by playing idle anim, setting it's time to 0, and sampling from it.
		/// </summary>
		public void ResetModelState ()
		{

			var state = PlayAnim (AnimGroup.WalkCycle, AnimIndex.Idle);
			state.normalizedTime = 0;
			AnimComponent.Sample ();

		}


		public AnimationState PlayAnim (AnimId animId, PlayMode playMode = PlayMode.StopAll)
        {
			var animState = LoadAnim (animId);
            if (null == animState)
                return null;

			LastAnimId = animId;
			LastSecondaryAnimId = new AnimId ();
			LastAnimState = animState;
			LastSecondaryAnimState = null;

			//animState.layer = 0;
			RemoveAllMixingTransforms (animState);

            _anim.Play (animState.name, playMode);

            return animState;
        }

		public AnimationState PlayAnim (AnimGroup animGroup, AnimIndex animIndex, PlayMode playMode = PlayMode.StopAll)
		{
			return PlayAnim (new AnimId (animGroup, animIndex), playMode);
		}

		public AnimationState PlayAnim (AnimId animId, bool resetModelStateIfAnimChanged, bool resetAnimStateIfAnimChanged)
		{
			bool animChanged = !animId.Equals (LastAnimId);

			if (resetModelStateIfAnimChanged && animChanged) {
				this.ResetModelState ();
			}

			var state = PlayAnim (animId);

			if (resetAnimStateIfAnimChanged && animChanged) {
				state.enabled = true;
				state.normalizedTime = 0;
				state.speed = 1;
				state.weight = 1;
				state.wrapMode = this.AnimComponent.wrapMode;
			}

			return state;
		}

		public bool Play2Anims (AnimId animIdA, AnimId animIdB)
		{
			// load anims

			var stateA = LoadAnim (animIdA);
			var stateB = LoadAnim (animIdB);

			if (null == stateA || null == stateB)
				return false;

			// reset model state if anims changed

			bool animsChanged = !animIdA.Equals (LastAnimId) || !animIdB.Equals (LastSecondaryAnimId);

			if (animsChanged) {
				//ResetModelState ();
			}

			// play anims

			// are mixing transforms and layers preserved ? - probably

			RemoveAllMixingTransforms (stateA);
			RemoveAllMixingTransforms (stateB);

			AddMixingTransform (stateA, this.Spine, true);

			AddMixingTransform (stateB, _root.transform, false);
			AddMixingTransform (stateB, this.L_Thigh, true);
			AddMixingTransform (stateB, this.R_Thigh, true);

			stateA.layer = 0;
			stateB.layer = 1;

			this.AnimComponent.Play( stateA.clip.name, PlayMode.StopSameLayer);
			this.AnimComponent.Play( stateB.clip.name, PlayMode.StopSameLayer);


			// assign last anims

			LastAnimId = animIdA;
			LastSecondaryAnimId = animIdB;
			LastAnimState = stateA;
			LastSecondaryAnimState = stateB;


			return true;
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

		public bool AddMixingTransform (AnimationState state, Transform tr, bool recursive)
		{
			List<Transform> list;
			if (m_mixedTransforms.TryGetValue (state, out list)) {
				if (list.Contains (tr))
					return false;
				state.AddMixingTransform (tr, recursive);
				list.Add (tr);
				return true;
			} else {
				state.AddMixingTransform (tr, recursive);
				list = new List<Transform> (){ tr };
				m_mixedTransforms.Add (state, list);
				return true;
			}
		}

		public bool RemoveMixingTransform (AnimationState state, Transform tr)
		{
			List<Transform> list;
			if (m_mixedTransforms.TryGetValue (state, out list)) {
				if (!list.Contains (tr))
					return false;
				state.RemoveMixingTransform (tr);
				list.Remove (tr);
				return true;
			} else {
				return false;
			}
		}
        
		public int RemoveAllMixingTransforms (AnimationState state)
		{
			List<Transform> list;
			if (m_mixedTransforms.TryGetValue (state, out list)) {
				int count = list.Count;
				foreach (var mix in list) {
					state.RemoveMixingTransform (mix);
				}
				list.Clear ();
				return count;
			} else {
				return 0;
			}
		}


        public Anim GetAnim (AnimGroup group, AnimIndex anim)
        {
            string animName = GetAnimName (group, anim);
			if (string.IsNullOrEmpty (animName))
				return null;

            Anim result;
            return _loadedAnims.TryGetValue (animName, out result) ? result : null;
        }

        public string GetAnimName (AnimGroup group, AnimIndex anim)
        {
			string animName = null, fileName = null;
			if (GetAnimNameAndFile (group, anim, ref animName, ref fileName))
				return animName;

			return null;
        }

		public bool GetAnimNameAndFile (AnimGroup group, AnimIndex anim, ref string animName, ref string fileName)
		{
			if (anim == AnimIndex.None)
				return false;
			
			if (group == AnimGroup.None)
				return false;
			
			if (null == this.Definition || string.IsNullOrEmpty (this.Definition.AnimGroupName))
				return false;

			var animGroup = AnimationGroup.Get (this.Definition.AnimGroupName, group);
			if (null == animGroup)
				return false;

			animName = animGroup [anim];
			fileName = animGroup.FileName;

			return true;
		}


        private AnimationState LoadAnim (AnimGroup group, AnimIndex anim)
        {
			string animName = null, fileName = null;
			if (GetAnimNameAndFile (group, anim, ref animName, ref fileName))
				return LoadAnim (animName, fileName);

			return null;
        }

		private AnimationState LoadAnim (string animName, string fileName)
		{
			AnimationState state = null;

			if (!_loadedAnims.ContainsKey(animName))
			{
				var importedAnim = Anim.Load(fileName, animName, _frames);
				if (importedAnim != null && importedAnim.Clip != null)
				{
					_loadedAnims.Add(animName, importedAnim);
					_anim.AddClip(importedAnim.Clip, animName);
					state = _anim[animName];
				}
				else
				{
					Debug.LogErrorFormat ("Failed to load anim - file: {0}, anim name: {1}", fileName, animName);
				}
			}
			else
			{
				state = _anim[animName];
			}

			return state;
		}

		private AnimationState LoadAnim (AnimId animId)
		{
			return animId.UsesAnimGroup ? LoadAnim (animId.AnimGroup, animId.AnimIndex) : LoadAnim (animId.AnimName, animId.FileName);
		}

		private void LoadAllAnimations()
        {
			foreach (var dict in AnimationGroup._sGroups.Values)
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