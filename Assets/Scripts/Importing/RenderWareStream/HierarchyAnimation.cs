using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    public enum HierarchyAnimationFlags
    {
        None = 0,
        SubHierarchy = 1,
        NoMatrices = 2,
        UpdateLocalMatrices = 4096,
        UpdateGlobalMatrices = 8192,
        LocalSpaceMatrices = 16384,
    }

    public enum HierarchyAnimationNodeFlags
    {
        None = 0,
        PopParentMatrix = 1,
        PushParentMatrix = 2,
    }

    public class HierarchyAnimationNode : IEnumerable<HierarchyAnimationNode>
    {
        private readonly List<HierarchyAnimationNode> _children;

        public readonly UInt32 NodeId;
        public readonly UInt32 NodeIndex;
        public readonly HierarchyAnimationNodeFlags Flags;

        public bool Push
        {
            get
            {
                return (Flags & HierarchyAnimationNodeFlags.PushParentMatrix)
                    == HierarchyAnimationNodeFlags.PushParentMatrix;
            }
        }

        public bool Pop
        {
            get
            {
                return (Flags & HierarchyAnimationNodeFlags.PopParentMatrix)
                    == HierarchyAnimationNodeFlags.PopParentMatrix;
            }
        }

        public HierarchyAnimationNode(BinaryReader reader)
        {
            _children = new List<HierarchyAnimationNode>();

            NodeId = reader.ReadUInt32();
            NodeIndex = reader.ReadUInt32();
            Flags = (HierarchyAnimationNodeFlags)reader.ReadUInt32();
        }

        public void AddChild(HierarchyAnimationNode child)
        {
            _children.Add(child);
        }

        public IEnumerator<HierarchyAnimationNode> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }
    }

    [SectionType(0x011e)]
    public class HierarchyAnimation : SectionData
    {
        public readonly UInt32 Version;
        public readonly UInt32 NodeId;
        public readonly UInt32 NodeCount;

        public readonly HierarchyAnimationNode Root;
        public readonly HierarchyAnimationNode[] Nodes;

        public readonly HierarchyAnimationFlags Flags;
        public readonly UInt32 KeyFrameSize;

        public HierarchyAnimation(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var reader = new BinaryReader(stream);

            Version = reader.ReadUInt32();
            NodeId = reader.ReadUInt32();
            NodeCount = reader.ReadUInt32();

            Nodes = new HierarchyAnimationNode[NodeCount];

            if (NodeCount > 0)
            {
                Flags = (HierarchyAnimationFlags)reader.ReadUInt32();
                KeyFrameSize = reader.ReadUInt32();

                for (int i = 0; i < NodeCount; ++i)
                {
                    Nodes[i] = new HierarchyAnimationNode(reader);
                }

                var stack = new Stack<HierarchyAnimationNode>();
                stack.Push(Root = Nodes[0]);

                foreach (var node in Nodes.Skip(1))
                {
                    stack.Peek().AddChild(node);

                    if (node.Push)
                    {
                        stack.Push(node);
                    }
                    else if (node.Pop)
                    {
                        var n = node;
                        do
                        {
                            n = stack.Pop();
                        } while (n.Pop);
                    }
                }
            }
        }
    }
}