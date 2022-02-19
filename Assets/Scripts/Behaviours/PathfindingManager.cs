using SanAndreasUnity.Importing.Paths;
using SanAndreasUnity.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

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

        private NodePathfindingData[][] m_nodePathfindingDatas = null;

        private readonly List<PathNodeId> m_modifiedDatas = new List<PathNodeId>();



        protected override void OnSingletonStart()
        {
            this.BackgroundJobRunner.EnsureBackgroundThreadStarted();
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
            var stopwatch = Stopwatch.StartNew();
            PathResult pathResult = new PathResult { IsSuccess = false };

            // find closest node of source position

            /*var closestSourceEdge = NodeReader.GetAreasInRadius(sourcePos, 300f)
                .SelectMany(_ => _.PedNodes)
                .Where(_ => Vector3.Distance(_.Position, sourcePos) < 1000f)
                .SelectMany(_ => NodeReader.GetAllLinkedNodes(_).Select(ln => (n1: _, n2: ln)))
                .MinBy(_ => MathUtils.DistanceFromPointToLineSegment(sourcePos, _.n1.Position, _.n2.Position), default);*/

            var closestSourceNode = NodeReader.GetAreasInRadius(sourcePos, 300f)
                .SelectMany(_ => _.PedNodes)
                .MinBy(_ => Vector3.Distance(_.Position, sourcePos), PathNode.InvalidNode);

            if (closestSourceNode.Equals(PathNode.InvalidNode))
                return pathResult;

            // find closest node of destination position
            var closestDestinationNode = NodeReader.GetAreasInRadius(destinationPos, 300f)
                .SelectMany(_ => _.PedNodes)
                .MinBy(_ => Vector3.Distance(_.Position, destinationPos), PathNode.InvalidNode);

            if (closestDestinationNode.Equals(PathNode.InvalidNode))
                return pathResult;




            this.RestoreModifiedDatas();

            if (FindPathFromNodeToNode(closestSourceNode.Id, closestDestinationNode.Id))
            {
                pathResult = BuildPath(closestDestinationNode.Id);
            }

            int numModifiedDatas = m_modifiedDatas.Count;
            RestoreModifiedDatas();

            pathResult.TimeElapsed = (float)stopwatch.Elapsed.TotalSeconds;

            UnityEngine.Debug.Log($"Path finding finished: time {pathResult.TimeElapsed * 1000} ms, num nodes {pathResult.Nodes?.Count ?? 0}, numModifiedDatas {numModifiedDatas}, g {pathResult.TotalWeight}, distance {pathResult.Distance}");

            return pathResult;
        }

        private bool FindPathFromNodeToNode(PathNodeId startId, PathNodeId targetId)
        {
            var closedList = new HashSet<PathNodeId>();
            var openList = new SortedSet<PathNodeId>(new NodeComparer(m_nodePathfindingDatas));

            var startData = GetData(startId);
            startData.f = startData.g + CalculateHeuristic(startId, targetId);
            SetData(startId, startData);

            openList.Add(startId);

            while (openList.Count > 0)
            {
                PathNodeId idN = openList.Min; // TODO: optimize ; call Remove() ;

                if (idN.Equals(targetId))
                {
                    return true;
                }

                openList.Remove(idN);
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
                        dataM.g = dataN.g + link.Length;
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
                        float gNew = dataN.g + link.Length;
                        float hNew = CalculateHeuristic(idM, targetId);
                        float fNew = gNew + hNew;

                        var dataM = GetData(idM);

                        if (dataM.f > fNew)
                        {
                            // update

                            openList.Remove(idM); // first remove with old data

                            dataM.g = gNew;
                            dataM.f = fNew;
                            dataM.parentId = idN;
                            dataM.hasParent = true;

                            SetData(idM, dataM);

                            openList.Add(idM); // now add with new data
                        }
                    }
                    else
                    {
                        var dataM = GetResettedData();
                        dataM.g = dataN.g + link.Length;
                        float h = CalculateHeuristic(idM, targetId);
                        dataM.f = dataM.g + h;
                        dataM.parentId = idN;
                        dataM.hasParent = true;
                        SetData(idM, dataM);
                        openList.Add(idM); // do it after setting data
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
    }
}
