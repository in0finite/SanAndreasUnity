//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Widget that tiles the sprite repeatedly, fully filling the area.
/// Used best with repeating tileable backgrounds.
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Tiled)")]
public class UITiledSprite : UISlicedSprite
{
    /// <summary>
    /// Tiled sprites don't have a border.
    /// </summary>
    public override Vector4 border { get { return Vector4.zero; } }

    /// <summary>
    /// Tiled sprite shouldn't inherit the sprite's changes to this function.
    /// </summary>
    override public void MakePixelPerfect()
    {
        Vector3 pos = cachedTransform.localPosition;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);
        pos.z = Mathf.RoundToInt(pos.z);
        cachedTransform.localPosition = pos;

        Vector3 scale = cachedTransform.localScale;
        scale.x = Mathf.RoundToInt(scale.x);
        scale.y = Mathf.RoundToInt(scale.y);
        scale.z = 1f;
        cachedTransform.localScale = scale;
    }

    /// <summary>
    /// Fill the draw buffers.
    /// </summary>
    public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
    {
        Texture tex = material.mainTexture;
        if (tex == null) return;

        Rect rect = mInner;

        if (atlas.coordinates == UIAtlas.Coordinates.TexCoords)
        {
            rect = NGUIMath.ConvertToPixels(rect, tex.width, tex.height, true);
        }

        Vector2 scale = cachedTransform.localScale;
        float pixelSize = atlas.pixelSize;
        float width = Mathf.Abs(rect.width / scale.x) * pixelSize;
        float height = Mathf.Abs(rect.height / scale.y) * pixelSize;

        // Safety check. Useful so Unity doesn't run out of memory if the sprites are too small.
        if (width < 0.01f || height < 0.01f)
        {
            Debug.LogWarning("The tiled sprite (" + NGUITools.GetHierarchy(gameObject) + ") is too small.\nConsider using a bigger one.");

            width = 0.01f;
            height = 0.01f;
        }

        Vector2 min = new Vector2(rect.xMin / tex.width, rect.yMin / tex.height);
        Vector2 max = new Vector2(rect.xMax / tex.width, rect.yMax / tex.height);

        Vector2 clipped = max;

        float y = 0f;

        while (y < 1f)
        {
            float x = 0f;
            clipped.x = max.x;

            float y2 = y + height;

            if (y2 > 1f)
            {
                clipped.y = min.y + (max.y - min.y) * (1f - y) / (y2 - y);
                y2 = 1f;
            }

            while (x < 1f)
            {
                float x2 = x + width;

                if (x2 > 1f)
                {
                    clipped.x = min.x + (max.x - min.x) * (1f - x) / (x2 - x);
                    x2 = 1f;
                }

                verts.Add(new Vector3(x2, -y, 0f));
                verts.Add(new Vector3(x2, -y2, 0f));
                verts.Add(new Vector3(x, -y2, 0f));
                verts.Add(new Vector3(x, -y, 0f));

                uvs.Add(new Vector2(clipped.x, 1f - min.y));
                uvs.Add(new Vector2(clipped.x, 1f - clipped.y));
                uvs.Add(new Vector2(min.x, 1f - clipped.y));
                uvs.Add(new Vector2(min.x, 1f - min.y));

                cols.Add(color);
                cols.Add(color);
                cols.Add(color);
                cols.Add(color);

                x += width;
            }
            y += height;
        }
    }
}