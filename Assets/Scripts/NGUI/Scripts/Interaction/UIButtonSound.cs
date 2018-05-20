//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Plays the specified sound.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Sound")]
public class UIButtonSound : MonoBehaviour
{
    public enum Trigger
    {
        OnClick,
        OnMouseOver,
        OnMouseOut,
        OnPress,
        OnRelease,
    }

    public AudioClip audioClip;
    public Trigger trigger = Trigger.OnClick;
    public float volume = 1f;
    public float pitch = 1f;

    private void OnHover(bool isOver)
    {
        if (enabled && ((isOver && trigger == Trigger.OnMouseOver) || (!isOver && trigger == Trigger.OnMouseOut)))
        {
            NGUITools.PlaySound(audioClip, volume, pitch);
        }
    }

    private void OnPress(bool isPressed)
    {
        if (enabled && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
        {
            NGUITools.PlaySound(audioClip, volume, pitch);
        }
    }

    private void OnClick()
    {
        if (enabled && trigger == Trigger.OnClick)
        {
            NGUITools.PlaySound(audioClip, volume, pitch);
        }
    }
}