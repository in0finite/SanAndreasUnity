using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Importing.Conversion;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ModelTestDummy : MonoBehaviour
    {
        private string _loadedModelName;

        public string ModelName;
        public List<string> TextureDictionaries;

        private void Update()
        {
            if (Cell.GameData == null) return;

            if (_loadedModelName != ModelName) {
                _loadedModelName = ModelName;

                LoadModel(ModelName, TextureDictionaries.ToArray());
            }
        }

        private void LoadModel(string modelName, params string[] txds)
        {
            var mf = GetComponent<MeshFilter>();
            var mr = GetComponent<MeshRenderer>();

            var geoms = Geometry.Load(modelName, txds);

            mf.sharedMesh = geoms.Geometry[0].Mesh;
            mr.sharedMaterials = geoms.Geometry[0].GetMaterials(MaterialFlags.Default);
        }
    }
}
