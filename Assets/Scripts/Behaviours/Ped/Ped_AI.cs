using Assets.Scripts.Importing.Paths;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
    public class Ped_AI : MonoBehaviour
    {
        [SerializeField] private Vector3 currentNodePos;
        [SerializeField] private Vector3 targetNodePos;
        public PathNode CurrentNode;
        public PathNode TargetNode;
        public Ped TargetPed;
        
        private TextMesh textMesh;

        public Ped MyPed { get; private set; }

        // Use this for initialization
        void Start()
        {
            this.MyPed = this.GetComponentOrLogError<Ped>();
            Ped.onDamaged += Ped_onDamaged;
            /*
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.characterSize = 0.3f;
            textMesh.anchor = TextAnchor.LowerLeft;
            */
        }

        private void Ped_onDamaged(Ped hitPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            if(hitPed.Equals(this.MyPed))
            {
                TargetPed = (Ped)dmgInfo.attacker;
            }
        }

        // Update is called once per frame
        void Update()
        {
            this.MyPed.ResetInput();
            if(TargetPed == null) // Not chasing player, just walking
            {
                currentNodePos = CurrentNode.Position;
                targetNodePos = TargetNode.Position;
                if (Vector2.Distance(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(TargetNode.Position.x, TargetNode.Position.z)) < 1f)
                {
                    PathNode tmp = TargetNode;
                    TargetNode = PathsManager.GetNextPathNode(CurrentNode, TargetNode);
                    CurrentNode = tmp;
                }
                this.MyPed.IsWalkOn = true;
                this.MyPed.Movement = (TargetNode.Position - this.MyPed.transform.position).normalized;
                this.MyPed.Heading = this.MyPed.Movement;
                //textMesh.color = Color.green;
                //textMesh.text = "Distance = " + Vector2.Distance(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(TargetNode.Position.x, TargetNode.Position.z)) + "\r\n" +
                //    "Current node = " + CurrentNode.AreaID + "_" + CurrentNode.NodeID + "\r\n" +
                //    "Target node = " + TargetNode.AreaID + "_" + TargetNode.NodeID;
            }
            else // Chasing someone
            {
                if(Vector3.Distance(TargetPed.transform.position, this.MyPed.transform.position) < 1f)
                {
                    this.MyPed.AimDirection = (TargetPed.transform.position - this.MyPed.transform.position).normalized;
                    this.MyPed.IsFireOn = true;
                }
                else
                {
                    this.MyPed.IsRunOn = true;
                    this.MyPed.Movement = (TargetPed.transform.position - this.MyPed.transform.position).normalized;
                    this.MyPed.Heading = this.MyPed.Movement;
                }
                //textMesh.color = Color.red;
                //textMesh.text = "Distance = " + Vector3.Distance(TargetPed.transform.position, this.MyPed.transform.position) + "\r\n"+
                //    "Target ped = " + TargetPed.name;
            }
        }
    }

}