//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Localization manager is able to parse localization information from text assets.
/// Although a singleton, you will generally not access this class as such. Instead
/// you should implement "void Localize (Localization loc)" functions in your classes.
/// Take a look at UILocalize to see how it's used.
/// </summary>

[AddComponentMenu("NGUI/Internal/Localization")]
public class Localization : MonoBehaviour
{
    private static Localization mInst;

    /// <summary>
    /// The instance of the localization class. Will create it if one isn't already around.
    /// </summary>

    static public Localization instance
    {
        get
        {
            if (mInst == null)
            {
                mInst = Object.FindObjectOfType(typeof(Localization)) as Localization;

                if (mInst == null)
                {
                    GameObject go = new GameObject("_Localization");
                    DontDestroyOnLoad(go);
                    mInst = go.AddComponent<Localization>();
                }
            }
            return mInst;
        }
    }

    /// <summary>
    /// Language the localization manager will start with.
    /// </summary>

    public string startingLanguage;

    /// <summary>
    /// Available list of languages.
    /// </summary>

    public TextAsset[] languages;

    private Dictionary<string, string> mDictionary = new Dictionary<string, string>();
    private string mLanguage;

    /// <summary>
    /// Name of the currently active language.
    /// </summary>

    public string currentLanguage
    {
        get
        {
            if (string.IsNullOrEmpty(mLanguage))
            {
                currentLanguage = PlayerPrefs.GetString("Language");

                if (string.IsNullOrEmpty(mLanguage))
                {
                    currentLanguage = startingLanguage;

                    if (string.IsNullOrEmpty(mLanguage) && (languages != null && languages.Length > 0))
                    {
                        currentLanguage = languages[0].name;
                    }
                }
            }
            return mLanguage;
        }
        set
        {
            if (mLanguage != value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Check the referenced assets first
                    if (languages != null)
                    {
                        for (int i = 0, imax = languages.Length; i < imax; ++i)
                        {
                            TextAsset asset = languages[i];

                            if (asset != null && asset.name == value)
                            {
                                Load(asset);
                                return;
                            }
                        }
                    }

                    // Not a referenced asset -- try to load it dynamically
                    TextAsset txt = Resources.Load(value, typeof(TextAsset)) as TextAsset;

                    if (txt != null)
                    {
                        Load(txt);
                        return;
                    }
                }

                // Either the language is null, or it wasn't found
                mDictionary.Clear();
                PlayerPrefs.DeleteKey("Language");
            }
        }
    }

    /// <summary>
    /// Determine the starting language.
    /// </summary>

    private void Awake()
    { if (mInst == null) { mInst = this; DontDestroyOnLoad(gameObject); } else Destroy(gameObject); }

    /// <summary>
    /// Oddly enough... sometimes if there is no OnEnable function in Localization, it can get the Awake call after UILocalize's OnEnable.
    /// </summary>

    private void OnEnable()
    { if (mInst == null) mInst = this; }

    /// <summary>
    /// Remove the instance reference.
    /// </summary>

    private void OnDestroy()
    { if (mInst == this) mInst = null; }

    /// <summary>
    /// Load the specified asset and activate the localization.
    /// </summary>

    private void Load(TextAsset asset)
    {
        mLanguage = asset.name;
        PlayerPrefs.SetString("Language", mLanguage);
        ByteReader reader = new ByteReader(asset);
        mDictionary = reader.ReadDictionary();
        UIRoot.Broadcast("OnLocalize", this);
    }

    /// <summary>
    /// Localize the specified value.
    /// </summary>

    public string Get(string key)
    {
        string val;
        return (mDictionary.TryGetValue(key, out val)) ? val : key;
    }
}