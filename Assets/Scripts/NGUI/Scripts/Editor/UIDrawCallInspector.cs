//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to view UIDrawCalls.
/// </summary>
[CustomEditor(typeof(UIDrawCall))]
public class UIDrawCallInspector : Editor
{
    /// <summary>
    /// Draw the inspector widget.
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
        {
            UIDrawCall dc = target as UIDrawCall;

            UIPanel[] panels = (UIPanel[])Component.FindObjectsOfType(typeof(UIPanel));

            foreach (UIPanel p in panels)
            {
                if (p.drawCalls.Contains(dc))
                {
                    EditorGUILayout.LabelField("Owner Panel", NGUITools.GetHierarchy(p.gameObject));
                    EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());
                    return;
                }
            }
            if (Event.current.type == EventType.Repaint) Debug.LogWarning("Orphaned UIDrawCall detected!\nUse [Selection -> Force Delete] to get rid of it.");
        }
    }
}