using Assets.Scripts.Importing.Paths;
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
    public class Ped_AI : MonoBehaviour
    {
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

        // Use this for initialization
        void Start()
        {
            this.MyPed = this.GetComponentOrLogError<Ped>();
            Ped.onDamaged += Ped_onDamaged;
        }

        private void Ped_onDamaged(Ped hitPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            if(hitPed.Equals(this.MyPed))
            {
                if (this.MyPed.PedDef.DefaultType == Importing.Items.Definitions.PedestrianType.Criminal ||
                    this.MyPed.PedDef.DefaultType == Importing.Items.Definitions.PedestrianType.Cop ||
                    this.MyPed.PedDef.DefaultType == Importing.Items.Definitions.PedestrianType.GangMember)
                {
                    TargetPed = (Ped)dmgInfo.attacker;
                    this.Action = PedAction.Chasing;
                }
                else
                    this.Action = PedAction.Escaping;
            }
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
                            TargetNode = PathsManager.GetNextPathNode(previousNode, CurrentNode);
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
                            TargetNode = PathsManager.GetNextPathNode(CurrentNode, previousNode);
                        }
                        this.MyPed.IsSprintOn = true;
                        this.MyPed.Movement = (TargetNode.Position - this.MyPed.transform.position).normalized;
                        this.MyPed.Heading = this.MyPed.Movement;
                        break;
                }
            }
        }
    }

}