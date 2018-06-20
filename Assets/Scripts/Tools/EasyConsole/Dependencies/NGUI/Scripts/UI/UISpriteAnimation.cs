//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Very simple sprite animation. Attach to a sprite and specify a bunch of sprite names and it will cycle through them.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Sprite Animation")]
public class UISpriteAnimation : MonoBehaviour
{
    [HideInInspector] [SerializeField] private int mFPS = 30;
    [HideInInspector] [SerializeField] private string mPrefix = "";

    private UISprite mSprite;
    private float mDelta = 0f;
    private int mIndex = 0;
    private List<string> mSpriteNames = new List<string>();

    /// <summary>
    /// Animation framerate.
    /// </summary>
    public int framesPerSecond { get { return mFPS; } set { mFPS = value; } }

    /// <summary>
    /// Set the name prefix used to filter sprites from the atlas.
    /// </summary>
    public string namePrefix { get { return mPrefix; } set { if (mPrefix != value) { mPrefix = value; RebuildSpriteList(); } } }

    /// <summary>
    /// Rebuild the sprite list first thing.
    /// </summary>
    private void Start()
    { RebuildSpriteList(); }

    /// <summary>
    /// Advance the sprite animation process.
    /// </summary>
    private void Update()
    {
        if (mSpriteNames.Count > 1 && Application.isPlaying)
        {
            mDelta += Time.deltaTime;
            float rate = mFPS > 0f ? 1f / mFPS : 0f;

            if (rate < mDelta)
            {
                mDelta = (rate > 0f) ? mDelta - rate : 0f;
                if (++mIndex >= mSpriteNames.Count) mIndex = 0;
                mSprite.spriteName = mSpriteNames[mIndex];
                mSprite.MakePixelPerfect();
            }
        }
    }

    /// <summary>
    /// Rebuild the sprite list after changing the sprite name.
    /// </summary>
    private void RebuildSpriteList()
    {
        if (mSprite == null) mSprite = GetComponent<UISprite>();
        mSpriteNames.Clear();

        if (mSprite != null && mSprite.atlas != null)
        {
            List<UIAtlas.Sprite> sprites = mSprite.atlas.spriteList;

            for (int i = 0, imax = sprites.Count; i < imax; ++i)
            {
                UIAtlas.Sprite sprite = sprites[i];

                if (string.IsNullOrEmpty(mPrefix) || sprite.name.StartsWith(mPrefix))
                {
                    mSpriteNames.Add(sprite.name);
                }
            }
            mSpriteNames.Sort();
        }
    }
}