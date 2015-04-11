using System;
using System.Linq;
using System.Collections.Generic;
using SanAndreasUnity.Importing.Conversion;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class FrameInfo
    {
        private string _path;

        public readonly int Index;
        public readonly string Name;
        public readonly Transform Transform;

        public FrameInfo Parent { get; internal set; }

        public string Path
        {
            get { return _path ?? (_path = FindPath()); }
        }

        public FrameInfo(Geometry.GeometryFrame frame, Transform trans)
        {
            Index = frame.Source.Index;
            Name = frame.Name;
            Transform = trans;
        }

        private string FindPath()
        {
            return Parent == null ? Name : String.Format("{0}/{1}", Parent.Path, Name);
        }
    }

    public class FrameContainer : MonoBehaviour, IEnumerable<FrameInfo>
    {
        private FrameInfo[] _frames;

        internal void Initialize(Geometry.GeometryFrame[] frames,
            Dictionary<Geometry.GeometryFrame, Transform> transforms)
        {
            _frames = frames.Select(x => new FrameInfo(x, transforms[x])).ToArray();

            for (var i = 0; i < frames.Length; ++i) {
                var frame = frames[i];
                if (frame.ParentIndex == -1) continue;
                _frames[i].Parent = _frames[frame.ParentIndex];
            }
        }

        public FrameInfo this[string name]
        {
            get { return _frames.FirstOrDefault(x => x.Name == name); }
        }

        public FrameInfo this[int index]
        {
            get { return _frames[index]; }
        }

        public IEnumerator<FrameInfo> GetEnumerator()
        {
            return _frames.AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _frames.GetEnumerator();
        }
    }
}
