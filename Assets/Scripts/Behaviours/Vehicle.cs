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

            var geoms = Geometry.Load(def.ModelName, def.TextureDictionaryName);

            var i = 0;
            foreach (var geom in geoms) {
                var child = new GameObject();

                child.name = string.Format("Part {0}", i);
                child.transform.SetParent(transform, false);

                var mf = child.AddComponent<MeshFilter>();
                var mr = child.AddComponent<MeshRenderer>();

                mf.sharedMesh = geom.Mesh;
                mr.sharedMaterials = geom.GetMaterials();
            }
        }
    }
}
