using Assets.Scripts.Importing.Paths;
using UnityEngine;
using System.Collections;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
    public class Ped_AI : MonoBehaviour
    {
        [SerializeField] private Vector3 currentNodePos;
        [SerializeField] private Vector3 targetNodePos;
        public PathNode CurrentNode;
        public PathNode TargetNode;
        
        private TextMesh textMesh;

        public Ped MyPed { get; private set; }

        // Use this for initialization
        void Start()
        {
            this.MyPed = this.GetComponentOrLogError<Ped>();
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.characterSize = 0.3f;
            textMesh.color = Color.red;
            textMesh.anchor = TextAnchor.LowerLeft;
        }

        // Update is called once per frame
        void Update()
        {
            this.MyPed.ResetInput();
            currentNodePos = CurrentNode.Position;
            targetNodePos = TargetNode.Position;
            if (Vector2.Distance(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(TargetNode.Position.x, TargetNode.Position.z)) < 1)
            {
                PathNode tmp = TargetNode;
                TargetNode = PathsManager.GetNextPathNode(CurrentNode, TargetNode);
                CurrentNode = tmp;
            }
            this.MyPed.IsWalkOn = true;
            this.MyPed.Movement = (TargetNode.Position - CurrentNode.Position).normalized;
            this.MyPed.Heading = (TargetNode.Position - CurrentNode.Position).normalized;
            textMesh.text = "Distance = " + Vector2.Distance(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(TargetNode.Position.x, TargetNode.Position.z)) + "\r\n" +
                "Current node = " + CurrentNode.AreaID + "_" + CurrentNode.NodeID + "\r\n" +
                "Target node = " + TargetNode.AreaID + "_" + TargetNode.NodeID;
        }
    }

}