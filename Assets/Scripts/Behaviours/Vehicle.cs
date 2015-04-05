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
        public static Vehicle Create(VehicleSpawner spawner)
        {
            // TODO: Random cars
            if (spawner.Info.CarId == -1) return null;

            var inst = new GameObject().AddComponent<Vehicle>();

            inst.Initialize(spawner);

            return inst;
        }

        private void Initialize(VehicleSpawner spawner)
        {
            transform.position = spawner.transform.position;
            transform.localRotation = spawner.transform.localRotation;

            var def = Cell.GameData.GetDefinition<VehicleDef>(spawner.Info.CarId);

            name = def.GameName;

            var parts = Geometry.Load(def.ModelName, def.TextureDictionaryName);

            var children = new Transform[parts.Frames.Length];

            for (int i = 0; i < parts.Frames.Length; ++i)
            {
                var frame = parts.Frames[i];

                var child = new GameObject();

                child.name = string.Format("Frame {0}, {1}", i, frame.Name);

                var parentIndex = frame.ParentIndex;

                if (parentIndex < 0)
                {
                    child.transform.SetParent(transform, false);
                }
                else
                {
                    child.transform.SetParent(children[parentIndex], false);
                }

                child.transform.localPosition = frame.Position;
                children[i] = child.transform;

                if (frame.GeometryIndex < 0)
                {
                    continue;
                }

                // filter these out for now
                if (frame.Name.EndsWith("_vlo")) continue;
                if (frame.Name.EndsWith("_dam")) continue;

                var mf = child.AddComponent<MeshFilter>();
                var mr = child.AddComponent<MeshRenderer>();

                var geom = parts.Geometry[frame.GeometryIndex];

                mf.sharedMesh = geom.Mesh;
                mr.sharedMaterials = geom.GetMaterials();
            }
        }
    }
}
