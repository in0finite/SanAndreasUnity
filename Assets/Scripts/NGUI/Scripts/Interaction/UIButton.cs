//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Similar to UIButtonColor, but adds a 'disabled' state based on whether the collider is enabled or not.
/// </summary>
[AddComponentMenu("NGUI/Interaction/Button")]
public class UIButton : UIButtonColor
{
    /// <summary>
    /// If the collider is disabled, assume the disabled color.
    /// </summary>
    protected override void OnEnable()
    {
        if (isEnabled) base.OnEnable();
        else UpdateColor(false, true);
    }

    /// <summary>
    /// Whether the button should be enabled.
    /// </summary>
    public bool isEnabled
    {
        get
        {
            Collider col = GetComponent<Collider>();
            return col && col.enabled;
        }
        set
        {
            Collider col = GetComponent<Collider>();
            if (!col) return;

            if (col.enabled != value)
            {
                col.enabled = value;
                UpdateColor(value, false);
            }
        }
    }

    /// <summary>
    /// Update the button's color to either enabled or disabled state.
    /// </summary>
    private void UpdateColor(bool shouldBeEnabled, bool immediate)
    {
        if (tweenTarget)
        {
            if (!mInitDone) Init();

            Color c = defaultColor;

            if (!shouldBeEnabled)
            {
                c.r *= 0.65f;
                c.g *= 0.65f;
                c.b *= 0.65f;
            }

            TweenColor tc = TweenColor.Begin(tweenTarget, 0.15f, c);

            if (immediate)
            {
                tc.color = c;
                tc.enabled = false;
            }
        }
    }
}