//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// 9-sliced widget component used to draw large widgets using small textures.
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Sliced)")]
public class UISlicedSprite : UISprite
{
    [HideInInspector] [SerializeField] private bool mFillCenter = true;

    protected Rect mInner;
    protected Rect mInnerUV;
    protected Vector3 mScale = Vector3.one;

    /// <summary>
    /// Inner set of UV coordinates.
    /// </summary>
    public Rect innerUV
    {
        get
        {
            UpdateUVs(false);
            return mInnerUV;
        }
    }

    /// <summary>
    /// Whether the center part of the sprite will be filled or not. Turn it off if you want only to borders to show up.
    /// </summary>
    public bool fillCenter { get { return mFillCenter; } set { if (mFillCenter != value) { mFillCenter = value; MarkAsChanged(); } } }

    /// <summary>
    /// Sliced sprites generally have a border.
    /// </summary>
    public override Vector4 border
    {
        get
        {
            UIAtlas.Sprite sp = sprite;
            if (sp == null) return Vector2.zero;

            Rect outer = sp.outer;
            Rect inner = sp.inner;

            Texture tex = mainTexture;

            if (atlas.coordinates == UIAtlas.Coordinates.TexCoords && tex != null)
            {
                outer = NGUIMath.ConvertToPixels(outer, tex.width, tex.height, true);
                inner = NGUIMath.ConvertToPixels(inner, tex.width, tex.height, true);
            }
            return new Vector4(inner.xMin - outer.xMin, inner.yMin - outer.yMin, outer.xMax - inner.xMax, outer.yMax - inner.yMax) * atlas.pixelSize;
        }
    }

    /// <summary>
    /// Update the texture UVs used by the widget.
    /// </summary>
    override public void UpdateUVs(bool force)
    {
        if (cachedTransform.localScale != mScale)
        {
            mScale = cachedTransform.localScale;
            mChanged = true;
        }

        if (sprite != null && (force || mInner != mSprite.inner || mOuter != mSprite.outer))
        {
            Texture tex = mainTexture;

            if (tex != null)
            {
                mInner = mSprite.inner;
                mOuter = mSprite.outer;

                mInnerUV = mInner;
                mOuterUV = mOuter;

                if (atlas.coordinates == UIAtlas.Coordinates.Pixels)
                {
                    mOuterUV = NGUIMath.ConvertToTexCoords(mOuterUV, tex.width, tex.height);
                    mInnerUV = NGUIMath.ConvertToTexCoords(mInnerUV, tex.width, tex.height);
                }
            }
        }
    }

    /// <summary>
    /// Sliced sprite shouldn't inherit the sprite's changes to this function.
    /// </summary>
    override public void MakePixelPerfect()
    {
        Vector3 pos = cachedTransform.localPosition;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);
        pos.z = Mathf.RoundToInt(pos.z);
        cachedTransform.localPosition = pos;

        Vector3 scale = cachedTransform.localScale;
        scale.x = Mathf.RoundToInt(scale.x * 0.5f) << 1;
        scale.y = Mathf.RoundToInt(scale.y * 0.5f) << 1;
        scale.z = 1f;
        cachedTransform.localScale = scale;
    }

    /// <summary>
    /// Draw the widget.
    /// </summary>
    override public void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
    {
        if (mOuterUV == mInnerUV)
        {
            base.OnFill(verts, uvs, cols);
            return;
        }

        Vector2[] v = new Vector2[4];
        Vector2[] uv = new Vector2[4];

        Texture tex = mainTexture;

        v[0] = Vector2.zero;
        v[1] = Vector2.zero;
        v[2] = new Vector2(1f, -1f);
        v[3] = new Vector2(1f, -1f);

        if (tex != null)
        {
            float pixelSize = atlas.pixelSize;
            float borderLeft = (mInnerUV.xMin - mOuterUV.xMin) * pixelSize;
            float borderRight = (mOuterUV.xMax - mInnerUV.xMax) * pixelSize;
            float borderTop = (mInnerUV.yMax - mOuterUV.yMax) * pixelSize;
            float borderBottom = (mOuterUV.yMin - mInnerUV.yMin) * pixelSize;

            Vector3 scale = cachedTransform.localScale;
            scale.x = Mathf.Max(0f, scale.x);
            scale.y = Mathf.Max(0f, scale.y);

            Vector2 sz = new Vector2(scale.x / tex.width, scale.y / tex.height);
            Vector2 tl = new Vector2(borderLeft / sz.x, borderTop / sz.y);
            Vector2 br = new Vector2(borderRight / sz.x, borderBottom / sz.y);

            Pivot pv = pivot;

            // We don't want the sliced sprite to become smaller than the summed up border size
            if (pv == Pivot.Right || pv == Pivot.TopRight || pv == Pivot.BottomRight)
            {
                v[0].x = Mathf.Min(0f, 1f - (br.x + tl.x));
                v[1].x = v[0].x + tl.x;
                v[2].x = v[0].x + Mathf.Max(tl.x, 1f - br.x);
                v[3].x = v[0].x + Mathf.Max(tl.x + br.x, 1f);
            }
            else
            {
                v[1].x = tl.x;
                v[2].x = Mathf.Max(tl.x, 1f - br.x);
                v[3].x = Mathf.Max(tl.x + br.x, 1f);
            }

            if (pv == Pivot.Bottom || pv == Pivot.BottomLeft || pv == Pivot.BottomRight)
            {
                v[0].y = Mathf.Max(0f, -1f - (br.y + tl.y));
                v[1].y = v[0].y + tl.y;
                v[2].y = v[0].y + Mathf.Min(tl.y, -1f - br.y);
                v[3].y = v[0].y + Mathf.Min(tl.y + br.y, -1f);
            }
            else
            {
                v[1].y = tl.y;
                v[2].y = Mathf.Min(tl.y, -1f - br.y);
                v[3].y = Mathf.Min(tl.y + br.y, -1f);
            }

            uv[0] = new Vector2(mOuterUV.xMin, mOuterUV.yMax);
            uv[1] = new Vector2(mInnerUV.xMin, mInnerUV.yMax);
            uv[2] = new Vector2(mInnerUV.xMax, mInnerUV.yMin);
            uv[3] = new Vector2(mOuterUV.xMax, mOuterUV.yMin);
        }
        else
        {
            // No texture -- just use zeroed out texture coordinates
            for (int i = 0; i < 4; ++i) uv[i] = Vector2.zero;
        }

        for (int x = 0; x < 3; ++x)
        {
            int x2 = x + 1;

            for (int y = 0; y < 3; ++y)
            {
                if (!mFillCenter && x == 1 && y == 1) continue;

                int y2 = y + 1;

                verts.Add(new Vector3(v[x2].x, v[y].y, 0f));
                verts.Add(new Vector3(v[x2].x, v[y2].y, 0f));
                verts.Add(new Vector3(v[x].x, v[y2].y, 0f));
                verts.Add(new Vector3(v[x].x, v[y].y, 0f));

                uvs.Add(new Vector2(uv[x2].x, uv[y].y));
                uvs.Add(new Vector2(uv[x2].x, uv[y2].y));
                uvs.Add(new Vector2(uv[x].x, uv[y2].y));
                uvs.Add(new Vector2(uv[x].x, uv[y].y));

                cols.Add(color);
                cols.Add(color);
                cols.Add(color);
                cols.Add(color);
            }
        }
    }
}