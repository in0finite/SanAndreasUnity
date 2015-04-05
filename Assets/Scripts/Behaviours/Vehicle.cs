using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items.Definitions;
using UnityEngine;
using VehicleDef = SanAndreasUnity.Importing.Items.Definitions.Vehicle;

namespace SanAndreasUnity.Behaviours
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

        private WheelAlignment GetWheelAlignment(string frameName)
        {
            switch (frameName)
            {
                case "wheel_rf_dummy" :
                    return WheelAlignment.RightFront;
                case "wheel_lf_dummy":
                    return WheelAlignment.LeftFront;
                case "wheel_rb_dummy":
                    return WheelAlignment.RightBack;
                case "wheel_lb_dummy":
                    return WheelAlignment.LeftBack;
                default :
                    return WheelAlignment.None;
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

        private void AddPart(Geometry.GeometryFrame frame, Transform parent)
        {
            var child = new GameObject();
            child.name = frame.Name;

            child.transform.SetParent(parent, false);

            child.transform.localPosition = frame.Position;
            child.transform.localRotation = frame.Rotation;

            _children.Add(child.transform);

            if (frame.GeometryIndex < 0)
            {
                return;
            }

            var mf = child.AddComponent<MeshFilter>();
            var mr = child.AddComponent<MeshRenderer>();

            var geometry = _geometryParts.Geometry[frame.GeometryIndex];

            mf.sharedMesh = geometry.Mesh;
            mr.sharedMaterials = geometry.GetMaterials();

            // filter these out for now
            if (frame.Name.EndsWith("_vlo") ||
                frame.Name.EndsWith("_dam"))
            {
                child.SetActive(false);
            }
        }

        private void Initialize(VehicleSpawner spawner)
        {
            transform.position = spawner.transform.position;
            transform.localRotation = spawner.transform.localRotation;

            var def = Cell.GameData.GetDefinition<VehicleDef>(spawner.Info.CarId);

            name = def.GameName;

            _geometryParts = Geometry.Load(def.ModelName, def.TextureDictionaryName);

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
                        AddPart(_geometryParts.Frames[_wheelFrameIndex], _children[i]);
                    }
                }
            }
        }
    }
}
