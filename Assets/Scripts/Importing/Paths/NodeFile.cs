using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public bool IsHighway;
        public int SpawnProbability;

        public override string ToString()
        {
            return "TrafficLevel=" + TrafficLevel + ",Roadblocks=" + RoadBlocks + ",IsWater=" + IsWater + "\r\n" +
                 "EmergencyOnly=" + EmergencyOnly + ",IsHighway=" + IsHighway + ",SpawnProbability=" + SpawnProbability;
        }
    }

    public struct PathNode : IEquatable<PathNode>
    {
        public UnityEngine.Vector3 Position { get; set; }
        public int BaseLinkID { get; set; }
        public int AreaID { get; set; }
        public int NodeID { get; set; }
        public float PathWidth { get; set; }
        public int NodeType { get; set; } // enum
        public int LinkCount { get; set; }
        public PathNodeFlag Flags;

        public bool Equals(PathNode other)
        {
            return AreaID == other.AreaID && NodeID == other.NodeID;
        }
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
        public int Length { get; set; }
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

        internal static float[][] Borders { get; set; }

        public static void Load()
        {
            int row;
            int col;

            _nodeFiles.Clear();
            Borders = new float[64][];

            //TODO: according to https://gtamods.com/wiki/Paths_%28GTA_SA%29  only the active area and those surrounding it should be loaded at a time
            for (int i = 0; i < 64; i++)
            {
                using (Stream node = Archive.ArchiveManager.ReadFile("nodes" + i + ".dat"))
                {
                    NodeFile nf = new NodeFile(i, node);
                    _nodeFiles.Add(nf);
                }

                row = i % 8;
                col = i / 8;
                Borders[i] = new float[] { -3000 + (750 * row), -3000 + (750 * col) };
            }
        }

        public static NodeFile GetAreaById(int id)
        {
            return NodeFiles[id];
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
            node.LinkCount = flag & 15;
            node.Flags.RoadBlocks = Convert.ToBoolean(flag & 0xF);
            node.Flags.IsWater = Convert.ToBoolean(flag & 0x80);
            node.Flags.EmergencyOnly = Convert.ToBoolean(flag & 0x100);
            node.Flags.IsHighway = !Convert.ToBoolean(flag & 0x1000);
            node.Flags.SpawnProbability = (flag & 0xF0000) >> 16;

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

        /**
         * Returns all the areas ID around the given areaID
         */
        // TODO: this method doesn't work properly
        public static List<int> GetAreaNeighborhood(int areaID)
        {
            List<int> result = new List<int>();
            int indexX = (areaID + 1) % 8;
            int indexY = Convert.ToInt32(Math.Truncate(Convert.ToDecimal((areaID + 1) / 8)));
            if (indexY == 8) indexY = 7;

            int aW, aNW, aN, aNE, aE, aSE, aS, aSW;
            aW = (indexX == 1) ? -1 : (areaID - 1);
            aNW = (indexX == 1) ? -1 : ((indexY == 7) ? -1 : (areaID + 7));
            aN = (indexY == 7) ? -1 : (areaID + 8);
            aNE = (indexX == 0) ? -1 : ((indexY == 7) ? -1 : (areaID + 9));
            aE = (indexX == 0) ? -1 : (areaID + 1);
            aSE = (indexX == 0) ? -1 : ((indexY == 0) ? -1 : (areaID - 7));
            aS = (indexY == 0) ? -1 : (areaID - 8);
            aSW = (indexX == 1) ? -1 : ((indexY == 0) ? -1 : (areaID - 9));

            result.Add(aW);
            result.Add(aNW);
            result.Add(aN);
            result.Add(aNE);
            result.Add(aE);
            result.Add(aSE);
            result.Add(aS);
            result.Add(aSW);

            return result;
        }

        // TODO: this method doesn't work properly
        public static int GetAreaFromPosition(UnityEngine.Vector3 position)
        {
            for (int i = 0; i < 64; i++)
            {
                try
                {
                    if (position.x > NodeReader.Borders[i][0] && position.x < (NodeReader.Borders[i][0] + 750)
                        && position.z > NodeReader.Borders[i][1] && position.z < (NodeReader.Borders[i][1] + 750))
                    {
                        return i;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError("NodeReader.Borders is null");
                }
            }
            return -1;
        }
    }
}
