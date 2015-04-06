using System.Collections.Generic;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Conversion;
using UnityEngine;
using System.Linq;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.Vehicle;

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

        public static Vehicle Create(VehicleSpawner spawner)
        {
            // TODO: Random cars
            if (spawner.Info.CarId == -1) return null;

            var inst = new GameObject().AddComponent<Vehicle>();

            inst.Initialize(spawner);

            return inst;
        }

        private int _wheelFrameIndex;

        private List<Transform> _children = new List<Transform>();
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

        private readonly Dictionary<Transform, string> _namedFrames = new Dictionary<Transform, string>();
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
                    return WheelAlignment.RightBack;
                case "wheel_lb_dummy":
                    return WheelAlignment.LeftBack;
                default:
                    return WheelAlignment.None;
            }
        }

        public Transform GetPart(string name)
        {
            return _namedFrames.First(x => x.Value == name).Key;
        }

        private GameObject AddPart(Geometry.GeometryFrame frame, Transform parent)
        {
            var child = new GameObject();
            child.name = frame.Name;

            child.transform.SetParent(parent, false);

            child.transform.localPosition = frame.Position;
            child.transform.localRotation = frame.Rotation;

            _children.Add(child.transform);
            _namedFrames.Add(child.transform, frame.Name);

            if (frame.GeometryIndex != -1)
            {
                var mf = child.AddComponent<MeshFilter>();
                var mr = child.AddComponent<MeshRenderer>();

                var geometry = _geometryParts.Geometry[frame.GeometryIndex];

                mf.sharedMesh = geometry.Mesh;
                mr.sharedMaterials = geometry.GetMaterials(MaterialFlags.Vehicle);

                // filter these out for now
                if (frame.Name.EndsWith("_vlo") ||
                    frame.Name.EndsWith("_dam"))
                {
                    child.SetActive(false);
                }
            }

            return child;
        }

        private void Initialize(VehicleSpawner spawner)
        {
            transform.position = spawner.transform.position + Vector3.up * 1f;
            transform.localRotation = spawner.transform.localRotation;

            Definition = Cell.GameData.GetDefinition<VehicleDef>(spawner.Info.CarId);

            name = Definition.GameName;

            _geometryParts = Geometry.Load(Definition.ModelName,
                TextureDictionary.Load(Definition.TextureDictionaryName),
                TextureDictionary.Load("vehicle"),
                TextureDictionary.Load("misc"));

            for (int i = 0; i < _geometryParts.Frames.Length; ++i)
            {
                var frame = _geometryParts.Frames[i];

                if (frame.Name == "wheel")
                {
                    _wheelFrameIndex = i;
                }

                Transform parent;
                var parentIndex = frame.ParentIndex;

                if (parentIndex < 0)
                {
                    parent = transform;
                }
                else
                {
                    parent = _children[parentIndex];
                }

                AddPart(frame, parent);
            }

            for (int i = 0; i < _geometryParts.Frames.Length; ++i)
            {
                var frame = _geometryParts.Frames[i];

                if (frame.Name.StartsWith("wheel_"))
                {
                    var wheelAlignment = GetWheelAlignment(frame.Name);

                    if (wheelAlignment != WheelAlignment.RightFront)
                    {
                        var child = AddPart(_geometryParts.Frames[_wheelFrameIndex], _children[i]);

                        _wheels.Add(new Wheel
                        {
                            Alignment = wheelAlignment,
                            Parent = _children[i],
                            Child = child.transform,
                        });
                    }
                    else
                    {
                        _wheels.Add(new Wheel
                        {
                            Alignment = wheelAlignment,
                            Parent = _children[i],
                            Child = _children[_wheelFrameIndex],
                        });
                    }

                    if (wheelAlignment == WheelAlignment.LeftFront ||
                        wheelAlignment == WheelAlignment.LeftMid ||
                        wheelAlignment == WheelAlignment.LeftBack)
                    {
                        _children[i].Rotate(Vector3.up, 180.0f);
                    }
                }
            }

            InitializePhysics();
        }
    }
}
