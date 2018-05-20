//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Base class for all tweening operations.
/// </summary>

public abstract class UITweener : IgnoreTimeScale
{
    public enum Method
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
    }

    public enum Style
    {
        Once,
        Loop,
        PingPong,
    }

    /// <summary>
    /// Tweening method used.
    /// </summary>

    public Method method = Method.Linear;

    /// <summary>
    /// Does it play once? Does it loop?
    /// </summary>

    public Style style = Style.Once;

    /// <summary>
    /// How long is the duration of the tween?
    /// </summary>

    public float duration = 1f;

    /// <summary>
    /// Whether the tweener will use steeper curves for ease in / out style interpolation.
    /// </summary>

    public bool steeperCurves = false;

    /// <summary>
    /// Used by buttons and tween sequences. Group of '0' means not in a sequence.
    /// </summary>

    public int tweenGroup = 0;

    /// <summary>
    /// Target used with 'callWhenFinished', or this game object if none was specified.
    /// </summary>

    public GameObject eventReceiver;

    /// <summary>
    /// Name of the function to call when the tween finishes.
    /// </summary>

    public string callWhenFinished;

    private float mDuration = 0f;
    private float mAmountPerDelta = 1f;
    private float mFactor = 0f;

    /// <summary>
    /// Amount advanced per delta time.
    /// </summary>

    public float amountPerDelta
    {
        get
        {
            if (mDuration != duration)
            {
                mDuration = duration;
                mAmountPerDelta = Mathf.Abs((duration > 0f) ? 1f / duration : 1000f);
            }
            return mAmountPerDelta;
        }
    }

    /// <summary>
    /// Tween factor, 0-1 range.
    /// </summary>

    public float factor { get { return mFactor; } }

    /// <summary>
    /// Direction in which the tween is currently playing.
    /// </summary>

    public AnimationOrTween.Direction direction { get { return mAmountPerDelta < 0f ? AnimationOrTween.Direction.Reverse : AnimationOrTween.Direction.Forward; } }

    /// <summary>
    /// Update on start, so there is no frame in-between.
    /// </summary>

    private void Start()
    { Update(); }

    /// <summary>
    /// Update the tweening factor and call the virtual update function.
    /// </summary>

    private void Update()
    {
        float delta = UpdateRealTimeDelta();

        // Advance the sampling factor
        mFactor += amountPerDelta * delta;

        // Loop style simply resets the play factor after it exceeds 1.
        if (style == Style.Loop)
        {
            if (mFactor > 1f)
            {
                mFactor -= Mathf.Floor(mFactor);
            }
        }
        else if (style == Style.PingPong)
        {
            // Ping-pong style reverses the direction
            if (mFactor > 1f)
            {
                mFactor = 1f - (mFactor - Mathf.Floor(mFactor));
                mAmountPerDelta = -mAmountPerDelta;
            }
            else if (mFactor < 0f)
            {
                mFactor = -mFactor;
                mFactor -= Mathf.Floor(mFactor);
                mAmountPerDelta = -mAmountPerDelta;
            }
        }

        // Calculate the sampling value
        float val = Mathf.Clamp01(mFactor);

        if (method == Method.EaseIn)
        {
            val = 1f - Mathf.Sin(0.5f * Mathf.PI * (1f - val));
            if (steeperCurves) val *= val;
        }
        else if (method == Method.EaseOut)
        {
            val = Mathf.Sin(0.5f * Mathf.PI * val);

            if (steeperCurves)
            {
                val = 1f - val;
                val = 1f - val * val;
            }
        }
        else if (method == Method.EaseInOut)
        {
            const float pi2 = Mathf.PI * 2f;
            val = val - Mathf.Sin(val * pi2) / pi2;

            if (steeperCurves)
            {
                val = val * 2f - 1f;
                float sign = Mathf.Sign(val);
                val = 1f - Mathf.Abs(val);
                val = 1f - val * val;
                val = sign * val * 0.5f + 0.5f;
            }
        }

        // Call the virtual update
        OnUpdate(val);

        // If the factor goes out of range and this is a one-time tweening operation, disable the script
        if (style == Style.Once && (mFactor > 1f || mFactor < 0f))
        {
            mFactor = Mathf.Clamp01(mFactor);

            if (string.IsNullOrEmpty(callWhenFinished))
            {
                enabled = false;
            }
            else
            {
                if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
                {
                    // Notify the event listener target
                    eventReceiver.SendMessage(callWhenFinished, this, SendMessageOptions.DontRequireReceiver);
                }

                // Disable this script unless the SendMessage function above changed something
                if (mFactor == 1f && mAmountPerDelta > 0f || mFactor == 0f && mAmountPerDelta < 0f)
                {
                    enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Manually activate the tweening process, reversing it if necessary.
    /// </summary>

    public void Play(bool forward)
    {
        mAmountPerDelta = Mathf.Abs(amountPerDelta);
        if (!forward) mAmountPerDelta = -mAmountPerDelta;
        enabled = true;
    }

    [System.Obsolete("Use Tweener.Play instead")]
    public void Animate(bool forward) { Play(forward); }

    /// <summary>
    /// Manually reset the tweener's state to the beginning.
    /// </summary>

    public void Reset()
    {
        mFactor = (mAmountPerDelta < 0f) ? 1f : 0f;
    }

    /// <summary>
    /// Manually start the tweening process, reversing its direction.
    /// </summary>

    public void Toggle()
    {
        if (mFactor > 0f)
        {
            mAmountPerDelta = -amountPerDelta;
        }
        else
        {
            mAmountPerDelta = Mathf.Abs(amountPerDelta);
        }
        enabled = true;
    }

    /// <summary>
    /// Actual tweening logic should go here.
    /// </summary>

    abstract protected void OnUpdate(float factor);

    /// <summary>
    /// Starts the tweening operation.
    /// </summary>

    static public T Begin<T>(GameObject go, float duration) where T : UITweener
    {
        T comp = go.GetComponent<T>();
#if UNITY_FLASH
		if ((object)comp == null) comp = (T)go.AddComponent<T>();
#else
        if (comp == null) comp = go.AddComponent<T>();
#endif
        comp.duration = duration;
        comp.mFactor = 0f;
        comp.style = Style.Once;
        comp.enabled = true;
        return comp;
    }
}