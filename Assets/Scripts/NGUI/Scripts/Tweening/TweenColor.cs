//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's color.
/// </summary>

[AddComponentMenu("NGUI/Tween/Color")]
public class TweenColor : UITweener
{
    public Color from = Color.white;
    public Color to = Color.white;

    private Transform mTrans;
    private UIWidget mWidget;
    private Material mMat;
    private Light mLight;

    /// <summary>
    /// Current color.
    /// </summary>

    public Color color
    {
        get
        {
            if (mWidget != null) return mWidget.color;
            if (mLight != null) return mLight.color;
            if (mMat != null) return mMat.color;
            return Color.black;
        }
        set
        {
            if (mWidget != null) mWidget.color = value;
            if (mMat != null) mMat.color = value;

            if (mLight != null)
            {
                mLight.color = value;
                mLight.enabled = (value.r + value.g + value.b) > 0.01f;
            }
        }
    }

    /// <summary>
    /// Find all needed components.
    /// </summary>

    private void Awake()
    {
        mWidget = GetComponentInChildren<UIWidget>();
        Renderer ren = GetComponent<Renderer>();
        if (ren != null) mMat = ren.material;
        mLight = GetComponent<Light>();
    }

    /// <summary>
    /// Interpolate and update the color.
    /// </summary>

    override protected void OnUpdate(float factor)
    {
        color = from * (1f - factor) + to * factor;
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenColor Begin(GameObject go, float duration, Color color)
    {
        TweenColor comp = UITweener.Begin<TweenColor>(go, duration);
        comp.from = comp.color;
        comp.to = color;
        return comp;
    }
}