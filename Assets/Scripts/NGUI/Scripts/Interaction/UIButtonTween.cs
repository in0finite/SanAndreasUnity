//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using AnimationOrTween;
using UnityEngine;

/// <summary>
/// Attaching this to an object lets you activate tweener components on other objects.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Tween")]
public class UIButtonTween : MonoBehaviour
{
    /// <summary>
    /// Target on which there is one or more tween.
    /// </summary>

    public GameObject tweenTarget;

    /// <summary>
    /// If there are multiple tweens, you can choose which ones get activated by changing their group.
    /// </summary>

    public int tweenGroup = 0;

    /// <summary>
    /// Which event will trigger the tween.
    /// </summary>

    public Trigger trigger = Trigger.OnClick;

    /// <summary>
    /// Direction to tween in.
    /// </summary>

    public Direction playDirection = Direction.Forward;

    /// <summary>
    /// Whether the tween will be reset to the start or end when activated. If not, it will continue from where it currently is.
    /// </summary>

    public bool resetOnPlay = false;

    /// <summary>
    /// What to do if the tweenTarget game object is currently disabled.
    /// </summary>

    public EnableCondition ifDisabledOnPlay = EnableCondition.DoNothing;

    /// <summary>
    /// What to do with the tweenTarget after the tween finishes.
    /// </summary>

    public DisableCondition disableWhenFinished = DisableCondition.DoNotDisable;

    /// <summary>
    /// Whether the tweens on the child game objects will be considered.
    /// </summary>

    public bool includeChildren = false;

    /// <summary>
    /// Target used with 'callWhenFinished', or this game object if none was specified.
    /// </summary>

    public GameObject eventReceiver;

    /// <summary>
    /// Name of the function to call when the tween finishes.
    /// </summary>

    public string callWhenFinished;

    private UITweener[] mTweens;
    private bool mStarted = false;
    private bool mHighlighted = false;

    private void Start()
    { mStarted = true; if (tweenTarget == null) tweenTarget = gameObject; }

    private void OnEnable()
    { if (mStarted && mHighlighted) OnHover(UICamera.IsHighlighted(gameObject)); }

    private void OnHover(bool isOver)
    {
        if (enabled)
        {
            if (trigger == Trigger.OnHover ||
                (trigger == Trigger.OnHoverTrue && isOver) ||
                (trigger == Trigger.OnHoverFalse && !isOver))
            {
                Play(isOver);
            }
            mHighlighted = isOver;
        }
    }

    private void OnPress(bool isPressed)
    {
        if (enabled)
        {
            if (trigger == Trigger.OnPress ||
                (trigger == Trigger.OnPressTrue && isPressed) ||
                (trigger == Trigger.OnPressFalse && !isPressed))
            {
                Play(isPressed);
            }
        }
    }

    private void OnClick()
    {
        if (enabled && trigger == Trigger.OnClick)
        {
            Play(true);
        }
    }

    private void OnActivate(bool isActive)
    {
        if (enabled)
        {
            if (trigger == Trigger.OnActivate ||
                (trigger == Trigger.OnActivateTrue && isActive) ||
                (trigger == Trigger.OnActivateFalse && !isActive))
            {
                Play(isActive);
            }
        }
    }

    private void Update()
    {
        if (disableWhenFinished != DisableCondition.DoNotDisable && mTweens != null)
        {
            bool isFinished = true;
            bool properDirection = true;

            for (int i = 0, imax = mTweens.Length; i < imax; ++i)
            {
                UITweener tw = mTweens[i];

                if (tw.enabled)
                {
                    isFinished = false;
                    break;
                }
                else if ((int)tw.direction != (int)disableWhenFinished)
                {
                    properDirection = false;
                }
            }

            if (isFinished)
            {
                if (properDirection) NGUITools.SetActive(tweenTarget, false);
                mTweens = null;
            }
        }
    }

    /// <summary>
    /// Activate the tweeners.
    /// </summary>

    public void Play(bool forward)
    {
        GameObject go = (tweenTarget == null) ? gameObject : tweenTarget;

        if (!go.active)
        {
            // If the object is disabled, don't do anything
            if (ifDisabledOnPlay != EnableCondition.EnableThenPlay) return;

            // Enable the game object before tweening it
            NGUITools.SetActive(go, true);
        }

        // Gather the tweening components
        mTweens = includeChildren ? go.GetComponentsInChildren<UITweener>() : go.GetComponents<UITweener>();

        if (mTweens.Length == 0)
        {
            // No tweeners found -- should we disable the object?
            if (disableWhenFinished != DisableCondition.DoNotDisable) NGUITools.SetActive(tweenTarget, false);
        }
        else
        {
            bool activated = false;
            if (playDirection == Direction.Reverse) forward = !forward;

            // Run through all located tween components
            for (int i = 0, imax = mTweens.Length; i < imax; ++i)
            {
                UITweener tw = mTweens[i];

                // If the tweener's group matches, we can work with it
                if (tw.tweenGroup == tweenGroup)
                {
                    // Ensure that the game objects are enabled
                    if (!activated && !go.active)
                    {
                        activated = true;
                        NGUITools.SetActive(go, true);
                    }

                    // Toggle or activate the tween component
                    if (playDirection == Direction.Toggle) tw.Toggle();
                    else tw.Play(forward);
                    if (resetOnPlay) tw.Reset();

                    // Copy the event receiver
                    if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
                    {
                        tw.eventReceiver = eventReceiver;
                        tw.callWhenFinished = callWhenFinished;
                    }
                }
            }
        }
    }
}