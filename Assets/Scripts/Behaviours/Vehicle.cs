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

            Mesh mesh;
            Material[] materials;

            Geometry.Load(def.ModelName, def.TextureDictionaryName, out mesh, out materials);

            var mf = gameObject.AddComponent<MeshFilter>();
            var mr = gameObject.AddComponent<MeshRenderer>();

            mf.sharedMesh = mesh;
            mr.sharedMaterials = materials;
        }
    }
}
