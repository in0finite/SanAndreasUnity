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
    using UQuaternion = UnityEngine.Quaternion;

    public class Animation
    {
        private const float TimeScale = 1f / 64f;

        private static UnityEngine.AnimationClip Convert(Clip animation, FrameContainer frames)
        {
            var clip = new UnityEngine.AnimationClip();
            clip.legacy = true;

            var rotateAxes = new[] {
                new { Name = "x", Mask = new UVector4(1f, 0f, 0f, 0f) },
                new { Name = "y", Mask = new UVector4(0f, 1f, 0f, 0f) },
                new { Name = "z", Mask = new UVector4(0f, 0f, 1f, 0f) },
                new { Name = "w", Mask = new UVector4(0f, 0f, 0f, 1f) }
            };

            var translateAxes = new[] {
                new { Name = "x", Mask = new UVector3(1f, 0f, 0f) },
                new { Name = "y", Mask = new UVector3(0f, 1f, 0f) },
                new { Name = "z", Mask = new UVector3(0f, 0f, 1f) },
            };

            foreach (var bone in animation.Bones) {
                var bFrames = bone.Frames;
                var frame = frames.GetByBoneId(bone.BoneId);

                string bonePath = frame.Path;

                var axisAngle = bFrames.ToDictionary(x => x, x => {
                    var q = Types.Convert(x.Rotation);
                    float ang; UnityEngine.Vector3 axis;
                    q.ToAngleAxis(out ang, out axis);
                    return new UVector4(q.x, q.y, q.z, q.w);
                });

                foreach (var axis in rotateAxes) {
                    var keys = bFrames
                        .Select(x => new Keyframe(x.Time * TimeScale,
                            UVector4.Dot(axisAngle[x], axis.Mask)))
                        .ToArray();

                    clip.SetCurve(bonePath, typeof(Transform), "localRotation." + axis.Name,
                        new UnityEngine.AnimationCurve(keys));
                }

                var converted = bFrames.Select(x => Types.Convert(x.Translation)).ToArray();

                if (!converted.Any(x => !x.Equals(UVector3.zero))) continue;

                var anyVelocities = false;
                var deltaVals = converted.Select((x, i) => {
                    var prev = Math.Max(0, i - 1);
                    var next = Math.Min(i + 1, converted.Length - 1);

                    var prevTime = bFrames[prev].Time * TimeScale;
                    var nextTime = bFrames[next].Time * TimeScale;

                    return prevTime == nextTime || !(anyVelocities = true) ? UVector3.zero
                        : (converted[next] - converted[prev]) / (nextTime - prevTime);
                }).ToArray();

                foreach (var translateAxis in translateAxes) {
                    var positions = bFrames
                        .Select((x, i) => new Keyframe(x.Time * TimeScale,
                            UVector3.Dot(frame.transform.localPosition + converted[i], translateAxis.Mask)))
                        .ToArray();

                    var deltas = bFrames.Select((x, i) => new Keyframe(x.Time * TimeScale,
                        UVector3.Dot(deltaVals[i], translateAxis.Mask))).ToArray();

                    clip.SetCurve(bonePath, typeof(Transform), "localPosition." + translateAxis.Name,
                        new UnityEngine.AnimationCurve(positions));

                    if (!anyVelocities) continue;
                    
                    clip.SetCurve(bonePath, typeof(Behaviours.Frame), "LocalVelocity." + translateAxis.Name,
                        new UnityEngine.AnimationCurve(deltas));
                }
            }

            clip.wrapMode = WrapMode.Loop;
            clip.EnsureQuaternionContinuity();

            return clip;
        }

        private class Package
        {
            private readonly AnimationPackage _package;

            private readonly Dictionary<string, UnityEngine.AnimationClip> _clips
                = new Dictionary<string, AnimationClip>();

            public Package(string fileName)
            {
                using (var reader = new BinaryReader(ArchiveManager.ReadFile(fileName + ".ifp"))) {
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
            = new Dictionary<string, Package>();

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
