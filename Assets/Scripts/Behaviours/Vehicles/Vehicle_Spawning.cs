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

        public static Vehicle Create(VehicleSpawner spawner)
        {
            //Debug.Log("-111");
            return Create(spawner.Info.CarId, spawner.Info.Colors, spawner.transform.position,
                spawner.transform.rotation);
        }

        public static Vehicle Create(int carId, int[] colors, Vector3 position, Quaternion rotation)
        {
            //Debug.Log("-000");
            var inst = new GameObject().AddComponent<Vehicle>();

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

#if CLIENT
            if (Networking.Server.Instance != null)
            {
                Networking.Server.Instance.GlobalGroup.Add(inst);
            }
#endif

            OutOfRangeDestroyer destroyer = inst.gameObject.AddComponent<OutOfRangeDestroyer>();
            destroyer.timeUntilDestroyed = 5;
            destroyer.range = 300;

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
            public SeatAlignment Alignment { get; set; }

            public Transform Parent { get; set; }

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

        private readonly List<Wheel> _wheels = new List<Wheel>();
        private readonly List<Seat> _seats = new List<Seat>();

        public List<Wheel> Wheels { get { return _wheels; } }
        public List<Seat> Seats { get { return _seats; } }

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

            _geometryParts = Geometry.Load(Definition.ModelName,
                TextureDictionary.Load(Definition.TextureDictionaryName),
                TextureDictionary.Load("vehicle"),
                TextureDictionary.Load("misc"));

            _frames = _geometryParts.AttachFrames(transform, MaterialFlags.Vehicle);

            var wheelFrame = _frames.FirstOrDefault(x => x.Name == "wheel");

            if (wheelFrame == null)
            {
                Debug.LogWarningFormat("No wheels defined for {0}!", def.GameName);
                Destroy(gameObject);
                return;
            }

            foreach (var frame in _frames)
            {
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

            InitializePhysics();

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

            gameObject.SetLayerRecursive(Layer);
        }
    }
}