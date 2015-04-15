using System.Collections.Generic;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Vehicles;
using UnityEngine;
using System.Linq;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.VehicleDef;
using SanAndreasUnity.Importing.Items;
using System;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public partial class Vehicle
    {
        public enum WheelAlignment
        {
            None,
            RightFront,
            LeftFront,
            RightMid,
            LeftMid,
            RightBack,
            LeftBack,
        }

        public enum DoorAlignment
        {
            None,
            RightFront,
            LeftFront,
            RightRear,
            LeftRear,
        }

        public static Vehicle Create(VehicleSpawner spawner)
        {
            // TODO: Random cars
            if (spawner.Info.CarId == -1) return null;

            var inst = new GameObject().AddComponent<Vehicle>();

            inst.Initialize(spawner);

            return inst;
        }

        private Geometry.GeometryParts _geometryParts;

        public class Wheel
        {
            public WheelAlignment Alignment
            {
                get { return _alignment; }
                set
                {
                    _alignment = value;

                    IsLeftHand = (value == WheelAlignment.LeftFront ||
                        value == WheelAlignment.LeftMid ||
                        value == WheelAlignment.LeftBack);
                }
            }

            public bool IsLeftHand { get; private set; }

            public Transform Parent { get; set; }
            public Transform Child { get; set; }
            public WheelCollider Collider { get; set; }

            public Quaternion Roll { get; set; }

            private WheelAlignment _alignment;
        }

        private FrameContainer _frames;
        private readonly List<Wheel> _wheels = new List<Wheel>();

        private WheelAlignment GetWheelAlignment(string frameName)
        {
            switch (frameName) {
                case "wheel_rf_dummy":
                    return WheelAlignment.RightFront;
                case "wheel_lf_dummy":
                    return WheelAlignment.LeftFront;
                case "wheel_rm_dummy":
                    return WheelAlignment.RightMid;
                case "wheel_lm_dummy":
                    return WheelAlignment.LeftMid;
                case "wheel_rb_dummy":
                    return WheelAlignment.RightBack;
                case "wheel_lb_dummy":
                    return WheelAlignment.LeftBack;
                default:
                    return WheelAlignment.None;
            }
        }

        private DoorAlignment GetDoorAlignment(string frameName)
        {
            switch (frameName) {
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
            return _frames.GetByName(name).transform;
        }

        private void Initialize(VehicleSpawner spawner)
        {
            transform.position = spawner.transform.position + Vector3.up * 1f;
            transform.localRotation = spawner.transform.localRotation;

            Definition = Item.GetDefinition<VehicleDef>(spawner.Info.CarId);

            var clrs = CarColors.Get(Definition.ModelName);
            SetColors(clrs[UnityEngine.Random.Range(0, clrs.Count)]);

            name = Definition.GameName;

            _geometryParts = Geometry.Load(Definition.ModelName,
                TextureDictionary.Load(Definition.TextureDictionaryName),
                TextureDictionary.Load("vehicle"),
                TextureDictionary.Load("misc"));

            _frames = _geometryParts.AttachFrames(transform, MaterialFlags.Vehicle);
            var wheel = _frames.FirstOrDefault(x => x.Name == "wheel").transform;

            foreach (var frame in _frames) {
                if (!frame.Name.StartsWith("wheel_") || wheel == null) continue;

                var wheelAlignment = GetWheelAlignment(frame.Name);

                if (wheelAlignment != WheelAlignment.RightFront) {
                    var copy = Instantiate(wheel);
                    copy.SetParent(frame.transform, false);

                    _wheels.Add(new Wheel {
                        Alignment = wheelAlignment,
                        Parent = frame.transform,
                        Child = copy,
                    });
                } else {
                    _wheels.Add(new Wheel {
                        Alignment = wheelAlignment,
                        Parent = frame.transform,
                        Child = wheel,
                    });
                }

                if (wheelAlignment == WheelAlignment.LeftFront ||
                    wheelAlignment == WheelAlignment.LeftMid ||
                    wheelAlignment == WheelAlignment.LeftBack) {
                    frame.transform.Rotate(Vector3.up, 180.0f);
                }
            }

            InitializePhysics();

            foreach (var pair in _frames.Where(x => x.Name.StartsWith("door_"))) {
                var doorAlignment = GetDoorAlignment(pair.Name);

                if (doorAlignment != DoorAlignment.None) {
                    var hinge = pair.gameObject.AddComponent<HingeJoint>();
                    hinge.axis = Vector3.up;
                    hinge.useLimits = true;

                    float limit = 90.0f * ((doorAlignment == DoorAlignment.LeftFront || doorAlignment == DoorAlignment.LeftRear) ? 1.0f : -1.0f);
                    hinge.limits = new JointLimits { min = Mathf.Min(0, limit), max = Mathf.Max(0, limit), };

                    hinge.connectedBody = gameObject.GetComponent<Rigidbody>();
                }
            }
        }
    }
}
