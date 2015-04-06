using System;
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

    public class HierarchyAnimationNode
    {
        public HierarchyAnimationNode(BinaryReader reader)
        {
            NodeId = reader.ReadUInt32();
            NodeIndex = reader.ReadUInt32();
            Flags = (HierarchyAnimationNodeFlags)reader.ReadUInt32();
        }

        UInt32 NodeId;
        UInt32 NodeIndex;
        HierarchyAnimationNodeFlags Flags;
    }

    [SectionType(0x011e)]
    public class HierarchyAnimation : SectionData
    {
        public readonly UInt32 Version;
        public readonly UInt32 NodeId;
        public readonly UInt32 NodeCount;

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
            }
        }
    }
}
