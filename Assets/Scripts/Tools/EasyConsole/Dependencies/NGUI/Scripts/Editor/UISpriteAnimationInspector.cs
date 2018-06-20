//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UISpriteAnimations.
/// </summary>
[CustomEditor(typeof(UISpriteAnimation))]
public class UISpriteAnimationInspector : Editor
{
    /// <summary>
    /// Draw the inspector widget.
    /// </summary>
    public override void OnInspectorGUI()
    {
        NGUIEditorTools.DrawSeparator();
        EditorGUIUtility.LookLikeControls(80f);
        UISpriteAnimation anim = target as UISpriteAnimation;

        int fps = EditorGUILayout.IntField("Framerate", anim.framesPerSecond);
        fps = Mathf.Clamp(fps, 1, 60);

        if (anim.framesPerSecond != fps)
        {
            NGUIEditorTools.RegisterUndo("Sprite Animation Change", anim);
            anim.framesPerSecond = fps;
            EditorUtility.SetDirty(anim);
        }

        string namePrefix = EditorGUILayout.TextField("Name Prefix", (anim.namePrefix != null) ? anim.namePrefix : "");

        if (anim.namePrefix != namePrefix)
        {
            NGUIEditorTools.RegisterUndo("Sprite Animation Change", anim);
            anim.namePrefix = namePrefix;
            EditorUtility.SetDirty(anim);
        }
    }
}