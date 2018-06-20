//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using AnimationOrTween;
using UnityEngine;

/// <summary>
/// Play the specified animation on click.
/// Sends out the "OnAnimationFinished()" notification to the target when the animation finishes.
/// </summary>
[AddComponentMenu("NGUI/Interaction/Button Play Animation")]
public class UIButtonPlayAnimation : MonoBehaviour
{
    /// <summary>
    /// Target animation to activate.
    /// </summary>
    public Animation target;

    /// <summary>
    /// Optional clip name, if the animation has more than one clip.
    /// </summary>
    public string clipName;

    /// <summary>
    /// Which event will trigger the animation.
    /// </summary>
    public Trigger trigger = Trigger.OnClick;

    /// <summary>
    /// Which direction to animate in.
    /// </summary>
    public Direction playDirection = Direction.Forward;

    /// <summary>
    /// Whether the animation's position will be reset on play or will continue from where it left off.
    /// </summary>
    public bool resetOnPlay = false;

    /// <summary>
    /// Whether the selected object (this button) will be cleared when the animation gets activated.
    /// </summary>
    public bool clearSelection = false;

    /// <summary>
    /// What to do if the target game object is currently disabled.
    /// </summary>
    public EnableCondition ifDisabledOnPlay = EnableCondition.DoNothing;

    /// <summary>
    /// What to do with the target when the animation finishes.
    /// </summary>
    public DisableCondition disableWhenFinished = DisableCondition.DoNotDisable;

    /// <summary>
    /// Event receiver to trigger the callback on when the animation finishes.
    /// </summary>
    public GameObject eventReceiver;

    /// <summary>
    /// Function to call on the event receiver when the animation finishes.
    /// </summary>
    public string callWhenFinished;

    private bool mStarted = false;
    private bool mHighlighted = false;

    private void Start()
    { mStarted = true; }

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

    private void Play(bool forward)
    {
        if (target == null) target = GetComponentInChildren<Animation>();

        if (target != null)
        {
            if (clearSelection && UICamera.selectedObject == gameObject) UICamera.selectedObject = null;

            int pd = -(int)playDirection;
            Direction dir = forward ? playDirection : ((Direction)pd);
            ActiveAnimation anim = ActiveAnimation.Play(target, clipName, dir, ifDisabledOnPlay, disableWhenFinished);
            if (resetOnPlay) anim.Reset();

            // Copy the event receiver
            if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
            {
                anim.eventReceiver = eventReceiver;
                anim.callWhenFinished = callWhenFinished;
            }
        }
    }
}