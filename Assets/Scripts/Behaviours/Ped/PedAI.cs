using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Paths;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours
{
    public enum PedAction
    {
        WalkingAround,
        Chasing,
        Escaping
    }

    public class PedAI : MonoBehaviour
    {
        private static readonly List<PedAI> s_allPedAIs = new List<PedAI>();
        public static IReadOnlyList<PedAI> AllPedAIs => s_allPedAIs;

        private static bool s_subscribedToPedOnDamageEvent = false;

        [SerializeField] private Vector3 currentNodePos;
        [SerializeField] private Vector3 targetNodePos;
        [SerializeField] private Vector2 targetNodeOffset; // Adding random offset to prevent peds to have the exact destination

        public PedAction Action;

        /// <summary>
        /// The node where the Ped starts
        /// </summary>
        public PathNode CurrentNode;

        /// <summary>
        /// The node the Ped is targeting
        /// </summary>
        public PathNode TargetNode;

        /// <summary>
        /// The ped the Ped is chasing
        /// </summary>
        public Ped TargetPed;

        public Ped MyPed { get; private set; }


        private void Awake()
        {
            this.MyPed = this.GetComponentOrThrow<Ped>();

            if (!s_subscribedToPedOnDamageEvent)
            {
                s_subscribedToPedOnDamageEvent = true;
                Ped.onDamaged += OnPedDamaged;
            }
        }

        private void OnEnable()
        {
            s_allPedAIs.Add(this);
        }

        private void OnDisable()
        {
            s_allPedAIs.Remove(this);
        }

        private static void OnPedDamaged(Ped hitPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            var hitPedAi = hitPed.GetComponent<PedAI>();
            if (null == hitPedAi)
                return;

            if (hitPed.PedDef != null &&
                (hitPed.PedDef.DefaultType == PedestrianType.Criminal ||
                hitPed.PedDef.DefaultType == PedestrianType.Cop ||
                hitPed.PedDef.DefaultType.IsGangMember()))
            {
                hitPedAi.TargetPed = dmgInfo.GetAttackerPed();
                hitPedAi.Action = PedAction.Chasing;
            }
            else
                hitPedAi.Action = PedAction.Escaping;
        }

        // Update is called once per frame
        void Update()
        {
            this.MyPed.ResetInput();
            if (NetStatus.IsServer)
            {
                switch (this.Action)
                {
                    case PedAction.WalkingAround:
                        currentNodePos = CurrentNode.Position;
                        targetNodePos = TargetNode.Position;
                        if (Vector2.Distance(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(targetNodePos.x, targetNodePos.z)) < 3)
                        {
                            PathNode previousNode = CurrentNode;
                            CurrentNode = TargetNode;
                            TargetNode = GetNextPathNode(previousNode, CurrentNode);
                            targetNodeOffset = new Vector2(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 2));
                        }
                        this.MyPed.IsWalkOn = true;
                        Vector3 dest = targetNodePos + new Vector3(targetNodeOffset.x, 0, targetNodeOffset.y);
                        this.MyPed.Movement = (dest - this.MyPed.transform.position).normalized;
                        this.MyPed.Heading = this.MyPed.Movement;
                        break;
                    case PedAction.Chasing:
                        if (this.TargetPed != null)
                        {
                            if (Vector3.Distance(TargetPed.transform.position, this.MyPed.transform.position) < 10f)
                            {
                                this.MyPed.AimDirection = (TargetPed.transform.position - this.MyPed.transform.position).normalized;
                                this.MyPed.IsAimOn = true;
                                this.MyPed.IsFireOn = true;
                            }
                            else
                            {
                                this.MyPed.IsRunOn = true;
                                this.MyPed.Movement = (TargetPed.transform.position - this.MyPed.transform.position).normalized;
                                this.MyPed.Heading = this.MyPed.Movement;
                            }
                        }
                        else // The target is dead/disconnected
                        {
                            this.Action = PedAction.WalkingAround;
                        }
                        break;
                    case PedAction.Escaping:
                        currentNodePos = CurrentNode.Position;
                        targetNodePos = TargetNode.Position;
                        if (Vector2.Distance(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(TargetNode.Position.x, TargetNode.Position.z)) < 1f)
                        {
                            PathNode previousNode = CurrentNode;
                            CurrentNode = TargetNode;
                            TargetNode = GetNextPathNode(CurrentNode, previousNode);
                        }
                        this.MyPed.IsSprintOn = true;
                        this.MyPed.Movement = (TargetNode.Position - this.MyPed.transform.position).normalized;
                        this.MyPed.Heading = this.MyPed.Movement;
                        break;
                }
            }
        }

        public static PathNode GetNextPathNode(PathNode origin, PathNode current)
        {
            List<int> areas = NodeFile.GetAreaNeighborhood(origin.AreaID);
            NodeFile file = NodeReader.NodeFiles.First(f => f.Id == origin.AreaID);
            List<PathNode> possibilities = new List<PathNode>();
            for (int i = 0; i < current.LinkCount; i++)
            {
                int linkArrayIndex = current.BaseLinkID + i;
                NodeFile nf = NodeReader.NodeFiles.Single(nf2 => nf2.Id == file.NodeLinks[linkArrayIndex].AreaID);
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
                //No possibilities found, returning to origin
                return origin;
            }

        }
    }

}