//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity doesn't keep the values of static variables after scripts change get recompiled. One way around this
/// is to store the references in EditorPrefs -- retrieve them at start, and save them whenever something changes.
/// </summary>

public class UISettings
{
    private static bool mLoaded = false;
    private static UIFont mFont;
    private static UIAtlas mAtlas;
    private static TextAsset mFontData;
    private static Texture2D mFontTexture;
    private static string mFontName = "New Font";
    private static string mAtlasName = "New Atlas";
    private static int mAtlasPadding = 1;
    static public bool mAtlasTrimming = true;
    private static bool mPreview = true;

    private static Object GetObject(string name)
    {
        int assetID = EditorPrefs.GetInt(name, -1);
        return (assetID != -1) ? EditorUtility.InstanceIDToObject(assetID) : null;
    }

    private static void Load()
    {
        mLoaded = true;
        mFontName = EditorPrefs.GetString("NGUI Font Name");
        mAtlasName = EditorPrefs.GetString("NGUI Atlas Name");
        mFontData = GetObject("NGUI Font Asset") as TextAsset;
        mFontTexture = GetObject("NGUI Font Texture") as Texture2D;
        mFont = GetObject("NGUI Font") as UIFont;
        mAtlas = GetObject("NGUI Atlas") as UIAtlas;
        mPreview = EditorPrefs.GetInt("NGUI Preview") == 0;
        mAtlasPadding = EditorPrefs.GetInt("NGUI Atlas Padding", 1);
        mAtlasTrimming = EditorPrefs.GetBool("NGUI Atlas Trimming", true);
    }

    private static void Save()
    {
        EditorPrefs.SetString("NGUI Font Name", mFontName);
        EditorPrefs.SetString("NGUI Atlas Name", mAtlasName);
        EditorPrefs.SetInt("NGUI Font Asset", (mFontData != null) ? mFontData.GetInstanceID() : -1);
        EditorPrefs.SetInt("NGUI Font Texture", (mFontTexture != null) ? mFontTexture.GetInstanceID() : -1);
        EditorPrefs.SetInt("NGUI Font", (mFont != null) ? mFont.GetInstanceID() : -1);
        EditorPrefs.SetInt("NGUI Atlas", (mAtlas != null) ? mAtlas.GetInstanceID() : -1);
        EditorPrefs.SetInt("NGUI Preview", mPreview ? 0 : 1);
        EditorPrefs.SetInt("NGUI Atlas Padding", mAtlasPadding);
        EditorPrefs.SetBool("NGUI Atlas Trimming", mAtlasTrimming);
    }

    /// <summary>
    /// Default font used by NGUI.
    /// </summary>

    static public UIFont font
    {
        get
        {
            if (!mLoaded) Load();
            return mFont;
        }
        set
        {
            if (mFont != value)
            {
                mFont = value;
                mFontName = (mFont != null) ? mFont.name : "New Font";
                Save();
            }
        }
    }

    /// <summary>
    /// Default atlas used by NGUI.
    /// </summary>

    static public UIAtlas atlas
    {
        get
        {
            if (!mLoaded) Load();
            return mAtlas;
        }
        set
        {
            if (mAtlas != value)
            {
                mAtlas = value;
                mAtlasName = (mAtlas != null) ? mAtlas.name : "New Atlas";
                Save();
            }
        }
    }

    /// <summary>
    /// Name of the font, used by the Font Maker.
    /// </summary>

    static public string fontName { get { if (!mLoaded) Load(); return mFontName; } set { if (mFontName != value) { mFontName = value; Save(); } } }

    /// <summary>
    /// Data used to create the font, used by the Font Maker.
    /// </summary>

    static public TextAsset fontData { get { if (!mLoaded) Load(); return mFontData; } set { if (mFontData != value) { mFontData = value; Save(); } } }

    /// <summary>
    /// Texture used to create the font, used by the Font Maker.
    /// </summary>

    static public Texture2D fontTexture { get { if (!mLoaded) Load(); return mFontTexture; } set { if (mFontTexture != value) { mFontTexture = value; Save(); } } }

    /// <summary>
    /// Name of the atlas, used by the Atlas maker.
    /// </summary>

    static public string atlasName { get { if (!mLoaded) Load(); return mAtlasName; } set { if (mAtlasName != value) { mAtlasName = value; Save(); } } }

    /// <summary>
    /// Whether the texture preview will be shown.
    /// </summary>

    static public bool texturePreview { get { if (!mLoaded) Load(); return mPreview; } set { if (mPreview != value) { mPreview = value; Save(); } } }

    /// <summary>
    /// Added padding in-between of sprites when creating an atlas.
    /// </summary>

    static public int atlasPadding { get { if (!mLoaded) Load(); return mAtlasPadding; } set { if (mAtlasPadding != value) { mAtlasPadding = value; Save(); } } }

    /// <summary>
    /// Whether the transparent pixels will be trimmed away when creating an atlas.
    /// </summary>

    static public bool atlasTrimming { get { if (!mLoaded) Load(); return mAtlasTrimming; } set { if (mAtlasTrimming != value) { mAtlasTrimming = value; Save(); } } }
}