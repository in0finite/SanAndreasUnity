//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Turns the popup list it's attached to into a language selection list.
/// </summary>
[RequireComponent(typeof(UIPopupList))]
[AddComponentMenu("NGUI/Interaction/Language Selection")]
public class LanguageSelection : MonoBehaviour
{
    private UIPopupList mList;

    private void Start()
    {
        mList = GetComponent<UIPopupList>();
        UpdateList();
        mList.eventReceiver = gameObject;
        mList.functionName = "OnLanguageSelection";
    }

    private void UpdateList()
    {
        if (Localization.instance != null && Localization.instance.languages != null)
        {
            mList.items.Clear();

            for (int i = 0, imax = Localization.instance.languages.Length; i < imax; ++i)
            {
                TextAsset asset = Localization.instance.languages[i];
                if (asset != null) mList.items.Add(asset.name);
            }
            mList.selection = Localization.instance.currentLanguage;
        }
    }

    private void OnLanguageSelection(string language)
    {
        if (Localization.instance != null)
        {
            Localization.instance.currentLanguage = language;
        }
    }
}