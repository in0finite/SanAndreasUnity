//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Editable text input field that automatically saves its data to PlayerPrefs.
/// </summary>
[AddComponentMenu("NGUI/UI/Input (Saved)")]
public class UIInputSaved : UIInput
{
    public string playerPrefsField;

    private void Start()
    {
        Init();

        if (!string.IsNullOrEmpty(playerPrefsField) && PlayerPrefs.HasKey(playerPrefsField))
        {
            text = PlayerPrefs.GetString(playerPrefsField);
        }
    }

    private void OnApplicationQuit()
    {
        /*if (!string.IsNullOrEmpty(playerPrefsField))
        {
            PlayerPrefs.SetString(playerPrefsField, text);
        }*/
    }
}