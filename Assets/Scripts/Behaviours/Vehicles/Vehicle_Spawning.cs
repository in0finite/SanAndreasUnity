using System.Collections.Generic;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Vehicles;
using UnityEngine;
using System.Linq;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.VehicleDef;
using System;

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

        public enum DoorAlignment
        {
            None,
            RightFront,
            LeftFront,
            RightRear,
            LeftRear,
        }

        public enum SeatAlignment
        {
            None = -1,
            FrontRight = 0,
            FrontLeft = 1,
            BackRight = 2,
            BackLeft = 3,
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
            var inst = new GameObject().AddComponent<Vehicle>();

            VehicleDef def;
            if (spawner.Info.CarId == -1)
            {
                def = GetRandomDef();
            }
            else
            {
                def = Item.GetDefinition<VehicleDef>(spawner.Info.CarId);
            }

            inst.Initialize(def, spawner.transform.position, spawner.transform.rotation, spawner.Info.Colors);

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

        private FrameContainer _frames;
        private readonly List<Wheel> _wheels = new List<Wheel>();

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

        private void Initialize(VehicleDef def, Vector3 position, Quaternion rotation, int[] colors = null)
        {
            Definition = def;

            if (colors != null && colors[0] != -1)
            {
                SetColors(CarColors.FromIndices(colors.TakeWhile(x => x != -1).ToArray()));
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

            var wheel = wheelFrame.transform;

            foreach (var frame in _frames)
            {
                if (!frame.Name.StartsWith("wheel_") || wheel == null) continue;

                var wheelAlignment = GetWheelAlignment(frame.Name);

                Wheel inst;

                if (wheelAlignment != WheelAlignment.RightFront)
                {
                    var copy = Instantiate(wheel);
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
                    _wheels.Add(inst = new Wheel
                    {
                        Alignment = wheelAlignment,
                        Parent = frame.transform,
                        Child = wheel,
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

            SeatTransforms = new Transform[backSeat != null ? 4 : 2];

            if (frontSeat != null)
            {
                var frontSeatMirror = new GameObject("ped_frontseat").transform;
                frontSeatMirror.SetParent(frontSeat.parent, false);
                frontSeatMirror.localPosition = Vector3.Scale(frontSeat.localPosition, new Vector3(-1f, 1f, 1f));

                if (frontSeat.localPosition.x > 0f)
                {
                    SeatTransforms[(int)SeatAlignment.FrontRight] = frontSeat;
                    SeatTransforms[(int)SeatAlignment.FrontLeft] = frontSeatMirror;
                }
                else
                {
                    SeatTransforms[(int)SeatAlignment.FrontRight] = frontSeatMirror;
                    SeatTransforms[(int)SeatAlignment.FrontLeft] = frontSeat;
                }

                DriverTransform = SeatTransforms[(int)SeatAlignment.FrontLeft];
            }

            if (backSeat != null)
            {
                var backSeatMirror = new GameObject("ped_backseat").transform;
                backSeatMirror.SetParent(backSeat.parent, false);
                backSeatMirror.localPosition = Vector3.Scale(backSeat.localPosition, new Vector3(-1f, 1f, 1f));

                if (backSeat.localPosition.x > 0f)
                {
                    SeatTransforms[(int)SeatAlignment.BackRight] = backSeat;
                    SeatTransforms[(int)SeatAlignment.BackLeft] = backSeatMirror;
                }
                else
                {
                    SeatTransforms[(int)SeatAlignment.BackRight] = backSeatMirror;
                    SeatTransforms[(int)SeatAlignment.BackLeft] = backSeat;
                }
            }

            transform.position = position - Vector3.up * _wheels.Average(x => x.Child.position.y);
            transform.localRotation = rotation;

            gameObject.SetLayerRecursive(Layer);
        }
    }
}
