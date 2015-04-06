using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    public class Animation
    {
        private static UnityEngine.Vector3 Convert(Vector3 vec)
        {
            return new UnityEngine.Vector3(vec.X, vec.Z, vec.Y);
        }

        private static UnityEngine.Vector4 Convert(Vector4 vec)
        {
            return new UnityEngine.Vector4(vec.X, vec.Z, vec.Y, vec.W);
        }

        private static UnityEngine.Quaternion Convert(Quaternion quat)
        {
            return new UnityEngine.Quaternion(quat.X, quat.Z, quat.Y, quat.W);
        }

        private static UnityEngine.Matrix4x4 Convert(Matrix4x4 mat)
        {
            UnityEngine.Vector4 v0 = Convert(mat.V0);
            UnityEngine.Vector4 v1 = Convert(mat.V1);
            UnityEngine.Vector4 v2 = Convert(mat.V2);
            UnityEngine.Vector4 v3 = Convert(mat.V3);

            return new UnityEngine.Matrix4x4
            {
                m00 = v0.x, m01 = v0.y, m02 = v0.z, m03 = v0.w,
                m10 = v1.x, m11 = v1.y, m12 = v1.z, m13 = v1.w,
                m20 = v2.x, m21 = v2.y, m22 = v2.z, m23 = v2.w,
                m30 = v3.x, m31 = v3.y, m32 = v3.z, m33 = v3.w,
            };
        }
    }
}
