//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Simple script that lets you localize a UIWidget.
/// </summary>

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/UI/Localize")]
public class UILocalize : MonoBehaviour
{
    /// <summary>
    /// Localization key.
    /// </summary>

    public string key;

    private string mLanguage;
    private bool mStarted = false;

    /// <summary>
    /// This function is called by the Localization manager via a broadcast SendMessage.
    /// </summary>

    private void OnLocalize(Localization loc)
    {
        if (mLanguage != loc.currentLanguage)
        {
            UIWidget w = GetComponent<UIWidget>();
            UILabel lbl = w as UILabel;
            UISprite sp = w as UISprite;

            // If no localization key has been specified, use the label's text as the key
            if (string.IsNullOrEmpty(mLanguage) && string.IsNullOrEmpty(key) && lbl != null) key = lbl.text;

            // If we still don't have a key, use the widget's name
            string val = string.IsNullOrEmpty(key) ? loc.Get(w.name) : loc.Get(key);

            if (lbl != null)
            {
                lbl.text = val;
            }
            else if (sp != null)
            {
                sp.spriteName = val;
                sp.MakePixelPerfect();
            }
            mLanguage = loc.currentLanguage;
        }
    }

    /// <summary>
    /// Localize the widget on enable, but only if it has been started already.
    /// </summary>

    private void OnEnable()
    {
        if (mStarted && Localization.instance != null) OnLocalize(Localization.instance);
    }

    /// <summary>
    /// Localize the widget on start.
    /// </summary>

    private void Start()
    {
        mStarted = true;
        if (Localization.instance != null) OnLocalize(Localization.instance);
    }
}