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
        public readonly int BoneId;
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
            BoneId = frame.Source.HAnim != null ? (int) frame.Source.HAnim.NodeId : -1;
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
        private Dictionary<int, FrameInfo> _boneIdDict;

        public FrameInfo Root { get { return _frames.FirstOrDefault(x => x.Parent == null); } }

        internal void Initialize(Geometry.GeometryFrame[] frames,
            Dictionary<Geometry.GeometryFrame, Transform> transforms)
        {
            _frames = frames.Select(x => new FrameInfo(x, transforms[x])).ToArray();

            for (var i = 0; i < frames.Length; ++i) {
                var frame = frames[i];
                if (frame.ParentIndex == -1) continue;
                _frames[i].Parent = _frames[frame.ParentIndex];
            }

            _boneIdDict = _frames.ToDictionary(x => x.BoneId, x => x);
        }

        public FrameInfo GetByName(string name)
        {
            return _frames.FirstOrDefault(x => x.Name == name);
        }
        
        public FrameInfo GetByIndex(int index)
        {
            return _frames.FirstOrDefault(x => x.Name == name);
        }

        public FrameInfo GetByBoneId(int boneId)
        {
            return _boneIdDict[boneId];
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
