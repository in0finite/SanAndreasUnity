using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    public static class TransformEx
    {
        public static Transform FindChildRecursive(this Transform transform, string strName)
        {
            if (transform.name.Equals(strName, System.StringComparison.InvariantCultureIgnoreCase)) return transform;

            for (int i = 0; i < transform.childCount; i++)
            {
                var tran = transform.GetChild(i).FindChildRecursive(strName);
                if (tran) return tran;
            }

            return null;
        }

        //
        // Get all of the children in this transform, recursively
        //
        public static List<Transform> GetAllChildren(this Transform transform)
        {
            var list = new List<Transform>();
            transform.AddAllChildren(list);
            return list;
        }

        //
        // Add all of the children in this transform to this list
        //
        public static void AddAllChildren(this Transform transform, List<Transform> list)
        {
            list.Add(transform);

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).AddAllChildren(list);
            }
        }

        //
        // Return all children with the specified tag
        //
        public static Transform[] GetChildrenWithTag(this Transform transform, string strTag)
        {
            var children = GetAllChildren(transform);
            return children.Where(x => x.CompareTag(strTag)).ToArray();
        }

        //
        // Set local position and rotation to 0
        //
        public static void Identity(this GameObject go)
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
        }

        //
        // Create an empty child
        //
        public static GameObject CreateChild(this GameObject go)
        {
            var child = new GameObject();
            child.transform.parent = go.transform;
            child.Identity();

            return child;
        }

        //
        // Create an empty child
        //
        public static GameObject CreateChild(this GameObject go, string name)
        {
            var child = new GameObject(name);
            child.transform.parent = go.transform;
            child.Identity();

            return child;
        }

        //
        // Create a prefab as a child of this game object
        //
        public static GameObject InstantiateChild(this GameObject go, GameObject prefab)
        {
            var child = GameObject.Instantiate(prefab) as GameObject;
            child.transform.parent = go.transform;
            child.Identity();

            return child;
        }

        //
        // Create a prefab as a child of this game object
        //
        public static GameObject InstantiateChild(this GameObject go, GameObject prefab, string name)
        {
            var child = InstantiateChild(go, prefab);
            child.name = name;

            return child;
        }

        //
        // Change the layer of every object on this mother fucker
        //
        public static void SetLayerRecursive(this GameObject go, int Layer)
        {
            go.layer = Layer;

            for (int i = 0; i < go.transform.childCount; i++)
            {
                go.transform.GetChild(i).gameObject.SetLayerRecursive(Layer);
            }
        }

        //
        // Change the layer of every object on this mother fucker
        //
        public static void SetLayerRecursive(this GameObject go, string layer)
        {
            var layerint = LayerMask.NameToLayer(layer);
            if (layerint == 0)
            {
                Debug.LogWarning("SetLayerRecursive: couldn't find layer: " + layer);
                return;
            }

            go.SetLayerRecursive(layerint);
        }

        //
        // Invoke in x seconds, but only if we're not already invoking
        //
        public static void InvokeAtomic(this MonoBehaviour mb, string strName, float fDelay)
        {
            UnityEngine.Profiling.Profiler.BeginSample("InvokeAtomic");

            if (!mb.IsInvoking(strName))
            {
                mb.Invoke(strName, fDelay);
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        //
        // Returns a Bounds object of this object and all of its children's renderers (apart from particle systems)
        //
        public static Bounds WorkoutRenderBounds(this Transform tx)
        {
            Bounds b = new Bounds(Vector3.zero, Vector3.zero);

            var renderers = tx.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                //if (r is ParticleRenderer) continue;  // no longer available in newer versions of Unity
                if (r is ParticleSystemRenderer) continue;

                if (b.center == Vector3.zero)
                    b = r.bounds;
                else
                    b.Encapsulate(r.bounds);
            }

            return b;
        }
    }
}