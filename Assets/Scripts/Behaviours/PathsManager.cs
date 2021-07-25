using Assets.Scripts.Importing.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class PathsManager : MonoBehaviour
    {
        private bool pedSpawned;
        private int nbr;
        private GameObject pedPathNodesGO;
        void Awake()
        {
            pedSpawned = false;
            nbr = 0;
        }

        private void Update()
        {
            if(!pedSpawned && Loader.HasLoaded)
            {
                if (NodeReader.Nodes != null)
                {
                    Debug.Log("Spawning peds");
                    if (pedPathNodesGO == null)
                    {
                        Debug.LogError("PedPathNodes not yet created, creating one ...");
                        pedPathNodesGO = new GameObject("PedPathNodes");
                    }
                    foreach (NodeFile file in NodeReader.Nodes.Where(f => f.Id == 53 || f.Id == 52))
                    {
                        foreach (PathNode node in file.PathNodes.Where(pn => pn.IsPed))
                        {
                            GameObject tmp = pedPathNodesGO.CreateChild($"Path_{node.AreaID}_{node.NodeID}");
                            tmp.transform.position = node.Position;
                            tmp.AddComponent<MeshRenderer>();
                            TextMesh tm = tmp.AddComponent<TextMesh>();
                            tm.text = $"Path_{node.AreaID}_{node.NodeID}\r\n" +
                                $"AreaID = {node.AreaID}\r\n" +
                                $"NodeID = {node.NodeID}\r\n" +
                                $"Link count = {node.LinkCount}\r\n";
                            tm.characterSize = 0.3f;
                                
                            for (int i = 0; i < node.LinkCount-1; i++)
                            {
                                int linkArrayIndex = node.BaseLinkID + i;
                                NodeFile nf = NodeReader.Nodes.Single(nf2 => nf2.Id == file.NodeLinks[linkArrayIndex].AreaID);
                                PathNode targetNode = nf.PathNodes.ElementAt(file.NodeLinks[linkArrayIndex].NodeID);
                                    
                                GameObject line = tmp.CreateChild("link_" + i);
                                LineRenderer lr = line.AddComponent<LineRenderer>();
                                lr.positionCount = 2;
                                lr.SetPositions(new Vector3[] { node.Position, targetNode.Position });
                            }
                        }
                    }
                    pedSpawned = true;
                }
            }
        }

        public static bool SpawnPedWithAI(int nbr, Vector2 targetZone, float radius)
        {
            List<PathNode> nearNodes = new List<PathNode>();

            foreach (NodeFile file in NodeReader.Nodes)
            {
                foreach (PathNode node in file.PathNodes.Where(pn => pn.IsPed && Vector2.Distance(new Vector2(pn.Position.x, pn.Position.z), targetZone) < radius))
                {
                    nearNodes.Add(node);
                }
            }

            if(nearNodes.Count == 0)
            {
                Debug.LogError("Unable to find near nodes in the radius at position " + targetZone);
                return false;
            }
            else
            {
                for (int i = 0; i < nbr; i++)
                {
                    PathNode pedNode = nearNodes.ElementAt(UnityEngine.Random.Range(0, nearNodes.Count - 1));
                    Ped newPed = Ped.SpawnPed(Ped.RandomPedId, pedNode.Position, Quaternion.identity, false);
                    Ped_AI ai = newPed.gameObject.AddComponent<Ped_AI>();
                    ai.CurrentNode = pedNode;
                    ai.TargetNode = pedNode;
                    nearNodes.Remove(pedNode);
                    //Debug.Log($"Ped spawned at node {pedNode.AreaID}_{pedNode.NodeID} position {pedNode.Position}");
                }
                return true;
            }
        }
        
        public static PathNode GetNextPathNode(PathNode origin, PathNode current)
        {
            NodeFile file = NodeReader.Nodes.Where(f => f.Id == 53).First();
            List<PathNode> possibilities = new List<PathNode>();
            for (int i = 0; i < current.LinkCount; i++)
            {
                int linkArrayIndex = current.BaseLinkID + i;
                NodeFile nf = NodeReader.Nodes.Single(nf2 => nf2.Id == file.NodeLinks[linkArrayIndex].AreaID);
                PathNode target = nf.PathNodes.ElementAt(file.NodeLinks[linkArrayIndex].NodeID);
                if (!target.Equals(origin))
                    possibilities.Add(target);
            }

            if (possibilities.Count > 0)
            {
                return possibilities.ElementAt(UnityEngine.Random.Range(0, possibilities.Count - 1));
            }
            else
            {
                Debug.Log("No possibilites found, returning to origin");
                return origin;
            }

        }
        /*
        public static List<string> GetLinkedNode(int areaID, string id)
        {
            PathNode node = NodeReader.Nodes[areaID].Find(e => e.id.Equals(id));
            if (node.links != null)
            {
                if (node.links.Count > 0)
                {
                    List<string> result = new List<string>();
                    foreach (LinkInfo li in node.links)
                    {
                        result.Add("Linked Node: " + li.targetNode.id + " and linked NaviNode: " + li.naviNodeLink.id);
                    }
                    return result;
                }
            }
            return null;
        }*/
    }
}