using SanAndreasUnity.Importing.RenderWareStream;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    public class Types
    {
        public static UnityEngine.Vector2 Convert(Vector2 vec)
        {
            return new UnityEngine.Vector2(vec.X, vec.Y);
        }

        public static UnityEngine.Vector3 Convert(Vector3 vec)
        {
            return new UnityEngine.Vector3(vec.X, vec.Z, vec.Y);
        }

        public static Color32 Convert(Color4 clr)
        {
            return new Color32(clr.R, clr.G, clr.B, clr.A);
        }

        public static UnityEngine.BoneWeight Convert(SkinBoneIndices boneIndices, SkinBoneWeights boneWeights)
        {
            return new UnityEngine.BoneWeight
            {
                boneIndex0 = (int)boneIndices.Indices[0],
                boneIndex1 = (int)boneIndices.Indices[1],
                boneIndex2 = (int)boneIndices.Indices[2],
                boneIndex3 = (int)boneIndices.Indices[3],

                weight0 = boneWeights.Weights[0],
                weight1 = boneWeights.Weights[1],
                weight2 = boneWeights.Weights[2],
                weight3 = boneWeights.Weights[3],
            };
        }

        public static UnityEngine.BoneWeight[] Convert(SkinBoneIndices[] boneIndices, SkinBoneWeights[] boneWeights)
        {
            return Enumerable.Range(0, (int)boneIndices.Length).Select(x => Convert(boneIndices[x], boneWeights[x])).ToArray();
        }

        public static UnityEngine.Vector4 Convert(Vector4 vec)
        {
            return new UnityEngine.Vector4(vec.X, vec.Z, vec.Y, vec.W);
        }

        public static UnityEngine.Quaternion Convert(Quaternion quat)
        {
            return new UnityEngine.Quaternion(quat.X, quat.Z, quat.Y, -quat.W);
        }

        public static UnityEngine.Matrix4x4 Convert(Matrix4x4 mat)
        {
            UnityEngine.Vector4 v0 = Convert(mat.V0);
            UnityEngine.Vector4 v1 = Convert(mat.V2);
            UnityEngine.Vector4 v2 = Convert(mat.V1);
            UnityEngine.Vector4 v3 = Convert(mat.V3);

            return new UnityEngine.Matrix4x4
            {
                m00 = v0.x,
                m01 = v0.y,
                m02 = v0.z,
                m03 = 0f,
                m10 = v1.x,
                m11 = v1.y,
                m12 = v1.z,
                m13 = 0f,
                m20 = v2.x,
                m21 = v2.y,
                m22 = v2.z,
                m23 = 0f,
                m30 = v3.x,
                m31 = v3.y,
                m32 = v3.z,
                m33 = 1f,
            };
        }

        public static UnityEngine.Matrix4x4[] Convert(Matrix4x4[] mat)
        {
            return Enumerable.Range(0, (int)mat.Length).Select(x => Convert(mat[x])).ToArray();
        }
    }
}