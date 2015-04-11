using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    public class Animation
    {
        public static UnityEngine.AnimationClip Convert(Clip animation, Transform[] trans)
        {
            var clip = new UnityEngine.AnimationClip();
            clip.legacy = true;

            string bonePath = "";

            foreach (var bone in animation.Bones)
            {
                if (bone.BoneId == 41) bonePath = "unnamed/Root/ Pelvis/ L Thigh";
                else if (bone.BoneId == 42) bonePath = "unnamed/Root/ Pelvis/ L Thigh/ L Calf";
                else if (bone.BoneId == 51) bonePath = "unnamed/Root/ Pelvis/ R Thigh";
                else if (bone.BoneId == 52) bonePath = "unnamed/Root/ Pelvis/ R Thigh/ R Calf";
                else continue;


                //clip.SetCurve(bonePath, typeof(Transform), "localPosition.x", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float)x.Time * 0.1f, x.Translation.X)).ToArray()));
                //clip.SetCurve(bonePath, typeof(Transform), "localPosition.y", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float)x.Time * 0.1f, x.Translation.Z)).ToArray()));
                //clip.SetCurve(bonePath, typeof(Transform), "localPosition.z", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float)x.Time * 0.1f, x.Translation.Y)).ToArray()));

                clip.SetCurve(bonePath, typeof(Transform), "localRotation.x", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float)x.Time / 32f, x.Rotation.X)).ToArray()));
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.y", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float)x.Time / 32f, -x.Rotation.Z)).ToArray()));
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.z", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float)x.Time / 32f, x.Rotation.Y)).ToArray()));
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.w", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float)x.Time / 32f, x.Rotation.W)).ToArray()));  
            }

            clip.wrapMode = WrapMode.Loop;

            return clip;
        }
    }
}
