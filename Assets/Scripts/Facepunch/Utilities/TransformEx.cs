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

        public static void SetLayerRecursive(this GameObject go, int Layer)
        {
            go.layer = Layer;

            for (int i = 0; i < go.transform.childCount; i++)
            {
                go.transform.GetChild(i).gameObject.SetLayerRecursive(Layer);
            }
        }

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
    }
}