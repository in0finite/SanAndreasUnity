using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.VehicleDef;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        [Flags]
        public enum WheelAlignment
        {
            None = 0,

            Front = 1,
            Mid = 2,
            Rear = 4,

            Left = 8,
            Right = 16,

            LeftRightMask = Left | Right,
            FrontMidRearMask = Front | Mid | Rear,

            RightFront = Right | Front,
            LeftFront = Left | Front,
            RightMid = Right | Mid,
            LeftMid = Left | Mid,
            RightRear = Right | Rear,
            LeftRear = Left | Rear,
        }

        [Flags]
        public enum SeatAlignment
        {
            None = 0,

            Front = 1,
            Back = 2,

            Left = 4,
            Right = 8,

            FrontBackMask = Front | Back,
            LeftRightMask = Left | Right,

            FrontRight = Front | Right,
            FrontLeft = Front | Left,
            BackRight = Back | Right,
            BackLeft = Back | Left,
        }

        public enum DoorAlignment
        {
            None,
            RightFront,
            LeftFront,
            RightRear,
            LeftRear,
        }

        public enum VehicleComponentsRules
        {
            ALLOW_ALWAYS = 1,
            ONLY_WHEN_RAINING = 2,
            MAYBE_HIDE = 3,
            FULL_RANDOM = 4,
        }

        private static VehicleDef[] _sRandomSpawnable;
        private static int _sMaxSpawnableIndex;

        private static VehicleDef[] GetRandomSpawnableDefs(out int maxIndex)
        {
            var all = Item.GetDefinitions<VehicleDef>().ToArray();

            var defs = all
                .Where(x => x.Frequency > 0 && x.VehicleType == VehicleType.Car)
                .ToArray();

            maxIndex = defs.Sum(x => x.Frequency);

            return defs;
        }

        private static VehicleDef GetRandomDef()
        {
            if (_sRandomSpawnable == null)
            {
                _sRandomSpawnable = GetRandomSpawnableDefs(out _sMaxSpawnableIndex);
            }

            var index = UnityEngine.Random.Range(0, _sMaxSpawnableIndex);
            foreach (var def in _sRandomSpawnable)
            {
                index -= def.Frequency;
                if (index < 0) return def;
            }

            throw new Exception("Unable to find cars to spawn");
        }


		public static void GetPositionForSpawning(Transform inFrontOfTransform, out Vector3 pos, out Quaternion rot) {

			pos = Vector3.zero;
			rot = Quaternion.identity;

			Vector3 spawnOffset = new Vector3 (0, 2, 5);

			pos = inFrontOfTransform.position + inFrontOfTransform.forward * spawnOffset.z + inFrontOfTransform.up * spawnOffset.y
				+ inFrontOfTransform.right * spawnOffset.x;
			rot = Quaternion.LookRotation(-inFrontOfTransform.right, Vector3.up);

		}

        public static Vehicle Create(VehicleSpawnMapObject spawner)
        {
            return Create(spawner.Info.CarId, spawner.Info.Colors, spawner.transform.position,
                spawner.transform.rotation);
        }

		public static Vehicle Create(int carId, Vector3 position, Quaternion rotation)
		{
			return Create (carId, null, position, rotation);
		}

		public static Vehicle CreateInFrontOf(int carId, Transform inFrontOfTransform) {

			Vector3 pos;
			Quaternion rot;

			GetPositionForSpawning (inFrontOfTransform, out pos, out rot);

			return Create (carId, pos, rot);
		}

        public static Vehicle CreateRandomInFrontOf(Transform inFrontOfTransform)
        {
            return CreateInFrontOf(-1, inFrontOfTransform);
        }

        public static Vehicle Create(int carId, int[] colors, Vector3 position, Quaternion rotation)
        {
            GameObject go = Instantiate(VehicleManager.Instance.vehiclePrefab);
            try
            {
                var v = Create(go, carId, colors, position, rotation);
                if (Net.NetStatus.IsServer)
                {
                    v.GetComponent<VehicleController>().OnAfterCreateVehicle();
                    Net.NetManager.Spawn(go);
                }
                return v;
            }
            catch
            {
                // if something fails, destroy the game object
                Destroy(go);
                throw;
            }
        }

        public static Vehicle Create(GameObject vehicleGameObject, int carId, int[] colors, 
            Vector3 position, Quaternion rotation)
        {
            
            var inst = vehicleGameObject.AddComponent<Vehicle>();

            VehicleDef def;
            if (carId == -1)
            {
                def = GetRandomDef();
            }
            else
            {
                def = Item.GetDefinition<VehicleDef>(carId);
            }

            inst.Initialize(def, colors);

            inst.transform.position = position - Vector3.up * inst.AverageWheelHeight;
            inst.transform.localRotation = rotation;

            return inst;
        }

        private Geometry.GeometryParts _geometryParts;

        public class Wheel
        {
            public WheelAlignment Alignment { get; set; }

            public bool IsLeftHand
            {
                get { return (Alignment & WheelAlignment.Left) == WheelAlignment.Left; }
            }

            public bool IsRightHand
            {
                get { return (Alignment & WheelAlignment.Right) == WheelAlignment.Right; }
            }

            public bool IsFront
            {
                get { return (Alignment & WheelAlignment.Front) == WheelAlignment.Front; }
            }

            public bool IsMid
            {
                get { return (Alignment & WheelAlignment.Mid) == WheelAlignment.Mid; }
            }

            public bool IsRear
            {
                get { return (Alignment & WheelAlignment.Rear) == WheelAlignment.Rear; }
            }

            public Transform Parent { get; set; }
            public Transform Child { get; set; }
            public WheelCollider Collider { get; set; }
            public Wheel Complement { get; set; }

            public float Travel { get; private set; }

            public void UpdateTravel()
            {
                Travel = 1f;

                WheelHit hit;
                if (Collider.GetGroundHit(out hit))
                {
                    Travel = (-Parent.transform.InverseTransformPoint(hit.point).y - Collider.radius) / Collider.suspensionDistance;
                }
            }

            public Quaternion Roll { get; set; }
        }

        public class Seat
        {
            public SeatAlignment Alignment { get; internal set; }

            public Transform Parent { get; internal set; }

			/// <summary> Ped that is occupying this seat. </summary>
			public Ped OccupyingPed { get; internal set; }

            public double TimeWhenPedChanged { get; internal set; } = double.NegativeInfinity;
            public double TimeSincePedChanged => Time.timeAsDouble - this.TimeWhenPedChanged;

			public bool IsTaken { get { return this.OccupyingPed != null; } }

            public bool IsLeftHand
            {
                get { return (Alignment & SeatAlignment.Left) == SeatAlignment.Left; }
            }

            public bool IsRightHand
            {
                get { return (Alignment & SeatAlignment.Right) == SeatAlignment.Right; }
            }

            public bool IsFront
            {
                get { return (Alignment & SeatAlignment.Front) == SeatAlignment.Front; }
            }

            public bool IsBack
            {
                get { return (Alignment & SeatAlignment.Back) == SeatAlignment.Back; }
            }

            public bool IsDriver
            {
                get { return Alignment == SeatAlignment.FrontLeft; }
            }
        }

        private FrameContainer _frames;
        public FrameContainer Frames => _frames;

        public Transform EngineTransform { get; private set; }
        public Transform PetrolcapTransform { get; private set; }

        private static GameObject s_highDetailMeshesContainer;

        public Transform HighDetailMeshesParent { get; private set; }
        private List<KeyValuePair<Transform, Transform>> m_highDetailMeshObjectsToUpdate = new List<KeyValuePair<Transform, Transform>>();

        private readonly List<Wheel> _wheels = new List<Wheel>();
        private readonly List<Seat> _seats = new List<Seat>();
        private readonly List<Frame> _extras = new List<Frame>();

        public List<Wheel> Wheels { get { return _wheels; } }
        public List<Seat> Seats { get { return _seats; } }
        public List<Frame> Extras { get { return _extras; } }

        private WheelAlignment GetWheelAlignment(string frameName)
        {
            switch (frameName)
            {
                case "wheel_rf_dummy":
                    return WheelAlignment.RightFront;

                case "wheel_lf_dummy":
                    return WheelAlignment.LeftFront;

                case "wheel_rm_dummy":
                    return WheelAlignment.RightMid;

                case "wheel_lm_dummy":
                    return WheelAlignment.LeftMid;

                case "wheel_rb_dummy":
                    return WheelAlignment.RightRear;

                case "wheel_lb_dummy":
                    return WheelAlignment.LeftRear;

                default:
                    return WheelAlignment.None;
            }
        }

        private DoorAlignment GetDoorAlignment(string frameName)
        {
            switch (frameName)
            {
                case "door_rf_dummy":
                    return DoorAlignment.RightFront;

                case "door_lf_dummy":
                    return DoorAlignment.LeftFront;

                case "door_rr_dummy":
                    return DoorAlignment.RightRear;

                case "door_lr_dummy":
                    return DoorAlignment.LeftRear;

                default:
                    return DoorAlignment.None;
            }
        }

        public Transform GetPart(string name)
        {
            var frame = _frames.GetByName(name);
            return frame != null ? frame.transform : null;
        }

        private void AttachSeat(Transform parent, SeatAlignment alignment)
        {
            _seats.Add(new Seat { Parent = parent, Alignment = alignment });
        }

        public static Geometry.GeometryParts LoadGeometryParts(VehicleDef vehicleDef)
        {
            return Geometry.Load(vehicleDef.ModelName,
                TextureDictionary.Load(vehicleDef.TextureDictionaryName),
                TextureDictionary.Load("vehicle"),
                TextureDictionary.Load("misc"));
        }

        private void Initialize(VehicleDef def, int[] colors = null)
        {
            Definition = def;

            if (colors != null && colors[0] != -1)
            {
                SetColors(colors);
            }
            else
            {
                var defaultClrs = CarColors.GetCarDefaults(Definition.ModelName);

                if (defaultClrs != null)
                {
                    SetColors(defaultClrs[UnityEngine.Random.Range(0, defaultClrs.Count)]);
                }
                else
                {
                    Debug.LogWarningFormat("No colours defined for {0}!", def.GameName);
                }
            }

            name = Definition.GameName;

            _geometryParts = LoadGeometryParts(Definition);

            _frames = _geometryParts.AttachFrames(transform, MaterialFlags.Vehicle);

            var wheelFrame = _frames.FirstOrDefault(x => x.Name == "wheel");

            if (wheelFrame == null)
            {
                Debug.LogWarningFormat("No wheels defined for {0}!", def.GameName);
                Destroy(gameObject);
                return;
            }

            var engineFrame = _frames.FirstOrDefault(x => x.Name == "engine");
            if (engineFrame != null)
                this.EngineTransform = engineFrame.transform;

            var petrolcapFrame = _frames.FirstOrDefault(x => x.Name == "petrolcap");
            if (petrolcapFrame != null)
                this.PetrolcapTransform = petrolcapFrame.transform;

            foreach (var frame in _frames)
            {
                if (frame.Name.StartsWith("extra"))
                {
                    _extras.Add(frame);
                }

                if (!frame.Name.StartsWith("wheel_")) continue;
                if (!frame.Name.EndsWith("_dummy")) continue;

                var childFrames = _frames.Where(x => x.ParentIndex == frame.Index);

                // disable all children of wheel dummies
                foreach (var childFrame in childFrames)
                {
                    childFrame.gameObject.SetActive(false);
                }

                var wheelAlignment = GetWheelAlignment(frame.Name);

                Wheel inst;

                // see if this wheel dummy has a wheel child
                var wheel = childFrames.FirstOrDefault(x => x.Name == "wheel");

                if (wheel == null)
                {
                    var copy = Instantiate(wheelFrame.transform);
                    copy.SetParent(frame.transform, false);

                    _wheels.Add(inst = new Wheel
                    {
                        Alignment = wheelAlignment,
                        Parent = frame.transform,
                        Child = copy,
                    });
                }
                else
                {
                    // all children of wheel dummies get set to inactive so activate this one
                    wheel.gameObject.SetActive(true);

                    _wheels.Add(inst = new Wheel
                    {
                        Alignment = wheelAlignment,
                        Parent = frame.transform,
                        Child = wheel.transform,
                    });
                }

                if (inst.IsLeftHand)
                {
                    frame.transform.Rotate(Vector3.up, 180.0f);
                }

                inst.Complement = _wheels.FirstOrDefault(x =>
                    (x.Alignment & WheelAlignment.LeftRightMask) != (inst.Alignment & WheelAlignment.LeftRightMask) &&
                    (x.Alignment & WheelAlignment.FrontMidRearMask) == (inst.Alignment & WheelAlignment.FrontMidRearMask));

                if (inst.Complement != null)
                {
                    inst.Complement.Complement = inst;
                }
            }

            SelectExtras();

            InitializePhysics();

            this.Health = this.MaxHealth = Mathf.Pow(this.HandlingData.Mass, VehicleManager.Instance.massToHealthExponent);

            //this.SetupDoorsHingeJoints();

            var frontSeat = GetPart("ped_frontseat");
            var backSeat = GetPart("ped_backseat");

            if (frontSeat != null)
            {
                var frontSeatMirror = new GameObject("ped_frontseat").transform;
                frontSeatMirror.SetParent(frontSeat.parent, false);
                frontSeatMirror.localPosition = Vector3.Scale(frontSeat.localPosition, new Vector3(-1f, 1f, 1f));

                if (frontSeat.localPosition.x > 0f)
                {
                    AttachSeat(frontSeat, SeatAlignment.FrontRight);
                    AttachSeat(frontSeatMirror, SeatAlignment.FrontLeft);
                }
                else
                {
                    AttachSeat(frontSeatMirror, SeatAlignment.FrontRight);
                    AttachSeat(frontSeat, SeatAlignment.FrontLeft);
                }

                DriverTransform = GetSeat(SeatAlignment.FrontLeft).Parent;
            }

            if (backSeat != null)
            {
                var backSeatMirror = new GameObject("ped_backseat").transform;
                backSeatMirror.SetParent(backSeat.parent, false);
                backSeatMirror.localPosition = Vector3.Scale(backSeat.localPosition, new Vector3(-1f, 1f, 1f));

                if (backSeat.localPosition.x > 0f)
                {
                    AttachSeat(backSeat, SeatAlignment.BackRight);
                    AttachSeat(backSeatMirror, SeatAlignment.BackLeft);
                }
                else
                {
                    AttachSeat(backSeatMirror, SeatAlignment.BackRight);
                    AttachSeat(backSeat, SeatAlignment.BackLeft);
                }
            }

            // Add vehicle damage

            /*
            var dam = gameObject.AddComponent<VehicleDamage>();
            dam.damageParts = new Transform[] { transform.GetChild(0).Find("engine") };
            dam.deformMeshes = gameObject.GetComponentsInChildren<MeshFilter>();
            dam.displaceParts = gameObject.GetComponentsInChildren<Transform>().Where(x => x.GetComponent<Frame>() != null || x.GetComponent<FrameContainer>() != null).ToArray();
            dam.damageFactor = VehicleAPI.constDamageFactor;
            dam.collisionIgnoreHeight = -.4f;
            dam.collisionTimeGap = .1f;

            //OptimizeVehicle();

            dam.deformColliders = gameObject.GetComponentsInChildren<MeshCollider>();
            */
            

            UGameCore.Utilities.GameObjectExtensions.SetLayerRecursive(gameObject, Layer);

            SetupHighDetailMesh();

        }

        void SetupHighDetailMesh()
        {
            // We need to add mesh colliders with high detail vehicle's mesh.
            // These colliders will be used, among other things, when raycasting with weapons.
            // This is a problem because Unity does not support concave (non-convex) mesh colliders attached to rigid body.
            // Tried adding a separate kinematic rigid body (kinematic ones work with concave mesh colliders) to each object with a mesh filter, but without success.
            // So, we are left with the following options:
            // - somehow generate multiple convex meshes from a concave mesh
            // - create a separate game object with mesh colliders, and update his position/rotation every frame to be the same as vehicle's
            // Option with a separate game object is chosen.


            if (null == s_highDetailMeshesContainer)
            {
                s_highDetailMeshesContainer = new GameObject("Vehicle high detail meshes container");
            }

            GameObject parent = new GameObject(this.gameObject.name);
            this.HighDetailMeshesParent = parent.transform;
            parent.transform.parent = s_highDetailMeshesContainer.transform;
            parent.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);

            this.SetupDamagable();

            // for each mesh filter, create child game object with mesh collider

            foreach (var meshFilter in this.gameObject.GetComponentsInChildren<MeshFilter>())
            {
                GameObject child = new GameObject(meshFilter.gameObject.name, typeof(MeshCollider));
                child.layer = Vehicle.MeshLayer;
                child.transform.parent = parent.transform;
                child.transform.SetPositionAndRotation(meshFilter.transform.position, meshFilter.transform.rotation);

                var meshCollider = child.GetComponent<MeshCollider>();
                meshCollider.convex = false;
                meshCollider.sharedMesh = meshFilter.sharedMesh;

                if (null != meshFilter.gameObject.GetComponent<Rigidbody>()
                    || null != meshFilter.transform.parent.GetComponent<Rigidbody>()
                    || null != meshFilter.transform.parent.GetComponent<WheelCollider>())
                {
                    // this object has a dedicated rigid body or is a wheel, so it will move
                    // make sure that we update transform of this object
                    m_highDetailMeshObjectsToUpdate.Add(new KeyValuePair<Transform, Transform>(meshFilter.transform, child.transform));
                }

            }

            // add petrolcap

            /*
            if (this.PetrolcapTransform != null)
            {
                GameObject petrolcapGo = new GameObject(this.PetrolcapTransform.name, typeof(BoxCollider));
                petrolcapGo.layer = Vehicle.MeshLayer;
                petrolcapGo.transform.parent = parent.transform;
                petrolcapGo.transform.SetPositionAndRotation(this.PetrolcapTransform.position, this.PetrolcapTransform.rotation);

                var boxCollider = petrolcapGo.GetComponent<BoxCollider>();
                boxCollider.center = VehicleManager.Instance.petrolcapBoxColliderCenter;
                boxCollider.size = VehicleManager.Instance.petrolcapBoxColliderSize;

                this.PetrolcapUnderHighDetailMeshTransform = petrolcapGo.transform;
            }
            */

        }

        void SetupDoorsHingeJoints()
        {
            foreach (var pair in _frames.Where(x => x.Name.StartsWith("door_")))
            {
                var doorAlignment = GetDoorAlignment(pair.Name);

                if (doorAlignment == DoorAlignment.None) continue;

                var hinge = pair.gameObject.AddComponent<HingeJoint>();
                hinge.axis = Vector3.up;
                hinge.useLimits = true;

                var limit = 90.0f * ((doorAlignment == DoorAlignment.LeftFront || doorAlignment == DoorAlignment.LeftRear) ? 1.0f : -1.0f);
                hinge.limits = new JointLimits { min = Mathf.Min(0, limit), max = Mathf.Max(0, limit), };
                hinge.connectedBody = gameObject.GetComponent<Rigidbody>();
            }
        }

        private void SelectExtras()
        {
            int firstExtraIdx = -1;
            int secondExtraIdx = -1;

            VehicleDef.CompRulesUnion compsUnion = Definition.CompRules;
            if (compsUnion.HasExtraOne())
            {
                firstExtraIdx = ChooseComponent(Definition.CompRules.nExtraARule, compsUnion.nExtraAComp);
            }

            if (compsUnion.HasExtraTwo())
            {
                firstExtraIdx = ChooseComponent(compsUnion.nExtraBRule, compsUnion.nExtraBComp);
            }

            foreach (var extra in Extras)
            {
                Boolean isFirstExtra = extra.Name == ("extra" + firstExtraIdx.ToString());
                Boolean isSecondExtra = extra.Name == ("extra" + secondExtraIdx.ToString());

                extra.gameObject.SetActive(isFirstExtra || isSecondExtra);
            }
        }

        private int ChooseComponent(int rule, int comps)
        {
            VehicleDef.CompRulesUnion compsUnion = Definition.CompRules;

            if ((rule != 0) && IsValidCompRule(rule))
            {
                return ChooseComponentInternal(rule, comps);
            }
            else if (UnityEngine.Random.Range(0, 3) < 2)
            {
                int[] anVariations = new int[6];
                int numComps = GetListOfComponentsNotUsedByRules(Extras.Count, anVariations);
                if (numComps > 0)
                    return anVariations[UnityEngine.Random.Range(0, numComps)];
            }

            return -1;
        }

        private bool IsValidCompRule(int nRule)
        {
            //    TODO add weather checking, when weather is implemented.
            Boolean isRainingNow = false;

            return (nRule != (int)VehicleComponentsRules.ONLY_WHEN_RAINING)
                || isRainingNow
            ;
        }

        private int ChooseComponentInternal(int rule, int comps)
        {
            int component = -1;
            if (rule == (int)VehicleComponentsRules.ALLOW_ALWAYS || 
                rule == (int)VehicleComponentsRules.ONLY_WHEN_RAINING)
            {
                int iNumComps = CountCompsInRule(comps);
                int rand = UnityEngine.Random.Range(0, iNumComps);
                component = (comps >> (4 * rand)) & 0xF;
            }
            else if (rule == (int)VehicleComponentsRules.MAYBE_HIDE)
            {
                int iNumComps = CountCompsInRule(comps);
                int rand = UnityEngine.Random.Range(-1, iNumComps);
                if (rand != -1)
                    component = (comps >> (4 * rand)) & 0xF;
            }
            else if (rule == (int)VehicleComponentsRules.FULL_RANDOM)
            {
                component = UnityEngine.Random.Range(0, 5);
            }

            return component;
        }

        int CountCompsInRule(int comps)
        {
            int result = 0;
            while (comps != 0)
            {
                if ((comps & 0xF) != 0xF)
                    ++result;

                comps >>= 4;
            }

            return result;
        }

        int GetListOfComponentsNotUsedByRules(int numExtras, int[] outList)
        {
            int[] iCompsList = new int[]{ 0, 1, 2, 3, 4, 5 };

            VehicleDef.CompRulesUnion comps = Definition.CompRules;

            if (comps.nExtraARule != 0 && IsValidCompRule(comps.nExtraARule))
            {
                if (comps.nExtraARule == (int)VehicleComponentsRules.FULL_RANDOM)
                    return 0;

                if (comps.nExtraA_comp1 != 0xF)
                    iCompsList[comps.nExtraA_comp1] = 0xF;

                if (comps.nExtraA_comp2 != 0xF)
                    iCompsList[comps.nExtraA_comp2] = 0xF;

                if (comps.nExtraA_comp3 != 0xF)
                    iCompsList[comps.nExtraA_comp3] = 0xF;
            }

            if (comps.nExtraBRule != 0 && IsValidCompRule(comps.nExtraBRule))
            {
                if (comps.nExtraBRule == (int)VehicleComponentsRules.FULL_RANDOM)
                    return 0;

                if (comps.nExtraB_comp1 != 0xF)
                    iCompsList[comps.nExtraB_comp1] = 0xF;

                if (comps.nExtraB_comp2 != 0xF)
                    iCompsList[comps.nExtraB_comp2] = 0xF;

                if (comps.nExtraB_comp3 != 0xF)
                    iCompsList[comps.nExtraB_comp3] = 0xF;
            }

            int iNumComps = 0;
            for (int i = 0; i < numExtras; ++i)
            {
                if (iCompsList[i] == 0xF)
                    continue;

                outList[iNumComps] = i;
                ++iNumComps;
            }

            return iNumComps;
        }
    }
}