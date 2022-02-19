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

            public float TimeElapsed { get; set; }
        }

        public class NodeComparer : IComparer<PathNodeId>
        {
            private readonly NodePathfindingData[][] m_nodePathfindingDatas;

            public NodeComparer(NodePathfindingData[][] nodePathfindingDatas)
            {
                m_nodePathfindingDatas = nodePathfindingDatas;
            }

            int IComparer<PathNodeId>.Compare(PathNodeId a, PathNodeId b)
            {
                return m_nodePathfindingDatas[a.AreaID][a.NodeID].f.CompareTo(
                    m_nodePathfindingDatas[b.AreaID][b.NodeID].f);
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

            var closedList = new HashSet<PathNodeId>();
            var openList = new SortedSet<PathNodeId>(new NodeComparer(m_nodePathfindingDatas));

            PathNodeId startId = closestSourceNode.Id;
            PathNodeId targetId = closestDestinationNode.Id;

            var startData = GetData(startId);
            startData.f = startData.g + CalculateHeuristic(startId, targetId);
            SetData(startId, startData);

            openList.Add(startId);

            while (openList.Count > 0)
            {
                PathNodeId idN = openList.Min; // TODO: optimize ; call Remove() ;
                
                if (idN.Equals(targetId))
                {
                    pathResult.Nodes = BuildPath(idN);
                    pathResult.IsSuccess = true;
                    break;
                }

                var nodeN = NodeReader.GetNodeById(idN);
                var areaN = NodeReader.GetAreaById(idN.AreaID);
                var dataN = GetData(idN);

                for (int i = 0; i < nodeN.LinkCount; i++)
                {
                    NodeLink link = areaN.NodeLinks[nodeN.BaseLinkID + i];

                    PathNodeId idM = link.PathNodeId;
                    var dataM = GetData(idM);

                    float totalWeight = dataN.g + link.Length;

                    if (!openList.Contains(idM) && !closedList.Contains(idM))
                    {
                        dataM.parentId = idN;
                        dataM.hasParent = true;
                        dataM.g = totalWeight;
                        dataM.f = dataM.g + CalculateHeuristic(idM, targetId);
                        SetData(idM, dataM);

                        openList.Add(idM);
                    }
                    else
                    {
                        if (totalWeight < dataM.g)
                        {
                            dataM.parentId = idN;
                            dataM.hasParent = true;
                            dataM.g = totalWeight;
                            dataM.f = dataM.g + CalculateHeuristic(idM, targetId);
                            SetData(idM, dataM);

                            if (closedList.Contains(idM))
                            {
                                closedList.Remove(idM);
                                openList.Add(idM);
                            }
                        }
                    }
                }

                openList.Remove(idN);
                closedList.Add(idN);
            }

            int numModifiedDatas = m_modifiedDatas.Count;
            RestoreModifiedDatas();

            pathResult.TimeElapsed = (float)stopwatch.Elapsed.TotalSeconds;

            UnityEngine.Debug.Log($"Path finding finished: time {pathResult.TimeElapsed}, num nodes {pathResult.Nodes?.Count ?? 0}, numModifiedDatas {numModifiedDatas}");

            return pathResult;
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
            return Vector3.Distance(
                NodeReader.GetNodeById(source).Position,
                NodeReader.GetNodeById(destination).Position);
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

        private List<PathNodeId> BuildPath(PathNodeId root)
        {
            var list = new List<PathNodeId>();

            PathNodeId n = root;
            var data = GetData(n);

            while (data.hasParent)
            {
                list.Add(n);

                n = data.parentId;
                data = GetData(n);
            }

            list.Add(n);

            list.Reverse();

            return list;
        }
    }
}
