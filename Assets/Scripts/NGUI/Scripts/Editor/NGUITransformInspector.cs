//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform))]
public class NGUITransformInspector : Editor
{
    /// <summary>
    /// Draw the inspector widget.
    /// </summary>

    public override void OnInspectorGUI()
    {
        Transform trans = target as Transform;
        EditorGUIUtility.LookLikeControls(15f);

        Vector3 pos;
        Vector3 rot;
        Vector3 scale;

        // Position
        EditorGUILayout.BeginHorizontal();
        {
            if (DrawButton("P", "Reset Position", IsResetPositionValid(trans), 20f))
            {
                NGUIEditorTools.RegisterUndo("Reset Position", trans);
                trans.localPosition = Vector3.zero;
            }
            pos = DrawVector3(trans.localPosition);
        }
        EditorGUILayout.EndHorizontal();

        // Rotation
        EditorGUILayout.BeginHorizontal();
        {
            if (DrawButton("R", "Reset Rotation", IsResetRotationValid(trans), 20f))
            {
                NGUIEditorTools.RegisterUndo("Reset Rotation", trans);
                trans.localEulerAngles = Vector3.zero;
            }
            rot = DrawVector3(trans.localEulerAngles);
        }
        EditorGUILayout.EndHorizontal();

        // Scale
        EditorGUILayout.BeginHorizontal();
        {
            if (DrawButton("S", "Reset Scale", IsResetScaleValid(trans), 20f))
            {
                NGUIEditorTools.RegisterUndo("Reset Scale", trans);
                trans.localScale = Vector3.one;
            }
            scale = DrawVector3(trans.localScale);
        }
        EditorGUILayout.EndHorizontal();

        // If something changes, set the transform values
        if (GUI.changed)
        {
            NGUIEditorTools.RegisterUndo("Transform Change", trans);
            trans.localPosition = Validate(pos);
            trans.localEulerAngles = Validate(rot);
            trans.localScale = Validate(scale);
        }
    }

    /// <summary>
    /// Helper function that draws a button in an enabled or disabled state.
    /// </summary>

    private static bool DrawButton(string title, string tooltip, bool enabled, float width)
    {
        if (enabled)
        {
            // Draw a regular button
            return GUILayout.Button(new GUIContent(title, tooltip), GUILayout.Width(width));
        }
        else
        {
            // Button should be disabled -- draw it darkened and ignore its return value
            Color color = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.25f);
            GUILayout.Button(new GUIContent(title, tooltip), GUILayout.Width(width));
            GUI.color = color;
            return false;
        }
    }

    /// <summary>
    /// Helper function that draws a field of 3 floats.
    /// </summary>

    private static Vector3 DrawVector3(Vector3 value)
    {
        GUILayoutOption opt = GUILayout.MinWidth(30f);
        value.x = EditorGUILayout.FloatField("X", value.x, opt);
        value.y = EditorGUILayout.FloatField("Y", value.y, opt);
        value.z = EditorGUILayout.FloatField("Z", value.z, opt);
        return value;
    }

    /// <summary>
    /// Helper function that determines whether its worth it to show the reset position button.
    /// </summary>

    private static bool IsResetPositionValid(Transform targetTransform)
    {
        Vector3 v = targetTransform.localPosition;
        return (v.x != 0f || v.y != 0f || v.z != 0f);
    }

    /// <summary>
    /// Helper function that determines whether its worth it to show the reset rotation button.
    /// </summary>

    private static bool IsResetRotationValid(Transform targetTransform)
    {
        Vector3 v = targetTransform.localEulerAngles;
        return (v.x != 0f || v.y != 0f || v.z != 0f);
    }

    /// <summary>
    /// Helper function that determines whether its worth it to show the reset scale button.
    /// </summary>

    private static bool IsResetScaleValid(Transform targetTransform)
    {
        Vector3 v = targetTransform.localScale;
        return (v.x != 1f || v.y != 1f || v.z != 1f);
    }

    /// <summary>
    /// Helper function that removes not-a-number values from the vector.
    /// </summary>

    private static Vector3 Validate(Vector3 vector)
    {
        vector.x = float.IsNaN(vector.x) ? 0f : vector.x;
        vector.y = float.IsNaN(vector.y) ? 0f : vector.y;
        vector.z = float.IsNaN(vector.z) ? 0f : vector.z;
        return vector;
    }
}