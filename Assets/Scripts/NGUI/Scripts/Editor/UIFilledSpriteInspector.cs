//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;

/// <summary>
/// Inspector class used to edit UIFilledSprites.
/// </summary>
[CustomEditor(typeof(UIFilledSprite))]
public class UIFilledSpriteInspector : UISpriteInspector
{
    override protected bool OnDrawProperties()
    {
        UIFilledSprite sprite = mWidget as UIFilledSprite;

        if (!base.OnDrawProperties()) return false;

        if ((int)sprite.fillDirection > (int)UIFilledSprite.FillDirection.Radial360)
        {
            sprite.fillDirection = UIFilledSprite.FillDirection.Horizontal;
            EditorUtility.SetDirty(sprite);
        }

        UIFilledSprite.FillDirection fillDirection = (UIFilledSprite.FillDirection)EditorGUILayout.EnumPopup("Fill Dir", sprite.fillDirection);
        float fillAmount = EditorGUILayout.Slider("Fill Amount", sprite.fillAmount, 0f, 1f);
        bool invert = EditorGUILayout.Toggle("Invert Fill", sprite.invert);

        if (sprite.fillDirection != fillDirection || sprite.fillAmount != fillAmount || sprite.invert != invert)
        {
            NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
            sprite.fillDirection = fillDirection;
            sprite.fillAmount = fillAmount;
            sprite.invert = invert;
            EditorUtility.SetDirty(sprite);
        }
        return true;
    }
}