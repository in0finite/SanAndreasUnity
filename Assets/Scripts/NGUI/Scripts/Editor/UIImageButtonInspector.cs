//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CustomEditor(typeof(UIImageButton))]
public class UIImageButtonInspector : Editor
{
    private UIImageButton mButton;

    /// <summary>
    /// Atlas selection callback.
    /// </summary>

    private void OnSelectAtlas(MonoBehaviour obj)
    {
        if (mButton.target != null)
        {
            NGUIEditorTools.RegisterUndo("Atlas Selection", mButton.target);
            mButton.target.atlas = obj as UIAtlas;
            mButton.target.MakePixelPerfect();
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        mButton = target as UIImageButton;

        UISprite sprite = EditorGUILayout.ObjectField("Sprite", mButton.target, typeof(UISprite), true) as UISprite;

        if (mButton.target != sprite)
        {
            NGUIEditorTools.RegisterUndo("Image Button Change", mButton);
            mButton.target = sprite;
            if (sprite != null) sprite.spriteName = mButton.normalSprite;
        }

        if (mButton.target != null)
        {
            ComponentSelector.Draw<UIAtlas>(sprite.atlas as UIAtlas, OnSelectAtlas);

            if (sprite.atlas != null)
            {
                string normal = UISpriteInspector.SpriteField(sprite.atlas as UIAtlas, "Normal", mButton.normalSprite);
                string hover = UISpriteInspector.SpriteField(sprite.atlas as UIAtlas, "Hover", mButton.hoverSprite);
                string press = UISpriteInspector.SpriteField(sprite.atlas as UIAtlas, "Pressed", mButton.pressedSprite);

                if (mButton.normalSprite != normal ||
                    mButton.hoverSprite != hover ||
                    mButton.pressedSprite != press)
                {
                    NGUIEditorTools.RegisterUndo("Image Button Change", mButton, mButton.gameObject, sprite);
                    mButton.normalSprite = normal;
                    mButton.hoverSprite = hover;
                    mButton.pressedSprite = press;
                    sprite.spriteName = normal;
                    sprite.MakePixelPerfect();
                    NGUITools.AddWidgetCollider(mButton.gameObject);
                }
            }
        }
    }
}