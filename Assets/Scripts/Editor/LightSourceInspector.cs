using SanAndreasUnity.Behaviours.World;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    [CustomEditor(typeof(LightSource))]
    public class LightSourceInspector : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector ();

            GUILayout.Space (10);
            GUILayout.Label("Light info:");
            GUILayout.Space (10);

            var lightSource = (LightSource) this.target;

            EditorUtils.DrawFieldsInInspector(lightSource.LightInfo, 0, false);
        }

    }
}