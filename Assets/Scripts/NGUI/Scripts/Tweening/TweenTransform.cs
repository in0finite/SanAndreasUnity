//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's position, rotation and scale.
/// </summary>

[AddComponentMenu("NGUI/Tween/Transform")]
public class TweenTransform : UITweener
{
    public Transform from;
    public Transform to;

    private Transform mTrans;

    override protected void OnUpdate(float factor)
    {
        if (from != null && to != null)
        {
            if (mTrans == null) mTrans = transform;
            mTrans.position = from.position * (1f - factor) + to.position * factor;
            mTrans.localScale = from.localScale * (1f - factor) + to.localScale * factor;
            mTrans.rotation = Quaternion.Slerp(from.rotation, to.rotation, factor);
        }
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenTransform Begin(GameObject go, float duration, Transform from, Transform to)
    {
        TweenTransform comp = UITweener.Begin<TweenTransform>(go, duration);
        comp.from = from;
        comp.to = to;
        return comp;
    }
}