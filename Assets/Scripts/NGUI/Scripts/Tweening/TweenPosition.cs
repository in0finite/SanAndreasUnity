//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's position.
/// </summary>
[AddComponentMenu("NGUI/Tween/Position")]
public class TweenPosition : UITweener
{
    public Vector3 from;
    public Vector3 to;

    private Transform mTrans;

    public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }
    public Vector3 position { get { return cachedTransform.localPosition; } set { cachedTransform.localPosition = value; } }

    override protected void OnUpdate(float factor)
    {
        cachedTransform.localPosition = from * (1f - factor) + to * factor;
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>
    static public TweenPosition Begin(GameObject go, float duration, Vector3 pos)
    {
        TweenPosition comp = UITweener.Begin<TweenPosition>(go, duration);
        comp.from = comp.position;
        comp.to = pos;
        return comp;
    }
}