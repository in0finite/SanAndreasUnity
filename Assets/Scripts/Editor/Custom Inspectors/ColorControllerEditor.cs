using UnityEngine;
using UnityEditor;
using System.Linq;

public class ColorControllerEditor
{
    public static void OnInspectorGUI(ColorController target, SerializedObject so)
    {
        if (GUILayout.Button("Re-init all values"))
        {
            target._mapColor.Clear();

            foreach (var kv in ZHelpers.mapColors)
                target._mapColor.Add(kv.Value, 0);
        }
    }
}

[CustomEditor(typeof(StarController))]
public class StarControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ColorControllerEditor.OnInspectorGUI((ColorController)target, serializedObject);
    }
}

[CustomEditor(typeof(WeatherController))]
public class WeatherControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ColorControllerEditor.OnInspectorGUI((ColorController)target, serializedObject);
    }
}