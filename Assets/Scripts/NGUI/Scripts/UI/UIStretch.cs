//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script can be used to stretch objects relative to the screen's width and height.
/// The most obvious use would be to create a full-screen background by attaching it to a sprite.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Stretch")]
public class UIStretch : MonoBehaviour
{
    public enum Style
    {
        None,
        Horizontal,
        Vertical,
        Both,
        BasedOnHeight,
    }

    public Camera uiCamera = null;
    public Style style = Style.None;
    public Vector2 relativeSize = Vector2.one;

    private Transform mTrans;
    private UIRoot mRoot;

    private void OnEnable()
    {
        if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
        mRoot = NGUITools.FindInParents<UIRoot>(gameObject);
    }

    private void Update()
    {
        if (uiCamera != null && style != Style.None)
        {
            if (mTrans == null) mTrans = transform;

            Rect rect = uiCamera.pixelRect;
            float screenWidth = rect.width;
            float screenHeight = rect.height;

            if (mRoot != null && !mRoot.automatic && screenHeight > 1f)
            {
                float scale = mRoot.manualHeight / screenHeight;
                screenWidth *= scale;
                screenHeight *= scale;
            }

            Vector3 localScale = mTrans.localScale;

            if (style == Style.BasedOnHeight)
            {
                localScale.x = relativeSize.x * screenHeight;
                localScale.y = relativeSize.y * screenHeight;
            }
            else
            {
                if (style == Style.Both || style == Style.Horizontal) localScale.x = relativeSize.x * screenWidth;
                if (style == Style.Both || style == Style.Vertical) localScale.y = relativeSize.y * screenHeight;
            }

            if (mTrans.localScale != localScale) mTrans.localScale = localScale;
        }
    }
}