using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Importing.Paths
{
    public enum PathNodeTrafficLevel
    {
        Full = 0,
        High = 1,
        Medium = 2,
        Low = 3
    }

    public struct PathNodeFlag
    {
        public PathNodeTrafficLevel TrafficLevel;
        public bool RoadBlocks;
        public bool IsWater;
        public bool EmergencyOnly;
        public bool IsHighway; // ignored for peds
        public int SpawnProbability;
        public bool Parking;

        public override string ToString()
        {
            return "TrafficLevel=" + TrafficLevel + ",Roadblocks=" + RoadBlocks + ",IsWater=" + IsWater +
                 ",EmergencyOnly=" + EmergencyOnly + ",IsHighway=" + IsHighway + ",SpawnProbability=" + SpawnProbability +
                 ",Parking=" + Parking;
        }
    }

    public struct PathNodeId : IEquatable<PathNodeId>
    {
        public int AreaID { get; set; }
        public int NodeID { get; set; }

        public bool Equals(PathNodeId other)
        {
            return AreaID == other.AreaID && NodeID == other.NodeID;
        }

        public override int GetHashCode()
        {
            return ((AreaID << 5) + AreaID) ^ NodeID;
        }

        public override string ToString()
        {
            return $"(AreaID {AreaID}, NodeID {NodeID})";
        }

        public static PathNodeId InvalidId => new PathNodeId { AreaID = -1, NodeID = -1 };
    }

    public struct PathNode : IEquatable<PathNode>
    {
        public UnityEngine.Vector3 Position { get; set; }
        public int BaseLinkID { get; set; }
        public int AreaID { get; set; }
        public int NodeID { get; set; }
        public PathNodeId Id => new PathNodeId { AreaID = AreaID, NodeID = NodeID };
        public float PathWidth { get; set; }
        public int NodeType { get; set; } // enum
        public int LinkCount { get; set; }
        public PathNodeFlag Flags;

        public bool ShouldPedBeSpawnedHere => !this.Flags.EmergencyOnly;

        public bool Equals(PathNode other)
        {
            return AreaID == other.AreaID && NodeID == other.NodeID;
        }

        public override string ToString()
        {
            return $"(Id {Id}, Position {Position}, PathWidth {PathWidth})";
        }

        public static PathNode InvalidNode => new PathNode { AreaID = -1, NodeID = -1 };
    }

    public struct NavNode
    {
        public UnityEngine.Vector2 Position { get; set; }
        public int TargetAreaID { get; set; }
        public int TargetNodeID { get; set; }
        public UnityEngine.Vector2 Direction { get; set; }
        public int Width { get; set; }
        public int NumLeftLanes { get; set; }
        public int NumRightLanes { get; set; }
        public int TrafficLightDirection { get; set; }
        public int TrafficLightBehavior { get; set; }
        public int IsTrainCrossing { get; set; }
        public byte Flags { get; set; }
    }

    public struct NodeLink
    {
        public int AreaID { get; set; }
        public int NodeID { get; set; }
        public PathNodeId PathNodeId => new PathNodeId { AreaID = AreaID, NodeID = NodeID };
        public int Length { get; set; }

        public override string ToString()
        {
            return $"(AreaID {AreaID}, NodeID {NodeID}, Length {Length})";
        }
    }

    public struct PathIntersectionFlags
    {
        public bool IsRoadCross { get; set; }
        public bool IsTrafficLight { get; set; }
    }

    public struct NavNodeLink
    {
        public int NodeLink { get; set; }
        public int AreaID { get; set; }
    }

    public static class NodeReader
    {
        private static readonly List<NodeFile> _nodeFiles = new List<NodeFile>();
        public static IReadOnlyList<NodeFile> NodeFiles { get; } = new ReadOnlyCollection<NodeFile>(_nodeFiles);

        public static void Load()
        {
            _nodeFiles.Clear();

            //TODO: according to https://gtamods.com/wiki/Paths_%28GTA_SA%29  only the active area and those surrounding it should be loaded at a time
            for (int i = 0; i < 64; i++)
            {
                using (Stream node = Archive.ArchiveManager.ReadFile("nodes" + i + ".dat"))
                {
                    NodeFile nf = new NodeFile(i, node);
                    _nodeFiles.Add(nf);
                }
            }
        }

        public static Vector2Int GetAreaIndexes(int areaId)
        {
            return new Vector2Int(areaId % 8, areaId / 8);
        }

        public static int GetAreaIdFromIndexes(Vector2Int areaIndexes)
        {
            return areaIndexes.y * 8 + areaIndexes.x;
        }

        public static NodeFile GetAreaById(int id)
        {
            return NodeFiles[id];
        }

        public static PathNode GetNodeById(PathNodeId id)
        {
            return NodeFiles[id.AreaID].GetNodeById(id.NodeID);
        }

        public static Vector2Int GetAreaIndexesFromPosition(UnityEngine.Vector3 position, bool clamp)
        {
            return new Vector2Int(
                GetAreaIndexFromPosition(position.x, clamp),
                GetAreaIndexFromPosition(position.z, clamp));
        }

        private static int GetAreaIndexFromPosition(float position, bool clamp)
        {
            if (position >= 3000f)
            {
                return clamp ? 7 : -1;
            }

            if (position <= -3000f)
            {
                return clamp ? 0 : -1;
            }

            int areaIndex = Mathf.FloorToInt((position - (-3000.0f)) / 750.0f);

            // fix for potential floating point errors
            areaIndex = Mathf.Clamp(areaIndex, 0, 7);

            return areaIndex;
        }

        public static List<Vector2Int> GetAreaIndexesInRadius(UnityEngine.Vector3 pos, float radius)
        {
            // convert to outer bounding rect
            // get area indexes of rect min and rect max
            // all indexes in between match

            UnityEngine.Vector3 min = pos;
            min.x -= radius;
            min.z -= radius;

            UnityEngine.Vector3 max = pos;
            max.x += radius;
            max.z += radius;

            var areasInRadius = new List<Vector2Int>();

            // check for intersection with map
            // if there is no intersection, we can't use clamping below, because it will always return
            // some areas, even if they are outside of radius

            if (min.x >= 3000f)
                return areasInRadius;
            if (min.z >= 3000f)
                return areasInRadius;

            if (max.x <= -3000f)
                return areasInRadius;
            if (max.z <= -3000f)
                return areasInRadius;

            Vector2Int minIndexes = GetAreaIndexesFromPosition(min, true);
            Vector2Int maxIndexes = GetAreaIndexesFromPosition(max, true);

            for (int x = minIndexes.x; x <= maxIndexes.x; x++)
            {
                for (int y = minIndexes.y; y <= maxIndexes.y; y++)
                {
                    areasInRadius.Add(new Vector2Int(x, y));
                }
            }

            return areasInRadius;
        }

        public static IEnumerable<NodeFile> GetAreasInRadius(UnityEngine.Vector3 pos, float radius)
        {
            return GetAreaIndexesInRadius(pos, radius)
                .Select(GetAreaIdFromIndexes)
                .Select(GetAreaById);
        }

        public static IEnumerable<PathNode> GetAllLinkedNodes(PathNode pathNode)
        {
            var nodeArea = GetAreaById(pathNode.AreaID);

            for (int i = 0; i < pathNode.LinkCount; i++)
            {
                NodeLink link = nodeArea.NodeLinks[pathNode.BaseLinkID + i];

                NodeFile targetArea = GetAreaById(link.AreaID);
                PathNode targetNode = targetArea.GetNodeById(link.NodeID);

                yield return targetNode;
            }
        }

        public static float GetDistanceBetweenNodes(PathNodeId a, PathNodeId b)
        {
            return UnityEngine.Vector3.Distance(GetNodeById(a).Position, GetNodeById(b).Position);
        }
    }

    public class NodeFile
    {
        public int Id { get; }
        public int NumOfVehNodes { get; private set; }
        public int NumOfPedNodes { get; private set; }
        public int NumOfNavNodes { get; private set; }
        public int NumOfLinks { get; private set; }
        public List<PathNode> VehicleNodes { get; } = new List<PathNode>();
        public List<PathNode> PedNodes { get; } = new List<PathNode>();
        public List<NavNode> NavNodes { get; } = new List<NavNode>();
        public List<NodeLink> NodeLinks { get; } = new List<NodeLink>();
        public List<NavNodeLink> NavNodeLinks { get; } = new List<NavNodeLink>();
        public List<PathIntersectionFlags> PathIntersections { get; } = new List<PathIntersectionFlags>();


        public NodeFile(int id, Stream stream)
        {
            Id = id;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                ReadHeader(reader);
                ReadNodes(reader);
                ReadNavNodes(reader);
                ReadLinks(reader);
                reader.ReadBytes(768);
                ReadNavLinks(reader);
                ReadLinkLengths(reader);
                ReadPathIntersectionFlags(reader);
            }
        }

        private void ReadNodes(BinaryReader reader)
        {
            for (int i = 0; i < NumOfVehNodes; i++)
            {
                VehicleNodes.Add(ReadNode(reader));
            }

            for (int i = 0; i < NumOfPedNodes; i++)
            {
                PedNodes.Add(ReadNode(reader));
            }
        }

        private PathNode ReadNode(BinaryReader reader)
        {
            PathNode node = new PathNode();
            reader.ReadUInt32();
            reader.ReadUInt32();
            float x = (float)reader.ReadInt16() / 8;
            float z = (float)reader.ReadInt16() / 8;
            float y = (float)reader.ReadInt16() / 8;
            node.Position = new UnityEngine.Vector3(x, y, z);
            short heuristic = reader.ReadInt16();
            if (heuristic != 0x7FFE) UnityEngine.Debug.LogError("corrupted path node?");
            node.BaseLinkID = reader.ReadUInt16();
            node.AreaID = reader.ReadUInt16();
            node.NodeID = reader.ReadUInt16();
            node.PathWidth = (float)reader.ReadByte() / 8;
            node.NodeType = reader.ReadByte();

            int flag = reader.ReadInt32();
            node.LinkCount = flag & 0xF;
            node.Flags.TrafficLevel = (PathNodeTrafficLevel)((flag & 0x30) >> 4);
            node.Flags.RoadBlocks = (flag & (1 << 6)) != 0;
            node.Flags.IsWater = (flag & 0x80) != 0;
            node.Flags.EmergencyOnly = (flag & 0x100) != 0;
            node.Flags.IsHighway = (flag & 0x1000) == 0;
            node.Flags.SpawnProbability = (flag & 0xF0000) >> 16;
            node.Flags.Parking = (flag & 0x200000) != 0;

            return node;
        }

        private void ReadNavNodes(BinaryReader reader)
        {
            for (int i = 0; i < NumOfNavNodes; i++)
            {
                NavNode node = new NavNode();
                node.Position = new UnityEngine.Vector2(reader.ReadInt16(), reader.ReadInt16());
                node.TargetAreaID = reader.ReadUInt16();
                node.TargetNodeID = reader.ReadUInt16();
                node.Direction = new UnityEngine.Vector2(reader.ReadSByte(), reader.ReadSByte());
                node.Width = reader.ReadByte() / 8;

                byte flags = reader.ReadByte();
                node.NumLeftLanes = flags & 7;
                node.NumRightLanes = (flags >> 3) & 7;
                node.TrafficLightDirection = (flags >> 4) & 1;

                flags = reader.ReadByte();
                node.TrafficLightBehavior = flags & 3;
                node.IsTrainCrossing = (flags >> 2) & 1;
                node.Flags = reader.ReadByte();

                NavNodes.Add(node);
            }
        }
        private void ReadLinks(BinaryReader reader)
        {
            for (int i = 0; i < NumOfLinks; i++)
            {
                NodeLink link = new NodeLink();
                link.AreaID = reader.ReadUInt16();
                link.NodeID = reader.ReadUInt16();
                NodeLinks.Add(link);
            }
        }

        private void ReadNavLinks(BinaryReader reader)
        {
            for (int i = 0; i < NumOfNavNodes; i++)
            {
                ushort bytes = reader.ReadUInt16();
                NavNodeLink link = new NavNodeLink();
                link.NodeLink = bytes & 1023;
                link.AreaID = bytes >> 10;
                NavNodeLinks.Add(link);
            }
        }
        private void ReadLinkLengths(BinaryReader reader)
        {
            for (int i = 0; i < NumOfLinks; i++)
            {
                ushort length = reader.ReadByte();
                NodeLink tmp = NodeLinks[i];
                tmp.Length = length;
                NodeLinks[i] = tmp;
            }
        }

        private void ReadPathIntersectionFlags(BinaryReader reader)
        {
            for (int i = 0; i < NumOfLinks; i++)
            {
                byte roadCross = reader.ReadByte();
                //byte pedTrafficLight = reader.ReadByte();
                /*
				PathIntersectionFlags pif = new PathIntersectionFlags()
				{
					IsRoadCross = (roadCross & 1) ? true : false,
					IsTrafficLight = (roadCross & 1) ? true : false
				};*/
            }
        }

        private void ReadHeader(BinaryReader reader)
        {
            int numOfNodes = (int)reader.ReadUInt32();
            NumOfVehNodes = (int)reader.ReadUInt32();
            NumOfPedNodes = (int)reader.ReadUInt32();
            if (NumOfVehNodes + NumOfPedNodes != numOfNodes)
                throw new Exception($"Node file {Id} has invalid number of nodes");
            NumOfNavNodes = (int)reader.ReadUInt32();
            NumOfLinks = (int)reader.ReadUInt32();
        }

        public PathNode GetPedNodeById(int nodeId)
        {
            return PedNodes[nodeId - NumOfVehNodes];
        }

        public PathNode GetNodeById(int nodeId)
        {
            if (nodeId < NumOfVehNodes)
                return VehicleNodes[nodeId];
            return PedNodes[nodeId - NumOfVehNodes];
        }
    }
}
