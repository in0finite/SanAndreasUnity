//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's rotation.
/// </summary>

[AddComponentMenu("NGUI/Tween/Rotation")]
public class TweenRotation : UITweener
{
    public Vector3 from;
    public Vector3 to;

    private Transform mTrans;

    public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }
    public Quaternion rotation { get { return cachedTransform.localRotation; } set { cachedTransform.localRotation = value; } }

    override protected void OnUpdate(float factor)
    {
        cachedTransform.localRotation = Quaternion.Slerp(Quaternion.Euler(from), Quaternion.Euler(to), factor);
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenRotation Begin(GameObject go, float duration, Quaternion rot)
    {
        TweenRotation comp = UITweener.Begin<TweenRotation>(go, duration);
        comp.from = comp.rotation.eulerAngles;
        comp.to = rot.eulerAngles;
        return comp;
    }
}