using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Collision;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SanAndreasUnity.Importing.Conversion
{
    public class CollisionModel
    {
        private static UnityEngine.Vector3 Convert(Vector3 vec)
        {
            return new UnityEngine.Vector3(vec.X, vec.Z, vec.Y);
        }

        private static Mesh Convert(IEnumerable<Face> faces, IEnumerable<Vertex> vertices)
        {
            var mesh = new Mesh {
                vertices = vertices.Select(x => Convert(x.Position)).ToArray(),
                subMeshCount = 1
            };

            var indices = faces.SelectMany(x => x.GetIndices()).ToArray();

            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            return mesh;
        }

        private static Mesh Convert(FaceGroup group, IEnumerable<Face> faces, IEnumerable<Vertex> vertices)
        {
            return Convert(faces.Skip(group.StartFace).Take(1 + group.EndFace - group.StartFace), vertices);
        }

        private static GameObject _sTemplateParent;

        private static readonly Dictionary<string, CollisionModel> _sLoaded
            = new Dictionary<string, CollisionModel>();

        public static void Load(string name, Transform destParent)
        {
            CollisionModel col;

            if (_sLoaded.ContainsKey(name)) {
                col = _sLoaded[name];
                if (col == null) return;

                col.Spawn(destParent);
                return;
            }

            var file = CollisionFile.FromName(name);
            if (file == null || (file.Flags & Flags.NotEmpty) != Flags.NotEmpty) {
                _sLoaded.Add(name, null);
                return;
            }

            col = new CollisionModel(file);
            _sLoaded.Add(name, col);

            col.Spawn(destParent);
        }

        private readonly GameObject _template;

        private void Add<TCollider>(Action<TCollider> setup)
            where TCollider : Collider
        {
            var type = typeof (TCollider);
            var obj = new GameObject(type.Name, type);
            obj.transform.SetParent(_template.transform);

            setup(obj.GetComponent<TCollider>());
        }

        private CollisionModel(CollisionFile file)
        {
            if (_sTemplateParent == null) {
                _sTemplateParent = new GameObject("Collision Templates");
                _sTemplateParent.SetActive(false);
            }

            _template = new GameObject(file.Name);
            _template.transform.SetParent(_sTemplateParent.transform);

            foreach (var box in file.Boxes) {
                Add<BoxCollider>(x => {
                    var min = Convert(box.Min);
                    var max = Convert(box.Max);

                    x.center = (min + max) * .5f;
                    x.size = (max - min);
                });
            }

            foreach (var sphere in file.Spheres) {
                Add<SphereCollider>(x => {
                    x.center = Convert(sphere.Center);
                    x.radius = sphere.Radius;
                });
            }

            if (file.FaceGroups.Length > 0) {
                foreach (var group in file.FaceGroups) {
                    Add<MeshCollider>(x => {
                        x.sharedMesh = Convert(group, file.Faces, file.Vertices);
                    });
                }
            } else if (file.Faces.Length > 0) {
                Add<MeshCollider>(x => {
                    x.sharedMesh = Convert(file.Faces, file.Vertices);
                });
            }

            // TODO: MeshCollider
        }

        public void Spawn(Transform destParent)
        {
            var clone = Object.Instantiate(_template.gameObject);

            clone.name = "Collision";
            clone.transform.SetParent(destParent, false);
        }
    }
}
