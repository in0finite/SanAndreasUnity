using System.Linq;
using SanAndreasUnity.Behaviours.Peds.AI;
using SanAndreasUnity.Importing.Paths;
using UGameCore.Utilities;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    [CustomEditor(typeof(PedAI))]
    public class PedAIInspector : UnityEditor.Editor
    {
        private static bool _foldoutCurrent = true;
        private static bool _foldoutDestination = true;
        private static bool _foldoutLinkedCurrent = true;
        private static bool _foldoutLinkedDestination = true;


        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector ();

            var pedAI = (PedAI) this.target;

            GUILayout.Space (10);

            if (pedAI.CurrentState is IPathMovementState pathMovementState)
            {
                if (pathMovementState.PathMovementData.currentNode.HasValue)
                    DrawForNode(pathMovementState.PathMovementData.currentNode.Value, "Current node", ref _foldoutCurrent, true, ref _foldoutLinkedCurrent);
                if (pathMovementState.PathMovementData.destinationNode.HasValue)
                    DrawForNode(pathMovementState.PathMovementData.destinationNode.Value, "Destination node", ref _foldoutDestination, true, ref _foldoutLinkedDestination);
            }
        }

        void DrawForNode(PathNode node, string labelText, ref bool foldout, bool showLinkedNodes, ref bool foldoutLinked)
        {
            if (!string.IsNullOrWhiteSpace(labelText))
                foldout = EditorGUILayout.Foldout(foldout, labelText, true);

            if (!foldout)
                return;

            if (GUILayout.Button("Goto"))
                GoTo(node);

            EditorUtils.DrawFieldsAndPropertiesInInspector(node, 0);

            if (showLinkedNodes)
            {
                foldoutLinked = EditorGUILayout.Foldout(foldoutLinked, "Linked nodes", true);
                if (foldoutLinked)
                {
                    foreach (var linkedNode in NodeReader.GetAllLinkedNodes(node))
                    {
                        bool f = true;
                        bool fLinked = false;
                        DrawForNode(linkedNode, "", ref f, false, ref fLinked);
                    }
                }
            }

            GUILayout.Space(5);
        }

        void GoTo(PathNode node)
        {
            EditorUtils.FocusSceneViewsOnPosition(node.Position);
        }
    }
}
