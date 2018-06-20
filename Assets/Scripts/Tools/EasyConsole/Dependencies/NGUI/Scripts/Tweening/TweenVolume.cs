//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the audio source's volume.
/// </summary>
[AddComponentMenu("NGUI/Tween/Volume")]
public class TweenVolume : UITweener
{
    public float from = 0f;
    public float to = 1f;

    private AudioSource mSource;

    /// <summary>
    /// Cached version of 'audio', as it's always faster to cache.
    /// </summary>
    public AudioSource audioSource
    {
        get
        {
            if (mSource == null)
            {
                mSource = GetComponent<AudioSource>();

                if (mSource == null)
                {
                    mSource = GetComponentInChildren<AudioSource>();

                    if (mSource == null)
                    {
                        Debug.LogError("TweenVolume needs an AudioSource to work with", this);
                        enabled = false;
                    }
                }
            }
            return mSource;
        }
    }

    /// <summary>
    /// Audio source's current volume.
    /// </summary>
    public float volume { get { return audioSource.volume; } set { audioSource.volume = value; } }

    /// <summary>
    /// Tween update function.
    /// </summary>
    override protected void OnUpdate(float factor)
    {
        volume = from * (1f - factor) + to * factor;
        mSource.enabled = (mSource.volume > 0.01f);
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>
    static public TweenVolume Begin(GameObject go, float duration, float targetVolume)
    {
        TweenVolume comp = UITweener.Begin<TweenVolume>(go, duration);
        comp.from = comp.volume;
        comp.to = targetVolume;
        return comp;
    }
}