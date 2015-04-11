using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    public class Animation
    {
        public static UnityEngine.AnimationClip Convert(Clip animation, FrameContainer frames)
        {
            var clip = new UnityEngine.AnimationClip();
            clip.legacy = true;

            string bonePath = "";

            foreach (var bone in animation.Bones)
            {
                bonePath = frames.GetByBoneId(bone.BoneId).Path;

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
