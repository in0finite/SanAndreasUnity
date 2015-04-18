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

        private static UnityEngine.AnimationClip Convert(Clip animation, FrameContainer frames,
            out UVector3 rootStart, out UVector3 rootEnd)
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

            var root = animation.Bones.FirstOrDefault(x => x.BoneId == 0);
            if (root != null && root.FrameCount > 0) {
                rootStart = Types.Convert(root.Frames.First().Translation);
                rootEnd = Types.Convert(root.Frames.Last().Translation);
            } else {
                rootStart = rootEnd = UVector3.zero;
            }

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
                    
                    clip.SetCurve(bonePath, typeof(BFrame), "LocalVelocity." + translateAxis.Name,
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

            private readonly Dictionary<string, Animation> _anims
                = new Dictionary<string, Animation>();

            public Package(string fileName)
            {
                using (var reader = new BinaryReader(ArchiveManager.ReadFile(fileName + ".ifp"))) {
                    _package = new AnimationPackage(reader);
                }
            }

            public Animation Load(string clipName, FrameContainer frames)
            {
                if (_anims.ContainsKey(clipName)) return _anims[clipName];
                var anim = new Animation(_package[clipName], frames);
                _anims.Add(clipName, anim);
                return anim;
            }
        }

        private static readonly Dictionary<string, Package> _sLoaded
            = new Dictionary<string, Package>();

        public static Animation Load(string fileName, string clipName, FrameContainer frames)
        {
            Package package;
            if (!_sLoaded.ContainsKey(fileName)) {
                _sLoaded.Add(fileName, package = new Package(fileName));
            } else {
                package = _sLoaded[fileName];
            }

            return package.Load(clipName, frames);
        }

        private readonly UnityEngine.AnimationClip _clip;
        private readonly UVector3 _rootStart;
        private readonly UVector3 _rootEnd;

        public UnityEngine.AnimationClip Clip { get { return _clip; } }

        public UVector3 RootStart { get { return _rootStart; } }
        public UVector3 RootEnd { get { return _rootEnd; } }

        private Animation(Clip anim, FrameContainer frames)
        {
            _clip = Convert(anim, frames, out _rootStart, out _rootEnd);
        }
    }
}
