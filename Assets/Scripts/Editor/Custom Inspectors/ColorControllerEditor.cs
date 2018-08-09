using UnityEngine;
using UnityEditor;
using System.Linq;

public class ColorControllerEditor
{
    public static void OnInspectorGUI(ColorController target, SerializedObject so)
    {
        so.UpdateIfRequiredOrScript();

        /*var dictionary = so.FindProperty("_mapColor");

        if (target.serializedMapColor != null && target.serializedMapColor != null)
        {
            //if (GUILayout.Button("New value"))
            //    target.serializedMapColor.Add(Color.white, 0);

            EditorGUILayout.PropertyField(dictionary);

            /*for (int i = 0; i < target.serializedMapColor.Count; ++i)
            {
                var element = dictionary.GetFixedBufferElementAtIndex(i);

                GUILayout.BeginHorizontal();

                Color key = (Color)(element.FindPropertyRelative("key").objectReferenceValue as object);
                element.FindPropertyRelative("key").objectReferenceValue = EditorGUILayout.ColorField(key) as object as Object;

                float value = (float)(element.FindPropertyRelative("value").objectReferenceValue as object);
                element.FindPropertyRelative("value").objectReferenceValue = EditorGUILayout.FloatField(value) as object as Object;

                if (GUILayout.Button("Remove value"))
                    target.serializedMapColor.Remove(target.serializedMapColor.Keys.ElementAt(i));

                GUILayout.EndHorizontal();
            }
        }

        so.ApplyModifiedProperties();*/

        if (GUILayout.Button("Re-init all values"))
        {
            target._mapColor.Clear();

            foreach (var kv in ZHelpers.mapColors)
                target._mapColor.Add(kv.Value, 0);
        }

        //myTarget.serializedMapColor = EditorGUILayout.IntField("Experience", myTarget.experience);
        //EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
    }
}

[CustomEditor(typeof(StarController))]
public class StarControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ColorControllerEditor.OnInspectorGUI((ColorController)target, base.serializedObject);
    }
}

[CustomEditor(typeof(WeatherController))]
public class WeatherControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ColorControllerEditor.OnInspectorGUI((ColorController)target, base.serializedObject);
    }
}