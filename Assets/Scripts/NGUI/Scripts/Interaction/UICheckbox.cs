//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using AnimationOrTween;
using UnityEngine;

/// <summary>
/// Simple checkbox functionality. If 'option' is enabled, checking this checkbox will uncheck all other checkboxes with the same parent.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Checkbox")]
public class UICheckbox : MonoBehaviour
{
    static public UICheckbox current;

    public UISprite checkSprite;
    public Animation checkAnimation;
    public GameObject eventReceiver;
    public string functionName = "OnActivate";
    public bool startsChecked = true;
    public Transform radioButtonRoot;
    public bool optionCanBeNone = false;

    // Prior to 1.90 'option' was used to toggle the radio button group functionality
    [HideInInspector] [SerializeField] private bool option = false;

    private bool mChecked = true;
    private bool mStarted = false;
    private Transform mTrans;

    /// <summary>
    /// Whether the checkbox is checked.
    /// </summary>

    public bool isChecked
    {
        get { return mChecked; }
        set { if (radioButtonRoot == null || value || optionCanBeNone || !mStarted) Set(value); }
    }

    /// <summary>
    /// Legacy functionality support -- set the radio button root if the 'option' value was 'true'.
    /// </summary>

    private void Awake()
    {
        mTrans = transform;

        if (checkSprite != null) checkSprite.alpha = startsChecked ? 1f : 0f;

        if (option)
        {
            option = false;
            if (radioButtonRoot == null) radioButtonRoot = mTrans.parent;
        }
    }

    /// <summary>
    /// Activate the initial state.
    /// </summary>

    private void Start()
    {
        if (eventReceiver == null) eventReceiver = gameObject;
        mChecked = !startsChecked;
        mStarted = true;
        Set(startsChecked);
    }

    /// <summary>
    /// Check or uncheck on click.
    /// </summary>

    private void OnClick()
    { if (enabled) isChecked = !isChecked; }

    /// <summary>
    /// Fade out or fade in the checkmark and notify the target of OnChecked event.
    /// </summary>

    private void Set(bool state)
    {
        if (!mStarted)
        {
            mChecked = state;
            startsChecked = state;
            if (checkSprite != null) checkSprite.alpha = state ? 1f : 0f;
        }
        else if (mChecked != state)
        {
            // Uncheck all other checkboxes
            if (radioButtonRoot != null && state)
            {
                UICheckbox[] cbs = radioButtonRoot.GetComponentsInChildren<UICheckbox>(true);

                for (int i = 0, imax = cbs.Length; i < imax; ++i)
                {
                    UICheckbox cb = cbs[i];
                    if (cb != this && cb.radioButtonRoot == radioButtonRoot) cb.Set(false);
                }
            }

            // Remember the state
            mChecked = state;

            // Tween the color of the checkmark
            if (checkSprite != null)
            {
                Color c = checkSprite.color;
                c.a = mChecked ? 1f : 0f;
                TweenColor.Begin(checkSprite.gameObject, 0.2f, c);
            }

            // Send out the event notification
            if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
            {
                current = this;
                eventReceiver.SendMessage(functionName, mChecked, SendMessageOptions.DontRequireReceiver);
            }

            // Play the checkmark animation
            if (checkAnimation != null)
            {
                ActiveAnimation.Play(checkAnimation, state ? Direction.Forward : Direction.Reverse);
            }
        }
    }
}