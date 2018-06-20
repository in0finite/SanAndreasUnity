//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Attach this script to the parent of a group of checkboxes, or to a checkbox itself to save its state.
/// </summary>
[AddComponentMenu("NGUI/Interaction/Saved Option")]
public class UISavedOption : MonoBehaviour
{
    public string keyName;

    private string key
    { get { return (string.IsNullOrEmpty(keyName)) ? "NGUI State: " + name : keyName; } }

    /// <summary>
    /// Load and set the state of the checkboxes.
    /// </summary>
    private void OnEnable()
    {
        string s = PlayerPrefs.GetString(key);

        if (!string.IsNullOrEmpty(s))
        {
            UICheckbox c = GetComponent<UICheckbox>();

            if (c != null)
            {
                c.isChecked = (s == "true");
            }
            else
            {
                UICheckbox[] checkboxes = GetComponentsInChildren<UICheckbox>();

                for (int i = 0, imax = checkboxes.Length; i < imax; ++i)
                {
                    UICheckbox ch = checkboxes[i];
                    UIEventListener.Get(ch.gameObject).onClick -= Save;
                    ch.isChecked = (ch.name == s);
                    Debug.Log(s);
                    UIEventListener.Get(ch.gameObject).onClick += Save;
                }
            }
        }
    }

    /// <summary>
    /// Save the state on destroy.
    /// </summary>
    private void OnDisable()
    { Save(null); }

    /// <summary>
    /// Save the state.
    /// </summary>
    private void Save(GameObject go)
    {
        UICheckbox c = GetComponent<UICheckbox>();

        if (c != null)
        {
            PlayerPrefs.SetString(key, c.isChecked ? "true" : "false");
        }
        else
        {
            UICheckbox[] checkboxes = GetComponentsInChildren<UICheckbox>();

            for (int i = 0, imax = checkboxes.Length; i < imax; ++i)
            {
                UICheckbox ch = checkboxes[i];

                if (ch.isChecked)
                {
                    PlayerPrefs.SetString(key, ch.name);
                    break;
                }
            }
        }
    }
}