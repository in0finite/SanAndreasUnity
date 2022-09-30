using SanAndreasUnity.Importing.Paths;
using UGameCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace SanAndreasUnity.Behaviours
{
    public class PathfindingManager : StartupSingleton<PathfindingManager>
    {
        public class PathResult
        {
            public bool IsSuccess { get; set; }

            public List<PathNodeId> Nodes { get; set; }

            public float Distance { get; set; }

            public float TotalWeight { get; set; }

            public float TimeElapsed { get; set; }
        }

        private struct NodePathfindingData
        {
            public float f, g;
            public PathNodeId parentId;
            public bool hasParent;

            public override string ToString()
            {
                return $"(f {f}, g {g}{(hasParent ? $", parent {parentId}" : string.Empty)})";
            }
        }

        private class NodeComparer : IComparer<PathNodeId>
        {
            private readonly NodePathfindingData[][] m_nodePathfindingDatas;

            public NodeComparer(NodePathfindingData[][] nodePathfindingDatas)
            {
                m_nodePathfindingDatas = nodePathfindingDatas;
            }

            int IComparer<PathNodeId>.Compare(PathNodeId a, PathNodeId b)
            {
                float fa = m_nodePathfindingDatas[a.AreaID][a.NodeID].f;
                float fb = m_nodePathfindingDatas[b.AreaID][b.NodeID].f;

                if (fa == fb)
                {
                    // f is equal, compare by id

                    if (a.AreaID == b.AreaID)
                        return a.NodeID.CompareTo(b.NodeID);
                    return a.AreaID.CompareTo(b.AreaID);
                }
                
                return fa.CompareTo(fb);
            }
        }

        public BackgroundJobRunner BackgroundJobRunner { get; } = new BackgroundJobRunner();

        [SerializeField] private ushort m_maxTimePerFrameMs = 0;
        public ushort MaxTimePerFrameMs => m_maxTimePerFrameMs;

        private NodePathfindingData[][] m_nodePathfindingDatas = null;

        private readonly HashSet<PathNodeId> m_closedList = new HashSet<PathNodeId>();
        private readonly List<PathNodeId> m_modifiedDatas = new List<PathNodeId>();

        private static readonly FieldInfo s_rootField = typeof(SortedSet<PathNodeId>).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly Type s_nodeType = s_rootField.FieldType;
        private static readonly FieldInfo s_leftNodeField = s_nodeType.GetField("<Left>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_itemNodeField = s_nodeType.GetField("<Item>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        [SerializeField] private int m_navMeshPathfindingIterationsPerFrame = 100;



        protected override void OnSingletonStart()
        {
            this.BackgroundJobRunner.EnsureBackgroundThreadStarted();

            NavMesh.pathfindingIterationsPerFrame = m_navMeshPathfindingIterationsPerFrame;
        }

        protected override void OnSingletonDisable()
        {
            this.BackgroundJobRunner.ShutDown();
        }

        void OnLoaderFinished()
        {
            m_nodePathfindingDatas = new NodePathfindingData[NodeReader.NodeFiles.Count][];
            for (int i = 0; i < m_nodePathfindingDatas.Length; i++)
            {
                m_nodePathfindingDatas[i] = new NodePathfindingData[NodeReader.NodeFiles[i].NumOfPedNodes + NodeReader.NodeFiles[i].NumOfVehNodes];
            }
        }

        void Update()
        {
            this.BackgroundJobRunner.UpdateJobs(m_maxTimePerFrameMs);
        }

        public void FindPath(Vector3 source, Vector3 destination, Action<PathResult> callback)
        {
            this.BackgroundJobRunner.RegisterJob(new BackgroundJobRunner.Job<PathResult>()
            {
                action = () => FindPathInBackground(source, destination),
                callbackFinish = callback,
                priority = 1,
            });
        }

        private PathResult FindPathInBackground(Vector3 sourcePos, Vector3 destinationPos)
        {

            // First try including Emergency nodes as source node, then if it fails,
            // try without Emergency nodes. This is because some of them are isolated in
            // small groups.

            // find closest node of source position
            var closestSourceNode = NodeReader.GetAreasInRadius(sourcePos, 300f)
                .SelectMany(_ => _.PedNodes)
                .MinBy(_ => Vector3.Distance(_.Position, sourcePos), PathNode.InvalidNode);

            if (closestSourceNode.Equals(PathNode.InvalidNode))
                return new PathResult { IsSuccess = false };

            // find closest node of destination position
            var closestDestinationNode = NodeReader.GetAreasInRadius(destinationPos, 300f)
                .SelectMany(_ => _.PedNodes)
                .MinBy(_ => Vector3.Distance(_.Position, destinationPos), PathNode.InvalidNode);

            if (closestDestinationNode.Equals(PathNode.InvalidNode))
                return new PathResult { IsSuccess = false };

            var pathResult = FindPathInBackground(closestSourceNode, closestDestinationNode);
            if (pathResult.IsSuccess)
                return pathResult;

            // try with non-Emergency node

            if (!closestSourceNode.Flags.EmergencyOnly)
                return pathResult;

            closestSourceNode = NodeReader.GetAreasInRadius(sourcePos, 300f)
                .SelectMany(_ => _.PedNodes)
                .Where(_ => !_.Flags.EmergencyOnly)
                .MinBy(_ => Vector3.Distance(_.Position, sourcePos), PathNode.InvalidNode);

            if (closestSourceNode.Equals(PathNode.InvalidNode))
                return pathResult;

            return FindPathInBackground(closestSourceNode, closestDestinationNode);
        }

        private PathResult FindPathInBackground(PathNode sourceNode, PathNode destinationNode)
        {
            var stopwatch = Stopwatch.StartNew();
            PathResult pathResult = new PathResult { IsSuccess = false };

            this.RestoreModifiedDatas();

            if (FindPathFromNodeToNode(sourceNode.Id, destinationNode.Id))
            {
                pathResult = BuildPath(destinationNode.Id);
            }

            int numModifiedDatas = m_modifiedDatas.Count;
            RestoreModifiedDatas();

            m_closedList.Clear();

            pathResult.TimeElapsed = (float)stopwatch.Elapsed.TotalSeconds;

            UnityEngine.Debug.Log($"Path finding finished: time {pathResult.TimeElapsed * 1000} ms, num nodes {pathResult.Nodes?.Count ?? 0}, numModifiedDatas {numModifiedDatas}, g {pathResult.TotalWeight}, distance {pathResult.Distance}");

            return pathResult;
        }

        private bool FindPathFromNodeToNode(PathNodeId startId, PathNodeId targetId)
        {
            m_closedList.Clear();
            var closedList = m_closedList;

            var openList = new SortedSet<PathNodeId>(new NodeComparer(m_nodePathfindingDatas));

            var startData = GetData(startId);
            startData.f = startData.g + CalculateHeuristic(startId, targetId);
            SetData(startId, startData);

            AddOrThrow(openList, startId);

            while (openList.Count > 0)
            {
                PathNodeId idN = MinFast(openList);

                if (idN.Equals(targetId))
                {
                    return true;
                }

                RemoveOrThrow(openList, idN);
                closedList.Add(idN);

                var nodeN = NodeReader.GetNodeById(idN);
                var areaN = NodeReader.GetAreaById(idN.AreaID);
                var dataN = GetData(idN);

                for (int i = 0; i < nodeN.LinkCount; i++)
                {
                    NodeLink link = areaN.NodeLinks[nodeN.BaseLinkID + i];

                    PathNodeId idM = link.PathNodeId;

                    if (idM.Equals(targetId))
                    {
                        var dataM = GetResettedData();
                        dataM.g = dataN.g + CalculateLinkWeight(idN, idM);
                        float h = CalculateHeuristic(idM, targetId);
                        dataM.f = dataM.g + h;
                        dataM.parentId = idN;
                        dataM.hasParent = true;
                        SetData(idM, dataM);

                        return true;
                    }

                    if (closedList.Contains(idM))
                        continue;

                    if (openList.Contains(idM))
                    {
                        float gNew = dataN.g + CalculateLinkWeight(idN, idM);
                        float hNew = CalculateHeuristic(idM, targetId);
                        float fNew = gNew + hNew;

                        var dataM = GetData(idM);

                        if (dataM.f > fNew)
                        {
                            // update

                            RemoveOrThrow(openList, idM); // first remove with old data

                            dataM.g = gNew;
                            dataM.f = fNew;
                            dataM.parentId = idN;
                            dataM.hasParent = true;

                            SetData(idM, dataM);

                            AddOrThrow(openList, idM); // now add with new data
                        }
                    }
                    else
                    {
                        var dataM = GetResettedData();
                        dataM.g = dataN.g + CalculateLinkWeight(idN, idM);
                        float h = CalculateHeuristic(idM, targetId);
                        dataM.f = dataM.g + h;
                        dataM.parentId = idN;
                        dataM.hasParent = true;
                        SetData(idM, dataM);
                        AddOrThrow(openList, idM); // do it after setting data
                    }

                }
            }

            return false;
        }

        private NodePathfindingData GetData(PathNodeId id)
        {
            return m_nodePathfindingDatas[id.AreaID][id.NodeID];
        }

        private void SetData(PathNodeId id, NodePathfindingData data)
        {
            m_nodePathfindingDatas[id.AreaID][id.NodeID] = data;
            m_modifiedDatas.Add(id);
        }

        private void SetDataNoRemember(PathNodeId id, NodePathfindingData data)
        {
            m_nodePathfindingDatas[id.AreaID][id.NodeID] = data;
        }

        private float CalculateHeuristic(PathNodeId source, PathNodeId destination)
        {
            return NodeReader.GetDistanceBetweenNodes(source, destination);
        }

        private float CalculateLinkWeight(PathNodeId parentId, PathNodeId neighbourId)
        {
            return NodeReader.GetDistanceBetweenNodes(parentId, neighbourId);
        }

        private void RestoreModifiedDatas()
        {
            foreach (var id in m_modifiedDatas)
            {
                var data = GetResettedData();
                SetDataNoRemember(id, data);
            }

            m_modifiedDatas.Clear();
        }

        private static NodePathfindingData GetResettedData()
        {
            var data = new NodePathfindingData();
            data.parentId = PathNodeId.InvalidId;
            return data;
        }

        private PathResult BuildPath(PathNodeId root)
        {
            var list = new List<PathNodeId>();

            PathNodeId n = root;
            var data = GetData(n);
            float totalWeight = data.g;
            float distance = 0f;

            while (data.hasParent)
            {
                list.Add(n);

                distance += NodeReader.GetDistanceBetweenNodes(n, data.parentId);

                n = data.parentId;
                data = GetData(n);
            }

            list.Add(n);

            list.Reverse();

            return new PathResult
            {
                IsSuccess = true,
                Nodes = list,
                TotalWeight = totalWeight,
                Distance = distance,
            };
        }

        private static PathNodeId MinFast(SortedSet<PathNodeId> sortedSet)
        {
            object node = s_rootField.GetValue(sortedSet);
            object minNode = node;
            while (node != null)
            {
                minNode = node;
                node = s_leftNodeField.GetValue(node);
            }

            return (PathNodeId)s_itemNodeField.GetValue(minNode); // note: this will allocate memory
        }

        private static void AddOrThrow(SortedSet<PathNodeId> sortedSet, PathNodeId element)
        {
            if (!sortedSet.Add(element))
                throw new InvalidOperationException("Failed to add element to SortedSet");
        }

        private static void RemoveOrThrow(SortedSet<PathNodeId> sortedSet, PathNodeId element)
        {
            if (!sortedSet.Remove(element))
                throw new InvalidOperationException("Failed to remove element from SortedSet");
        }

        public static List<Vector3> CalculateFullNavMeshPath(
            Vector3 source, Vector3 dest, int areaMask)
        {
            var allCorners = new List<Vector3>();
            var path = new NavMeshPath();
            Vector3 nextSource = source;
            int i = 0;

            while (true)
            {
                if (!NavMesh.CalculatePath(nextSource, dest, areaMask, path))
                    break;

                Vector3[] pathCorners = path.corners;
                allCorners.AddRange(pathCorners);

                if (path.status == NavMeshPathStatus.PathComplete)
                    break;

                if (pathCorners.Length <= 1) // partial path that leads nowhere ?
                    break;

                nextSource = pathCorners.Last();

                i++;
                if (i >= 50)
                    break;
            }

            return allCorners;
        }
    }
}
