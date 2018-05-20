//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UITextures.
/// </summary>

[CustomEditor(typeof(UITexture))]
public class UITextureInspector : UIWidgetInspector
{
    override protected bool OnDrawProperties()
    {
        Material mat = EditorGUILayout.ObjectField("Material", mWidget.material, typeof(Material), false) as Material;

        if (mWidget.material != mat)
        {
            NGUIEditorTools.RegisterUndo("Material Selection", mWidget);
            mWidget.material = mat;
        }
        return (mWidget.material != null);
    }

    override protected void OnDrawTexture()
    {
        Texture2D tex = mWidget.mainTexture as Texture2D;

        if (tex != null)
        {
            // Draw the atlas
            EditorGUILayout.Separator();
            NGUIEditorTools.DrawSprite(tex, new Rect(0f, 0f, 1f, 1f), null);

            // Sprite size label
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
            EditorGUI.DropShadowLabel(rect, "Texture Size: " + tex.width + "x" + tex.height);
        }
    }
}