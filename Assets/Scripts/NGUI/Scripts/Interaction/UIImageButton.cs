//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sample script showing how easy it is to implement a standard button that swaps sprites.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Image Button")]
public class UIImageButton : MonoBehaviour
{
    public UISprite target;
    public string normalSprite;
    public string hoverSprite;
    public string pressedSprite;

    private void Start()
    {
        if (target == null) target = GetComponentInChildren<UISprite>();
    }

    private void OnHover(bool isOver)
    {
        if (target != null)
        {
            target.spriteName = isOver ? hoverSprite : normalSprite;
            target.MakePixelPerfect();
        }
    }

    private void OnPress(bool pressed)
    {
        if (target != null)
        {
            target.spriteName = pressed ? pressedSprite : normalSprite;
            target.MakePixelPerfect();
        }
    }
}