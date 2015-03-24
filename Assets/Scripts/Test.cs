using SanAndreasUnity.Importing.Items;
using UnityEngine;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Conversion;

namespace SanAndreasUnity
{
    public class Test : MonoBehaviour
    {
        void Start()
        {
            ResourceManager.LoadArchive(ResourceManager.GetPath(ResourceManager.ModelsDir, "gta3.img"));

            var data = new GameData(ResourceManager.GetPath("data", "gta.dat"));

            foreach (var inst in data.GetInstances("la")) {
                var obj = data.GetObject(inst.ObjectId);

                if (obj == null) {
                    Debug.LogFormat("Can't find: {0}", inst.ObjectId);
                    continue;
                }

                var gobj = new GameObject(obj.Geometry);

                gobj.transform.SetParent(transform);
                gobj.transform.position = inst.Position;
                gobj.transform.rotation = inst.Rotation;
                gobj.isStatic = true;

                var mf = gobj.AddComponent<MeshFilter>();
                var mr = gobj.AddComponent<MeshRenderer>();

                Mesh mesh;
                Material[] materials;

                try {
                    Geometry.Load(obj.Geometry, obj.TextureDictionary, out mesh, out materials);
                } catch {
                    Destroy(gobj);
                    continue;
                }

                mf.mesh = mesh;
                mr.materials = materials;
            }
        }
    }
}
