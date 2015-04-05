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

        public static Vehicle Create(VehicleSpawner spawner)
        {
            // TODO: Random cars
            if (spawner.Info.CarId == -1) return null;

            var inst = new GameObject().AddComponent<Vehicle>();

            inst.Initialize(spawner);

            return inst;
        }

        private void AddPart(Geometry.GeometryFrame frame)
        {
            var child = new GameObject();
            child.name = frame.Name;

            var parentIndex = frame.ParentIndex;

            if (parentIndex < 0)
            {
                child.transform.SetParent(transform, false);
            }
            else
            {
                child.transform.SetParent(_children[parentIndex], false);
            }

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

                AddPart(frame);
            }
        }
    }
}
