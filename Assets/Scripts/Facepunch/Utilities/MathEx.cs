namespace UnityEngine
{
    public static class MathExtension
    {
        public static float SnapToCenter(this float val, float snapValue)
        {
            if (snapValue == 0) return val;

            return Mathf.Round((val / snapValue) + 0.5f) * snapValue - 0.5f;
        }

        public static float SnapToEdge(this float val, float snapValue)
        {
            if (snapValue == 0) return val;

            return Mathf.Round(val / snapValue) * snapValue;
        }

        public static Vector3 SnapToCenter(this Vector2 val, float snapValue)
        {
            return new Vector2(val.x.SnapToCenter(snapValue), val.y.SnapToCenter(snapValue));
        }

        public static Vector3 SnapToEdge(this Vector2 val, float snapValue)
        {
            return new Vector2(val.x.SnapToEdge(snapValue), val.y.SnapToEdge(snapValue));
        }

        public static Vector3 SnapToCenter(this Vector3 val, float snapValue)
        {
            return new Vector3(val.x.SnapToCenter(snapValue), val.y.SnapToCenter(snapValue), val.z.SnapToCenter(snapValue));
        }

        public static Vector3 SnapToEdge(this Vector3 val, float snapValue)
        {
            return new Vector3(val.x.SnapToEdge(snapValue), val.y.SnapToEdge(snapValue), val.z.SnapToEdge(snapValue));
        }

        public static Vector3 ToVector3XZ(this Vector2 val)
        {
            return new Vector3(val.x, 0.0f, val.y);
        }

        public static float ClosestPointDistance(this Ray val, Vector3 point)
        {
            return Vector3.Dot(point - val.origin, val.direction);
        }

        public static Vector3 ClosestPoint(this Ray val, Vector3 point)
        {
            return val.GetPoint(val.ClosestPointDistance(point));
        }

        public static float ClosestRayPointDistance(this Ray val, Ray ray)
        {
            Vector3 v0 = val.origin;
            Vector3 v1 = val.direction;
            Vector3 v2 = ray.origin;
            Vector3 v3 = ray.direction;
            Vector3 v4 = v0 - v2;

            float d0 = 0.0f;
            float d1 = 0.0f;

            float dv4v3 = Vector3.Dot(v4, v3);
            float dv3v1 = Vector3.Dot(v3, v1);
            float dv3v3 = Vector3.Dot(v3, v3);
            float dv4v1 = Vector3.Dot(v4, v1);
            float dv1v1 = Vector3.Dot(v1, v1);

            float denom = dv1v1 * dv3v3 - dv3v1 * dv3v1;

            if (Mathf.Abs(denom) > Mathf.Epsilon)
            {
                float numer = dv4v3 * dv3v1 - dv4v1 * dv3v3;
                d0 = numer / denom;
            }
            else
            {
                d0 = 0.0f;
            }

            d1 = (dv4v3 + d0 * dv3v1) / dv3v3;

            if (d1 >= 0.0f)
            {
                return d0;
            }
            else
            {
                d1 = 0.0f;

                return val.ClosestPointDistance(ray.origin);
            }
        }

        public static Vector3 ClosestRayPoint(this Ray val, Ray ray)
        {
            return val.GetPoint(val.ClosestRayPointDistance(ray));
        }

        public static int Mod(this int val, int mod)
        {
            int r = val % mod;

            return r < 0 ? val + mod : r;
        }

        public static bool PlaneTest(this Ray ray, Vector3 planeCenter, Quaternion planeRot, Vector2 planeSize, out Vector3 hitPosition, float gridSize = 0.0f, bool edge = true)
        {
            Plane plane = new Plane(planeRot * Vector3.up, planeCenter);

            hitPosition = Vector3.zero;
            float hitDistance = 0.0f;

            if (!plane.Raycast(ray, out hitDistance))
            {
                return false;
            }

            hitPosition = ray.origin + ray.direction * hitDistance;

            Vector3 hitOffset = hitPosition - planeCenter;

            float distanceLf = Vector3.Dot(hitOffset, planeRot * Vector3.left);
            float distanceUp = Vector3.Dot(hitOffset, planeRot * Vector3.forward);

            if (gridSize > 0.0f)
            {
                if (edge)
                {
                    distanceLf = distanceLf.SnapToEdge(gridSize);
                    distanceUp = distanceUp.SnapToEdge(gridSize);
                }
                else
                {
                    distanceLf = distanceLf.SnapToCenter(gridSize);
                    distanceUp = distanceUp.SnapToCenter(gridSize);
                }
            }

            hitPosition = planeCenter;
            hitPosition += (planeRot * Vector3.left) * distanceLf;
            hitPosition += (planeRot * Vector3.forward) * distanceUp;

            return true;
        }

        public static float NormalizeAngle(this float ang)
        {
            return ang - Mathf.Floor((ang + 180f) / 360f) * 360f;
        }

        public static float AngleDiff(this float ang, float other)
        {
            return (other - ang).NormalizeAngle();
        }
    }
}
