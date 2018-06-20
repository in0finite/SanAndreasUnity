//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UISlicedSprites.
/// </summary>
[CustomEditor(typeof(UISlicedSprite))]
public class UISlicedSpriteInspector : UISpriteInspector
{
    /// <summary>
    /// Draw the atlas and sprite selection fields.
    /// </summary>
    override protected bool OnDrawProperties()
    {
        if (base.OnDrawProperties())
        {
            UISlicedSprite sp = mSprite as UISlicedSprite;
            bool fill = EditorGUILayout.Toggle("Fill Center", sp.fillCenter);

            if (sp.fillCenter != fill)
            {
                NGUIEditorTools.RegisterUndo("Sprite Change", sp);
                sp.fillCenter = fill;
                EditorUtility.SetDirty(sp.gameObject);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Any and all derived functionality.
    /// </summary>
    protected override void OnDrawTexture()
    {
        UISlicedSprite sprite = mWidget as UISlicedSprite;
        Texture2D tex = sprite.mainTexture as Texture2D;

        if (tex != null)
        {
            // Draw the atlas
            EditorGUILayout.Separator();
            Rect rect = NGUIEditorTools.DrawSprite(tex, sprite.outerUV, mUseShader ? mSprite.atlas.spriteMaterial : null);

            // Draw the selection
            NGUIEditorTools.DrawOutline(rect, sprite.outerUV, sprite.innerUV);

            // Sprite size label
            string text = "Sprite Size: ";
            text += Mathf.RoundToInt(Mathf.Abs(sprite.outerUV.width * tex.width));
            text += "x";
            text += Mathf.RoundToInt(Mathf.Abs(sprite.outerUV.height * tex.height));

            rect = GUILayoutUtility.GetRect(Screen.width, 18f);
            EditorGUI.DropShadowLabel(rect, text);
        }
    }
}