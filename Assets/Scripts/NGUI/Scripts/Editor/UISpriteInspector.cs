//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UISprite))]
public class UISpriteInspector : UIWidgetInspector
{
    protected UISprite mSprite;

    /// <summary>
    /// Atlas selection callback.
    /// </summary>

    private void OnSelectAtlas(MonoBehaviour obj)
    {
        if (mSprite != null)
        {
            NGUIEditorTools.RegisterUndo("Atlas Selection", mSprite);
            bool resize = (mSprite.atlas == null);
            mSprite.atlas = obj as UIAtlas;
            if (resize) mSprite.MakePixelPerfect();
            EditorUtility.SetDirty(mSprite.gameObject);
        }
    }

    /// <summary>
    /// Convenience function that displays a list of sprites and returns the selected value.
    /// </summary>

    static public string SpriteField(UIAtlas atlas, string field, string name, params GUILayoutOption[] options)
    {
        List<string> sprites = atlas.GetListOfSprites();
        return (sprites != null && sprites.Count > 0) ? NGUIEditorTools.DrawList(field, sprites.ToArray(), name, options) : null;
    }

    /// <summary>
    /// Convenience function that displays a list of sprites and returns the selected value.
    /// </summary>

    static public string SpriteField(UIAtlas atlas, string name, params GUILayoutOption[] options)
    {
        return SpriteField(atlas, "Sprite", name, options);
    }

    /// <summary>
    /// Draw the atlas and sprite selection fields.
    /// </summary>

    override protected bool OnDrawProperties()
    {
        mSprite = mWidget as UISprite;
        ComponentSelector.Draw<UIAtlas>(mSprite.atlas as UIAtlas, OnSelectAtlas);
        if (mSprite.atlas == null) return false;

        string spriteName = SpriteField(mSprite.atlas as UIAtlas, mSprite.spriteName);

        if (mSprite.spriteName != spriteName)
        {
            NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
            mSprite.spriteName = spriteName;
            mSprite.MakePixelPerfect();
            EditorUtility.SetDirty(mSprite.gameObject);
        }
        return true;
    }

    /// <summary>
    /// Draw the sprite texture.
    /// </summary>

    override protected void OnDrawTexture()
    {
        Texture2D tex = mSprite.mainTexture as Texture2D;

        if (tex != null)
        {
            // Draw the atlas
            EditorGUILayout.Separator();
            Rect rect = NGUIEditorTools.DrawSprite(tex, mSprite.outerUV, mUseShader ? mSprite.atlas.spriteMaterial : null);

            // Draw the selection
            NGUIEditorTools.DrawOutline(rect, mSprite.outerUV, new Color(0.4f, 1f, 0f, 1f));

            // Sprite size label
            string text = "Sprite Size: ";
            text += Mathf.RoundToInt(Mathf.Abs(mSprite.outerUV.width * tex.width));
            text += "x";
            text += Mathf.RoundToInt(Mathf.Abs(mSprite.outerUV.height * tex.height));

            rect = GUILayoutUtility.GetRect(Screen.width, 18f);
            EditorGUI.DropShadowLabel(rect, text);
        }
    }
}