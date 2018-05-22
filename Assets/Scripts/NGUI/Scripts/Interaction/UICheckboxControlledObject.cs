//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Example script showing how to activate or deactivate a game object when OnActivate event is received.
/// OnActivate event is sent out by the UICheckbox script.
/// </summary>
[AddComponentMenu("NGUI/Interaction/Checkbox Controlled Object")]
public class UICheckboxControlledObject : MonoBehaviour
{
    public GameObject target;
    public bool inverse = false;

    private void OnActivate(bool isActive)
    {
        if (target != null) NGUITools.SetActive(target, inverse ? !isActive : isActive);
    }
}