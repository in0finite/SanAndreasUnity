using SanAndreasUnity.Behaviours.World;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    [CustomEditor(typeof(StaticGeometry))]
    public class StaticGeometryInspector : UnityEditor.Editor
    {
        private Vector2 _scrollViewPos;

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector ();

            GUILayout.Space (10);
            GUILayout.Label("Info:");
            GUILayout.Space (10);

            var staticGeometry = (StaticGeometry) this.target;

            _scrollViewPos = EditorGUILayout.BeginScrollView(_scrollViewPos, GUILayout.MinHeight(350));

            EditorUtils.DrawPropertiesInInspector(staticGeometry, 1, false);

            GUILayout.Space (10);
            GUILayout.Label("Object definition:");
            GUILayout.Space (10);

            if (staticGeometry.ObjectDefinition != null)
                EditorUtils.DrawFieldsAndPropertiesInInspector(staticGeometry.ObjectDefinition, 0);

            GUILayout.Space (10);
            GUILayout.Label("Placement info:");
            GUILayout.Space (10);

            if (staticGeometry.Instance != null)
                EditorUtils.DrawFieldsAndPropertiesInInspector(staticGeometry.Instance, 0);

            EditorGUILayout.EndScrollView();
        }

    }
}
