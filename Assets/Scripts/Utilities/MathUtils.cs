using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public static class MathUtils
    {
        public static float DistanceFromPointToLineSegment(Vector3 p, Vector3 v, Vector3 w)
        {
            // Return minimum distance between line segment vw and point p
            float l2 = Vector3.SqrMagnitude(v - w);  // i.e. |w-v|^2 -  avoid a sqrt
            if (l2 == 0.0f) return Vector3.Distance(p, v);   // v == w case
                                                    // Consider the line extending the segment, parameterized as v + t (w - v).
                                                    // We find projection of point p onto the line. 
                                                    // It falls where t = [(p-v) . (w-v)] / |w-v|^2
                                                    // We clamp t from [0,1] to handle points outside the segment vw.
            float t = Mathf.Max(0, Mathf.Min(1, Vector3.Dot(p - v, w - v) / l2));
            Vector3 projection = v + t * (w - v);  // Projection falls on the segment
            return Vector3.Distance(p, projection);
        }

        public static Vector3 MinComponents(Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }

        public static Vector3 MaxComponents(Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        public static Vector3 NormalizedOrZero(this Vector3 vec)
        {
            if (vec == Vector3.zero)
                return Vector3.zero;

            return vec.normalized;
        }
    }
}
