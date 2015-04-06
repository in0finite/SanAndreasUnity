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

        private static UnityEngine.Quaternion Convert(Quaternion quat)
        {
            return new UnityEngine.Quaternion(quat.X, quat.Z, quat.Y, quat.W);
        }
    }
}
