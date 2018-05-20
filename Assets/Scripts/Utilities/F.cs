using SanAndreasUnity.Behaviours.Vehicles;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    //Static class with extra functions
    public static class F
    {
        //Returns the number with the greatest absolute value
        public static float MaxAbs(params float[] nums)
        {
            float result = 0;

            for (int i = 0; i < nums.Length; i++)
            {
                if (Mathf.Abs(nums[i]) > Mathf.Abs(result))
                {
                    result = nums[i];
                }
            }

            return result;
        }

        //Returns the topmost parent with a certain component
        public static Component GetTopmostParentComponent<T>(Transform tr) where T : Component
        {
            Component getting = null;

            while (tr.parent != null)
            {
                if (tr.parent.GetComponent<T>() != null)
                {
                    getting = tr.parent.GetComponent<T>();
                }

                tr = tr.parent;
            }

            return getting;
        }

        // WIP: This causes Unity to crash
        /*public static void OptimizeVehicle(this Vehicle v)
        {
            foreach (var col in v.gameObject.GetComponentsInChildren<Collider>())
            {
                if (!(col is MeshCollider))
                    Object.Destroy(col);
            }

            foreach (var go in v.gameObject.GetComponentsInChildren<MeshFilter>())
                go.gameObject.AddComponent<MeshCollider>();
        }*/

        public static void OptimizeVehicle(this Vehicle v)
        {
            var cols = v.gameObject.GetComponentsInChildren<Collider>().Where(x => x.GetType() != typeof(MeshCollider));
            foreach (var col in cols)
                col.enabled = false;

            var filters = v.gameObject.GetComponentsInChildren<MeshFilter>().Where(x => x.sharedMesh != null);
            foreach (var filter in filters)
                filter.gameObject.AddComponent<MeshCollider>();
        }

        public static Mesh GetSharedMesh(this Collider col)
        {
            if (col is MeshCollider)
            {
                return ((MeshCollider)col).sharedMesh;
            }
            else
            {
                // WIP: Depending on the collider generate a diferent shape
                MeshFilter f = col.gameObject.GetComponent<MeshFilter>();
                return f != null ? f.sharedMesh : null;
            }
        }

        public static bool BetweenInclusive(this float v, float min, float max)
        {
            return v >= min && v <= max;
        }

        public static bool BetweenExclusive(this float v, float min, float max)
        {
            return v > min && v < max;
        }
    }
}