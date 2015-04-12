using System;
using SanAndreasUnity.Importing.RenderWareStream;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class Frame : MonoBehaviour
    {
        private string _path;

        public int Index { get; private set; }
        public int BoneId { get; private set; }
        public string Name { get; private set; }

        public Frame Parent { get; internal set; }

        public HierarchyAnimationFlags Flags;

        public string Path
        {
            get { return _path ?? (_path = FindPath()); }
        }

        public bool AnimationDriven;

        public Vector3 RotationAxis;
        public float RotationAngle;

        internal void Initialize(Importing.Conversion.Geometry.GeometryFrame frame)
        {
            Index = frame.Source.Index;
            BoneId = frame.Source.HAnim != null ? (int) frame.Source.HAnim.NodeId : -1;
            Name = frame.Name;

            transform.localRotation.ToAngleAxis(out RotationAngle, out RotationAxis);

            if (frame.Source.HAnim == null) return;

            Flags = frame.Source.HAnim.Flags;
        }

        private string FindPath()
        {
            return Parent == null ? Name : string.Format("{0}/{1}", Parent.Path, Name);
        }

        private void Update()
        {
            if (!AnimationDriven) return;
            transform.localRotation = Quaternion.AngleAxis(RotationAngle, RotationAxis);
        }
    }
}
