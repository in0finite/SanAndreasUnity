//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Simple example script of how a button can be rotated visibly when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Rotation")]
public class UIButtonRotation : MonoBehaviour
{
    public Transform tweenTarget;
    public Vector3 hover = Vector3.zero;
    public Vector3 pressed = Vector3.zero;
    public float duration = 0.2f;

    private Quaternion mRot;
    private bool mInitDone = false;
    private bool mStarted = false;
    private bool mHighlighted = false;

    private void Start()
    { mStarted = true; }

    private void OnEnable()
    { if (mStarted && mHighlighted) OnHover(UICamera.IsHighlighted(gameObject)); }

    private void OnDisable()
    {
        if (tweenTarget != null)
        {
            TweenRotation tc = tweenTarget.GetComponent<TweenRotation>();

            if (tc != null)
            {
                tc.rotation = mRot;
                tc.enabled = false;
            }
        }
    }

    private void Init()
    {
        mInitDone = true;
        if (tweenTarget == null) tweenTarget = transform;
        mRot = tweenTarget.localRotation;
    }

    private void OnPress(bool isPressed)
    {
        if (enabled)
        {
            if (!mInitDone) Init();
            TweenRotation.Begin(tweenTarget.gameObject, duration, isPressed ? mRot * Quaternion.Euler(pressed) :
                (UICamera.IsHighlighted(gameObject) ? mRot * Quaternion.Euler(hover) : mRot)).method = UITweener.Method.EaseInOut;
        }
    }

    private void OnHover(bool isOver)
    {
        if (enabled)
        {
            if (!mInitDone) Init();
            TweenRotation.Begin(tweenTarget.gameObject, duration, isOver ? mRot * Quaternion.Euler(hover) : mRot).method = UITweener.Method.EaseInOut;
            mHighlighted = isOver;
        }
    }
}