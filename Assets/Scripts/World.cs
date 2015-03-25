using System.Collections;
using UnityEngine;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Conversion;
using System.Diagnostics;
using System;
using System.IO;
using System.Collections.Generic;

namespace SanAndreasUnity
{
    public class World : MonoBehaviour
    {
        void Start()
        {
            ResourceManager.LoadArchive(ResourceManager.GetPath("models", "gta3.img"));
            //ResourceManager.LoadArchive(ResourceManager.GetPath("models", "gta_int.img"));
            //ResourceManager.LoadArchive(ResourceManager.GetPath("models", "player.img"));

            StartCoroutine(Load());
        }

        private class ToLoad : IComparable<ToLoad>
        {
            public readonly Instance Instance;
            public readonly Transform Parent;
            public readonly Importing.Items.Object Object;
            public readonly float Distance;

            public ToLoad(Instance inst, Transform parent, Importing.Items.Object obj, float dist)
            {
                Instance = inst;
                Parent = parent;
                Object = obj;
                Distance = dist;
            }

            public int CompareTo(ToLoad other)
            {
                return Math.Sign(Distance - other.Distance);
            }
        }

        private IEnumerator Load()
        {
            var data = new GameData(ResourceManager.GetPath("data", "gta.dat"));
            
            var toLoad = new List<ToLoad>();

            foreach (var group in data.GetGroups()) {
                var parent = new GameObject(group.Substring(group.IndexOf('/') + 1));

                parent.transform.SetParent(transform);

                foreach (var raw in data.GetInstances(group)) {
                    var inst = raw;
                    var dist = Vector3.Distance(inst.Position, Camera.main.transform.position);
                    
                    while (true) {
                        var obj = data.GetObject(inst.ObjectId);
                        if (obj == null) {
                            UnityEngine.Debug.LogFormat("Can't find: {0}", inst.ObjectId);
                            break;
                        }

                        if (obj.DrawDist > 0 && dist > obj.DrawDist) {
                            if (inst.LodInstance != null) {
                                inst = inst.LodInstance;
                                continue;
                            }

                            if ((obj.Flags & ObjectFlag.DisableDrawDist) != ObjectFlag.DisableDrawDist) break;
                        }

                        toLoad.Add(new ToLoad(inst, parent.transform, obj, dist));
                        break;
                    }
                }
            }

            toLoad.Sort();

            var timer = new Stopwatch();
            timer.Start();

            while (toLoad.Count > 0) {
                var next = toLoad[0];
                toLoad.RemoveAt(0);

                var gobj = new GameObject(next.Object.Geometry);

                gobj.transform.SetParent(next.Parent);
                gobj.transform.position = next.Instance.Position;
                gobj.transform.rotation = next.Instance.Rotation;
                gobj.isStatic = true;

                var mf = gobj.AddComponent<MeshFilter>();
                var mr = gobj.AddComponent<MeshRenderer>();

                Mesh mesh;
                Material[] materials;

                try {
                    Geometry.Load(next.Object.Geometry, next.Object.TextureDictionary,
                        next.Object.Flags, out mesh, out materials);
                } catch (NotImplementedException) {
                    Destroy(gobj);
                    continue;
                } catch (FileNotFoundException) {
                    Destroy(gobj);
                    continue;
                }

                mf.mesh = mesh;
                mr.materials = materials;

                if (timer.Elapsed.TotalSeconds > 1d / 60d) {
                    timer.Reset();
                    yield return null;
                    timer.Start();
                }
            }
        }
    }
}
