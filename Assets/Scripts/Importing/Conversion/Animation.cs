using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Archive;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    public class Animation
    {
        private static UnityEngine.AnimationClip Convert(Clip animation, FrameContainer frames)
        {
            var clip = new UnityEngine.AnimationClip();
            clip.legacy = true;

            string bonePath = "";

            foreach (var bone in animation.Bones)
            {
                bonePath = frames.GetByBoneId(bone.BoneId).Path;

                //clip.SetCurve(bonePath, typeof(Transform), "localPosition.x", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float) x.Time / 50f, x.Translation.X)).ToArray()));
                //clip.SetCurve(bonePath, typeof(Transform), "localPosition.y", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float) x.Time / 50f, x.Translation.Z)).ToArray()));
                //clip.SetCurve(bonePath, typeof(Transform), "localPosition.z", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float) x.Time / 50f, x.Translation.Y)).ToArray()));

                clip.SetCurve(bonePath, typeof(Transform), "localRotation.x", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float) x.Time / 50f, x.Rotation.X)).ToArray()));
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.y", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float) x.Time / 50f, -x.Rotation.Z)).ToArray()));
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.z", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float) x.Time / 50f, x.Rotation.Y)).ToArray()));
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.w", new UnityEngine.AnimationCurve(bone.Frames.Select(x => new Keyframe((float) x.Time / 50f, x.Rotation.W)).ToArray()));  
            }

            clip.wrapMode = WrapMode.Loop;

            return clip;
        }

        private class Package
        {
            private readonly AnimationPackage _package;

            private readonly Dictionary<string, UnityEngine.AnimationClip> _clips
                = new Dictionary<string,AnimationClip>();

            public Package(string fileName)
            {
                using (var reader = new BinaryReader(ArchiveManager.ReadFile(fileName))) {
                    _package = new AnimationPackage(reader);
                }
            }

            public UnityEngine.AnimationClip Load(string clipName, FrameContainer frames)
            {
                if (_clips.ContainsKey(clipName)) return _clips[clipName];
                var clip = Convert(_package[clipName], frames);
                _clips.Add(clipName, clip);
                return clip;
            }
        }

        private static readonly Dictionary<string, Package> _sLoaded
            = new Dictionary<string,Package>();

        public static UnityEngine.AnimationClip Load(string fileName, string clipName, FrameContainer frames)
        {
            Package package;
            if (!_sLoaded.ContainsKey(fileName)) {
                _sLoaded.Add(fileName, package = new Package(fileName));
            } else {
                package = _sLoaded[fileName];
            }

            return package.Load(clipName, frames);
        }
    }
}
