using SanAndreasUnity.Importing.Conversion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class FrameContainer : MonoBehaviour, IEnumerable<Frame>
    {
        private Frame[] _frames;
        private Dictionary<int, Frame> _boneIdDict;

        public Frame Root { get { return _frames.FirstOrDefault(x => x.Parent == null); } }

        internal void Initialize(Geometry.GeometryFrame[] frames,
            Dictionary<Geometry.GeometryFrame, Transform> transforms)
        {
            _frames = frames.Select(x =>
            {
                var frame = transforms[x].gameObject.AddComponent<Frame>();
                frame.Initialize(x);
                return frame;
            }).ToArray();

            for (var i = 0; i < frames.Length; ++i)
            {
                var frame = frames[i];
                if (frame.ParentIndex == -1) continue;
                _frames[i].Parent = _frames[frame.ParentIndex];
                _frames[i].ParentIndex = frame.ParentIndex;
            }

            _boneIdDict = _frames.Where(x => x.BoneId > -1).ToDictionary(x => x.BoneId, x => x);
        }

		public Frame GetByName(string name, bool ignoreWhiteSpaces = false)
        {
			if (ignoreWhiteSpaces)
				name = name.Replace (" ", "");

			if (ignoreWhiteSpaces)
				return _frames.FirstOrDefault (x => x.Name.Replace (" ", "") == name);
			else
				return _frames.FirstOrDefault (x => x.Name == name);
        }

        public Frame GetByIndex(int index)
        {
            return _frames[index];
        }

        public bool HasBoneWithId(int boneId)
        {
            return _boneIdDict.ContainsKey(boneId);
        }

        public Frame GetByBoneId(int boneId)
        {
            return _boneIdDict[boneId];
        }

        public IEnumerator<Frame> GetEnumerator()
        {
            return _frames.AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _frames.GetEnumerator();
        }

        private void OnDrawGizmosSelected()
        {
            if (_frames == null) return;

            foreach (var frame in _frames)
            {
                if (frame == null) return;

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(frame.transform.position, 0.02f);

                if (frame.Parent != null)
                {
                    Gizmos.DrawLine(frame.transform.position, frame.Parent.transform.position);
                }
            }
        }
    }
}