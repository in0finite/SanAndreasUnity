using SanAndreasUnity.Behaviours.World;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    [CustomEditor(typeof(WaterFaceInfo))]
    public class WaterFaceInfoInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            GUILayout.Space(10);
            GUILayout.Label("Info:");
            GUILayout.Space(10);

            var waterFaceInfo = (WaterFaceInfo)this.target;

            if (waterFaceInfo.WaterFace != null)
            {
                EditorUtils.DrawFieldsAndPropertiesInInspector(waterFaceInfo.WaterFace, 0);

                foreach (var vertex in waterFaceInfo.WaterFace.Vertices)
                {
                    GUILayout.Space(10);
                    EditorUtils.DrawFieldsAndPropertiesInInspector(vertex, 0);
                }
            }
        }
    }
}
