using System.Collections.Generic;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
    using Anim = SanAndreasUnity.Importing.Conversion.Animation;

    public class PedModel : MonoBehaviour
    {
        
		/// <summary> State of last played anim. </summary>
		public AnimationState LastAnimState { get; private set; }

		public AnimationState LastSecondaryAnimState { get; private set; }

		/// <summary> Last played anim. </summary>
		public AnimId LastAnimId { get; private set; }

		public AnimId LastSecondaryAnimId { get; private set; }

		/// <summary> Did anims changed when played them last time ? </summary>
		public bool AnimsChanged { get { return this.FirstAnimChanged || this.SecondAnimChanged; } }
		public bool FirstAnimChanged { get; private set; }
		public bool SecondAnimChanged { get; private set; }

		private readonly AnimId m_invalidAnimId = new AnimId ();

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

		public Transform LeftClavicle { get; private set; }
		public Transform RightClavicle { get; private set; }

		public Transform Head { get; private set; }

		public Transform Neck { get; private set; }
		public Transform LBreast { get; private set; }
		public Transform RBreast { get; private set; }

		public Transform UpperSpine { get; private set; }
		public Transform Belly { get; private set; }

        public Transform Spine { get; private set; }
        public Transform R_Thigh { get; private set; }
        public Transform L_Thigh { get; private set; }

		public Transform Pelvis { get; private set; }

		public class FrameAnimData
		{
			public Vector3 pos;
			public Quaternion rot;
			public Vector3 velocity;
			public Frame frame;

			public FrameAnimData (Vector3 pos, Quaternion rot, Vector3 velocity, Frame frame)
			{
				this.pos = pos;
				this.rot = rot;
				this.velocity = velocity;
				this.frame = frame;
			}
		}

		private List<FrameAnimData> m_originalFrameDatas = new List<FrameAnimData> ();
		public List<FrameAnimData> OriginalFrameDatas { get { return m_originalFrameDatas; } }

		private Ped m_ped;

		/// <summary> Velocity of the model extracted from animation. </summary>
		public Vector3 Velocity { get; private set; }

		/// <summary> Velocity axis to use. </summary>
		public int VelocityAxis { get; set; }

		private Dictionary<AnimationState, List<Transform>> m_mixedTransforms = new Dictionary<AnimationState, List<Transform>>();

		public event System.Action onLateUpdate = delegate {};



        private void Awake()
        {
			m_ped = this.GetComponentInParent<Ped> ();
        }

        private void LateUpdate()
        {
            if (_root == null) return;

            var trans = _root.transform;

            if (IsInVehicle)
            {
                // 'Anchor' pedestrian model into the vehicle
				this.Velocity = Vector3.zero;
                trans.parent.localPosition = VehicleParentOffset;
            }
            else
            {
                // Store movement defined by animation for pedestrian model
                this.Velocity = _root.LocalVelocity;
                trans.parent.localPosition = new Vector3(0f, -trans.localPosition.y * .5f, -trans.localPosition.z);
            }

			this.onLateUpdate ();
        }

        private void Update()
        {

            if (Loader.HasLoaded)
            {
                if (!loadedModelOnStartup)
                {
					loadedModelOnStartup = true;

                    m_ped.OnSpawn();

                    // load model on startup
                    //Debug.Log("Loading pedestrian model after startup.");
					Load (m_startingPedId);

                    // and play animation
					PlayAnim(AnimGroup.WalkCycle, AnimIndex.Idle);

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

			LastAnimId = m_invalidAnimId;
			LastSecondaryAnimId = m_invalidAnimId;
			LastAnimState = null;
			LastSecondaryAnimState = null;

			m_originalFrameDatas.Clear ();

			_anim = this.gameObject.GetOrAddComponent<UnityEngine.Animation> ();

			LoadModel(Definition.ModelName, Definition.TextureDictionaryName);

			// save original model state
			// TODO: we should first reset all anim parameters (eg mixing transforms) ?

			var state = PlayAnim(AnimGroup.WalkCycle, AnimIndex.Idle);

			if (state != null) {
				state.time = 0f;
				this.AnimComponent.Sample ();
			}

			this.SaveModelState ();

			// load some anims

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
				_frames = null;
                _loadedAnims.Clear();
            }

            var geoms = Geometry.Load(modelName, txds);
            _frames = geoms.AttachFrames(transform, MaterialFlags.Default);

			// we have to remove white spaces from frame names and their game objects, because some models
			// have white spaces, and some don't - and we need to have the same frame names for all models,
			// otherwise animations will not work on all of them
			foreach (var frame in _frames)
			{
				frame.Name = frame.Name.Replace (" ", "");
				frame.gameObject.name = frame.Name;
			}


            _root = _frames.GetByName("Root");

			System.Func<string, Transform> getFrame = (string frameName) => {
				Frame frame = _frames.GetByName (frameName, true);
				if(null == frame) {
					Debug.LogErrorFormat("Failed to find frame by name '{0}' on model '{1}'", frameName, modelName);
					return null;
				}
				return frame.transform;
			};

            m_rightFinger = getFrame(" R Finger");
            m_leftFinger = getFrame(" L Finger");
			m_rightHand = getFrame (" R Hand");
			m_leftHand = getFrame (" L Hand");
			RightUpperArm = getFrame (" R UpperArm");
			LeftUpperArm = getFrame (" L UpperArm");
			RightForeArm = getFrame (" R ForeArm");
			LeftForeArm = getFrame (" L ForeArm");
			RightClavicle = getFrame ("Bip01 R Clavicle");
			LeftClavicle = getFrame ("Bip01 L Clavicle");
			Head = getFrame (" Head");
			Neck = getFrame (" Neck");
			LBreast = getFrame ("L breast");
			RBreast = getFrame ("R breast");
			UpperSpine = getFrame (" Spine1");
			Belly = getFrame ("Belly");
			Spine = getFrame(" Spine");
            R_Thigh = getFrame(" R Thigh");
            L_Thigh = getFrame(" L Thigh");
			Pelvis = getFrame(" Pelvis");

        }

		/// <summary>
		/// Resets the state of the model. This includes position, rotation, and velocity of every frame (bone).
		/// It does it by playing idle anim, setting it's time to 0, and sampling from it.
		/// </summary>
		public void ResetModelState ()
		{

			//var state = PlayAnim (AnimGroup.WalkCycle, AnimIndex.Idle);
			//state.normalizedTime = 0;
			//AnimComponent.Sample ();

			foreach (var frameData in m_originalFrameDatas) {
				if (null == frameData.frame)
					continue;
				ResetFrameState (frameData);
			}

		}

		private void SaveModelState ()
		{
			m_originalFrameDatas.Clear ();

			foreach (var frame in this.Frames) {
				m_originalFrameDatas.Add (new FrameAnimData (frame.transform.localPosition, frame.transform.localRotation, 
					frame.LocalVelocity, frame));
			}
		}

		private void ResetFrameState (FrameAnimData frameData)
		{
			frameData.frame.transform.localPosition = frameData.pos;
			frameData.frame.transform.localRotation = frameData.rot;
			frameData.frame.LocalVelocity = frameData.velocity;
		}

		public void ResetFrameState (Transform frameTransform)
		{
			Frame frame = frameTransform.GetComponent<Frame> ();
			if (null == frame)
				return;

			var frameData = m_originalFrameDatas.FirstOrDefault( f => f.frame == frame );
			if (frameData != null) {
				ResetFrameState (frameData);
			}

		}


		public AnimationState PlayAnim (AnimId animId, PlayMode playMode = PlayMode.StopAll)
        {
			this.FirstAnimChanged = this.SecondAnimChanged = false;

			var animState = LoadAnim (animId);
            if (null == animState)
                return null;

			this.FirstAnimChanged = !animId.Equals (LastAnimId);
			this.SecondAnimChanged = !m_invalidAnimId.Equals (LastSecondaryAnimId);

			LastAnimId = animId;
			LastSecondaryAnimId = m_invalidAnimId;
			LastAnimState = animState;
			LastSecondaryAnimState = null;

			// reset velocity axis
			this.VelocityAxis = 2;

			//animState.layer = 0;
			RemoveAllMixingTransforms (animState);

            _anim.Play (animState.name, playMode);

            return animState;
        }

		public AnimationState PlayAnim (AnimGroup animGroup, AnimIndex animIndex, PlayMode playMode = PlayMode.StopAll)
		{
			return PlayAnim (new AnimId (animGroup, animIndex), playMode);
		}

		public bool Play2Anims (AnimId animIdA, AnimId animIdB)
		{

			this.FirstAnimChanged = this.SecondAnimChanged = false;

			// load anims

			var stateA = LoadAnim (animIdA);
			var stateB = LoadAnim (animIdB);

			if (null == stateA || null == stateB)
				return false;
			
			this.FirstAnimChanged = !animIdA.Equals (LastAnimId);
			this.SecondAnimChanged = !animIdB.Equals (LastSecondaryAnimId);

			// reset velocity axis
			this.VelocityAxis = 2;

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

		public void AddMixingTransforms (AnimationState state, params Transform[] transforms)
		{
			foreach (var tr in transforms)
				AddMixingTransform (state, tr, false);
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