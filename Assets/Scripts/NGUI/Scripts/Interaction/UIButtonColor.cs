//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Simple example script of how a button can be colored when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Color")]
public class UIButtonColor : MonoBehaviour
{
    /// <summary>
    /// Target with a widget, renderer, or light that will have its color tweened.
    /// </summary>

    public GameObject tweenTarget;

    /// <summary>
    /// Color to apply on hover event (mouse only).
    /// </summary>

    public Color hover = new Color(0.6f, 1f, 0.2f, 1f);

    /// <summary>
    /// Color to apply on the pressed event.
    /// </summary>

    public Color pressed = Color.grey;

    /// <summary>
    /// Duration of the tween process.
    /// </summary>

    public float duration = 0.2f;

    protected Color mColor;
    protected bool mInitDone = false;
    protected bool mStarted = false;
    protected bool mHighlighted = false;

    /// <summary>
    /// UIButtonColor's default (starting) color. It's useful to be able to change it, just in case.
    /// </summary>

    public Color defaultColor { get { return mColor; } set { mColor = value; } }

    private void Start()
    { mStarted = true; if (!mInitDone) Init(); }

    protected virtual void OnEnable()
    {
        if (mStarted && mHighlighted) OnHover(UICamera.IsHighlighted(gameObject));
    }

    private void OnDisable()
    {
        if (tweenTarget != null)
        {
            TweenColor tc = tweenTarget.GetComponent<TweenColor>();

            if (tc != null)
            {
                tc.color = mColor;
                tc.enabled = false;
            }
        }
    }

    protected void Init()
    {
        mInitDone = true;
        if (tweenTarget == null) tweenTarget = gameObject;
        UIWidget widget = tweenTarget.GetComponent<UIWidget>();

        if (widget != null)
        {
            mColor = widget.color;
        }
        else
        {
            Renderer ren = tweenTarget.GetComponent<Renderer>();

            if (ren != null)
            {
                mColor = ren.material.color;
            }
            else
            {
                Light lt = tweenTarget.GetComponent<Light>();

                if (lt != null)
                {
                    mColor = lt.color;
                }
                else
                {
                    Debug.LogWarning(NGUITools.GetHierarchy(gameObject) + " has nothing for UIButtonColor to color", this);
                    enabled = false;
                }
            }
        }
    }

    private void OnPress(bool isPressed)
    {
        if (!mInitDone) Init();
        if (enabled) TweenColor.Begin(tweenTarget, duration, isPressed ? pressed : (UICamera.IsHighlighted(gameObject) ? hover : mColor));
    }

    private void OnHover(bool isOver)
    {
        if (enabled)
        {
            if (!mInitDone) Init();
            TweenColor.Begin(tweenTarget, duration, isOver ? hover : mColor);
            mHighlighted = isOver;
        }
    }
}