//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script can be used to forward events from one object to another.
/// In most cases you should use UIEventListener script instead. For example:
/// UIEventListener.Get(gameObject).onClick += MyClickFunction;
/// </summary>
[AddComponentMenu("NGUI/Interaction/Forward Events")]
public class UIForwardEvents : MonoBehaviour
{
    public GameObject target;
    public bool onHover = false;
    public bool onPress = false;
    public bool onClick = false;
    public bool onDoubleClick = false;
    public bool onSelect = false;
    public bool onDrag = false;
    public bool onDrop = false;
    public bool onInput = false;
    public bool onSubmit = false;

    private void OnHover(bool isOver)
    {
        if (onHover && target != null)
        {
            target.SendMessage("OnHover", isOver, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnPress(bool pressed)
    {
        if (onPress && target != null)
        {
            target.SendMessage("OnPress", pressed, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnClick()
    {
        if (onClick && target != null)
        {
            target.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDoubleClick()
    {
        if (onDoubleClick && target != null)
        {
            target.SendMessage("OnDoubleClick", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnSelect(bool selected)
    {
        if (onSelect && target != null)
        {
            target.SendMessage("OnSelect", selected, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrag(Vector2 delta)
    {
        if (onDrag && target != null)
        {
            target.SendMessage("OnDrag", delta, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrop(GameObject go)
    {
        if (onDrop && target != null)
        {
            target.SendMessage("OnDrop", go, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnInput(string text)
    {
        if (onInput && target != null)
        {
            target.SendMessage("OnInput", text, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnSubmit()
    {
        if (onSubmit && target != null)
        {
            target.SendMessage("OnSubmit", SendMessageOptions.DontRequireReceiver);
        }
    }
}