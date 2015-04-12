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
    using BFrame = SanAndreasUnity.Behaviours.Frame;
    using UVector3 = UnityEngine.Vector3;
    using UVector4 = UnityEngine.Vector4;

    public class Animation
    {
        private const float TimeScale = 0.02f;

        private static UnityEngine.AnimationClip Convert(Clip animation, FrameContainer frames)
        {
            var clip = new UnityEngine.AnimationClip();
            clip.legacy = true;

            var rotateAxes = new[] {
                new { Name = "RotationAxis.x", Mask = new UVector4(1f, 0f, 0f, 0f) },
                new { Name = "RotationAxis.y", Mask = new UVector4(0f, 1f, 0f, 0f) },
                new { Name = "RotationAxis.z", Mask = new UVector4(0f, 0f, 1f, 0f) },
                new { Name = "RotationAngle", Mask = new UVector4(0f, 0f, 0f, 1f) }
            };

            var translateAxes = new[] {
                new { Name = "localPosition.x", Mask = new UVector3(1f, 0f, 0f) },
                new { Name = "localPosition.y", Mask = new UVector3(0f, 1f, 0f) },
                new { Name = "localPosition.z", Mask = new UVector3(0f, 0f, 1f) },
            };

            foreach (var bone in animation.Bones)
            {
                var frame = frames.GetByBoneId(bone.BoneId);
                string bonePath = frame.Path;

                var axisAngle = bone.Frames.ToDictionary(x => x, x => {
                    var q = Types.Convert(x.Rotation);
                    float ang; UnityEngine.Vector3 axis;
                    q.ToAngleAxis(out ang, out axis);
                    return new UVector4(axis.x, axis.y, axis.z, ang);
                });

                foreach (var axis in rotateAxes) {
                    var keys = bone.Frames
                        .Select(x => new Keyframe(x.Time * TimeScale,
                            UVector4.Dot(axisAngle[x], axis.Mask)))
                        .ToArray();

                    clip.SetCurve(bonePath, typeof(BFrame), axis.Name,
                        new UnityEngine.AnimationCurve(keys));
                }

                if (bone.Name == "Root") continue;

                foreach (var translateAxis in translateAxes)
                {
                    var keys = bone.Frames
                        .Select(x => new Keyframe(x.Time * TimeScale,
                            UVector3.Dot(frame.transform.localPosition + Types.Convert(x.Translation), translateAxis.Mask)))
                        .ToArray();

                    clip.SetCurve(bonePath, typeof(Transform), translateAxis.Name,
                        new UnityEngine.AnimationCurve(keys));
                }
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
