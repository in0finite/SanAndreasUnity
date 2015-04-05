using System.Collections.Generic;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Conversion;
using UnityEngine;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.Vehicle;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class Vehicle : MonoBehaviour
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

        private int _wheelFrameIndex;

        private List<Transform> _children = new List<Transform>();
        private Geometry.GeometryParts _geometryParts;

        private List<GameObject> _wheels = new List<GameObject>();

        private WheelAlignment GetWheelAlignment(string frameName)
        {
            switch (frameName)
            {
                case "wheel_rf_dummy" :
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
                default :
                    return WheelAlignment.None;
            }
        }

        private void Update()
        {
            foreach (var wheel in _wheels)
            {
                wheel.transform.Rotate(Vector3.left, Time.deltaTime * 500.0f);
            }
        }

        public static Vehicle Create(VehicleSpawner spawner)
        {
            // TODO: Random cars
            if (spawner.Info.CarId == -1) return null;

            var inst = new GameObject().AddComponent<Vehicle>();

            inst.Initialize(spawner);

            return inst;
        }

        private GameObject AddPart(Geometry.GeometryFrame frame, Transform parent)
        {
            var child = new GameObject();
            child.name = frame.Name;

            child.transform.SetParent(parent, false);

            child.transform.localPosition = frame.Position;
            child.transform.localRotation = frame.Rotation;

            _children.Add(child.transform);

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

            var def = Cell.GameData.GetDefinition<VehicleDef>(spawner.Info.CarId);

            name = def.GameName;

            _geometryParts = Geometry.Load(def.ModelName, 
                TextureDictionary.Load(def.TextureDictionaryName),
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

                        _wheels.Add(child);
                    }
                    else
                    {
                        _wheels.Add(_children[_wheelFrameIndex].gameObject);
                    }

                    if (wheelAlignment == WheelAlignment.LeftFront ||
                        wheelAlignment == WheelAlignment.LeftMid ||
                        wheelAlignment == WheelAlignment.LeftBack)
                    {
                        _children[i].Rotate(Vector3.up, 180.0f);
                    }
                }
            }

            _geometryParts.AttachCollisionModel(transform, true);

            gameObject.AddComponent<Rigidbody>();
        }
    }
}
