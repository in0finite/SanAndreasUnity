using System.Linq;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Importing.Paths;
using SanAndreasUnity.Utilities;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    [CustomEditor(typeof(PedAI))]
    public class PedAIInspector : UnityEditor.Editor
    {
        private static bool _foldoutCurrent = true;
        private static bool _foldoutTarget = true;
        private static bool _foldoutAdjacentCurrent = true;
        private static bool _foldoutAdjacentTarget = true;


        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector ();

            var pedAI = (PedAI) this.target;

            GUILayout.Space (10);

            DrawForNode(pedAI.CurrentNode, "Current node:", ref _foldoutCurrent, true, ref _foldoutAdjacentCurrent);
            DrawForNode(pedAI.TargetNode, "Target node:", ref _foldoutTarget, true, ref _foldoutAdjacentTarget);
        }

        void DrawForNode(PathNode node, string labelText, ref bool foldout, bool showAdjacentNodes, ref bool foldoutAdjacent)
        {
            if (!string.IsNullOrWhiteSpace(labelText))
                foldout = EditorGUILayout.Foldout(foldout, labelText, true);

            if (!foldout)
                return;

            if (GUILayout.Button("Goto"))
                GoTo(node);

            EditorUtils.DrawFieldsAndPropertiesInInspector(node, 0);

            if (showAdjacentNodes)
            {
                foldoutAdjacent = EditorGUILayout.Foldout(foldoutAdjacent, "Adjacent nodes:", true);
                if (foldoutAdjacent)
                {
                    foreach (var adjacentNode in NodeReader.GetAllLinkedNodes(node))
                    {
                        bool f = true;
                        bool fAdjacent = false;
                        DrawForNode(adjacentNode, "", ref f, false, ref fAdjacent);
                    }
                }
            }

            GUILayout.Space(5);
        }

        void GoTo(PathNode node)
        {
            SceneView.sceneViews.Cast<SceneView>().ForEach(s => s.LookAt(node.Position));
        }
    }
}
