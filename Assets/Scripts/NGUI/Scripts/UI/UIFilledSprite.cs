//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Similar to a regular UISprite, but lets you only display a part of it. Great for progress bars, sliders, timers, etc.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite (Filled)")]
public class UIFilledSprite : UISprite
{
    public enum FillDirection
    {
        Horizontal,
        Vertical,
        Radial90,
        Radial180,
        Radial360,
    }

    [HideInInspector] [SerializeField] private FillDirection mFillDirection = FillDirection.Radial360;
    [HideInInspector] [SerializeField] private float mFillAmount = 1.0f;
    [HideInInspector] [SerializeField] private bool mInvert = false;

    /// <summary>
    /// Direction of the cut procedure.
    /// </summary>

    public FillDirection fillDirection
    {
        get
        {
            return mFillDirection;
        }
        set
        {
            if (mFillDirection != value)
            {
                mFillDirection = value;
                mChanged = true;
            }
        }
    }

    /// <summary>
    /// Amount of the sprite shown. 0-1 range with 0 being nothing shown, and 1 being the full sprite.
    /// </summary>

    public float fillAmount
    {
        get
        {
            return mFillAmount;
        }
        set
        {
            float val = Mathf.Clamp01(value);

            if (mFillAmount != val)
            {
                mFillAmount = val;
                mChanged = true;
            }
        }
    }

    /// <summary>
    /// Whether the sprite should be filled in the opposite direction.
    /// </summary>

    public bool invert
    {
        get
        {
            return mInvert;
        }
        set
        {
            if (mInvert != value)
            {
                mInvert = value;
                mChanged = true;
            }
        }
    }

    /// <summary>
    /// Adjust the specified quad, making it be radially filled instead.
    /// </summary>

    private bool AdjustRadial(Vector2[] xy, Vector2[] uv, float fill, bool invert)
    {
        // Nothing to fill
        if (fill < 0.001f) return false;

        // Nothing to adjust
        if (!invert && fill > 0.999f) return true;

        // Convert 0-1 value into 0 to 90 degrees angle in radians
        float angle = Mathf.Clamp01(fill);
        if (!invert) angle = 1f - angle;
        angle *= 90f * Mathf.Deg2Rad;

        // Calculate the effective X and Y factors
        float fx = Mathf.Sin(angle);
        float fy = Mathf.Cos(angle);

        // Normalize the result, so it's projected onto the side of the rectangle
        if (fx > fy)
        {
            fy *= 1f / fx;
            fx = 1f;

            if (!invert)
            {
                xy[0].y = Mathf.Lerp(xy[2].y, xy[0].y, fy);
                xy[3].y = xy[0].y;

                uv[0].y = Mathf.Lerp(uv[2].y, uv[0].y, fy);
                uv[3].y = uv[0].y;
            }
        }
        else if (fy > fx)
        {
            fx *= 1f / fy;
            fy = 1f;

            if (invert)
            {
                xy[0].x = Mathf.Lerp(xy[2].x, xy[0].x, fx);
                xy[1].x = xy[0].x;

                uv[0].x = Mathf.Lerp(uv[2].x, uv[0].x, fx);
                uv[1].x = uv[0].x;
            }
        }
        else
        {
            fx = 1f;
            fy = 1f;
        }

        if (invert)
        {
            xy[1].y = Mathf.Lerp(xy[2].y, xy[0].y, fy);
            uv[1].y = Mathf.Lerp(uv[2].y, uv[0].y, fy);
        }
        else
        {
            xy[3].x = Mathf.Lerp(xy[2].x, xy[0].x, fx);
            uv[3].x = Mathf.Lerp(uv[2].x, uv[0].x, fx);
        }
        return true;
    }

    /// <summary>
    /// Helper function that copies the contents of the array, rotated by the specified offset.
    /// </summary>

    private void Rotate(Vector2[] v, int offset)
    {
        for (int i = 0; i < offset; ++i)
        {
            Vector2 v0 = new Vector2(v[3].x, v[3].y);

            v[3].x = v[2].y;
            v[3].y = v[2].x;

            v[2].x = v[1].y;
            v[2].y = v[1].x;

            v[1].x = v[0].y;
            v[1].y = v[0].x;

            v[0].x = v0.y;
            v[0].y = v0.x;
        }
    }

    /// <summary>
    /// Virtual function called by the UIScreen that fills the buffers.
    /// </summary>

    override public void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
    {
        float x0 = 0f;
        float y0 = 0f;
        float x1 = 1f;
        float y1 = -1f;

        float u0 = mOuterUV.xMin;
        float v0 = mOuterUV.yMin;
        float u1 = mOuterUV.xMax;
        float v1 = mOuterUV.yMax;

        // Horizontal and vertical filled sprites are simple -- just end the sprite prematurely
        if (mFillDirection == FillDirection.Horizontal || mFillDirection == FillDirection.Vertical)
        {
            float du = (u1 - u0) * mFillAmount;
            float dv = (v1 - v0) * mFillAmount;

            if (fillDirection == FillDirection.Horizontal)
            {
                if (mInvert)
                {
                    x0 = (1f - mFillAmount);
                    u0 = u1 - du;
                }
                else
                {
                    x1 *= mFillAmount;
                    u1 = u0 + du;
                }
            }
            else if (fillDirection == FillDirection.Vertical)
            {
                if (mInvert)
                {
                    y1 *= mFillAmount;
                    v0 = v1 - dv;
                }
                else
                {
                    y0 = -(1f - mFillAmount);
                    v1 = v0 + dv;
                }
            }
        }

        // Starting quad for the sprite
        Vector2[] xy = new Vector2[4];
        Vector2[] uv = new Vector2[4];

        xy[0] = new Vector2(x1, y0);
        xy[1] = new Vector2(x1, y1);
        xy[2] = new Vector2(x0, y1);
        xy[3] = new Vector2(x0, y0);

        uv[0] = new Vector2(u1, v1);
        uv[1] = new Vector2(u1, v0);
        uv[2] = new Vector2(u0, v0);
        uv[3] = new Vector2(u0, v1);

        if (fillDirection == FillDirection.Radial90)
        {
            // Adjust the quad radially, and if 'false' is returned (it's not visible), just exit
            if (!AdjustRadial(xy, uv, mFillAmount, mInvert)) return;
        }
        else if (fillDirection == FillDirection.Radial180)
        {
            // Working in 0-1 coordinates is easier
            Vector2[] oxy = new Vector2[4];
            Vector2[] ouv = new Vector2[4];

            for (int i = 0; i < 2; ++i)
            {
                oxy[0] = new Vector2(0f, 0f);
                oxy[1] = new Vector2(0f, 1f);
                oxy[2] = new Vector2(1f, 1f);
                oxy[3] = new Vector2(1f, 0f);

                ouv[0] = new Vector2(0f, 0f);
                ouv[1] = new Vector2(0f, 1f);
                ouv[2] = new Vector2(1f, 1f);
                ouv[3] = new Vector2(1f, 0f);

                // Each half must be rotated 90 degrees clockwise in order for it to fill properly
                if (mInvert)
                {
                    if (i > 0)
                    {
                        Rotate(oxy, i);
                        Rotate(ouv, i);
                    }
                }
                else if (i < 1)
                {
                    Rotate(oxy, 1 - i);
                    Rotate(ouv, 1 - i);
                }

                // Each half must fill in only a part of the space
                float x, y;

                if (i == 1)
                {
                    x = mInvert ? 0.5f : 1f;
                    y = mInvert ? 1f : 0.5f;
                }
                else
                {
                    x = mInvert ? 1f : 0.5f;
                    y = mInvert ? 0.5f : 1f;
                }

                oxy[1].y = Mathf.Lerp(x, y, oxy[1].y);
                oxy[2].y = Mathf.Lerp(x, y, oxy[2].y);
                ouv[1].y = Mathf.Lerp(x, y, ouv[1].y);
                ouv[2].y = Mathf.Lerp(x, y, ouv[2].y);

                float amount = (mFillAmount) * 2 - i;
                bool odd = (i % 2) == 1;

                if (AdjustRadial(oxy, ouv, amount, !odd))
                {
                    if (mInvert) odd = !odd;

                    // Add every other side in reverse order so they don't come out backface-culled due to rotation
                    if (odd)
                    {
                        for (int b = 0; b < 4; ++b)
                        {
                            x = Mathf.Lerp(xy[0].x, xy[2].x, oxy[b].x);
                            y = Mathf.Lerp(xy[0].y, xy[2].y, oxy[b].y);

                            float u = Mathf.Lerp(uv[0].x, uv[2].x, ouv[b].x);
                            float v = Mathf.Lerp(uv[0].y, uv[2].y, ouv[b].y);

                            verts.Add(new Vector3(x, y, 0f));
                            uvs.Add(new Vector2(u, v));
                            cols.Add(color);
                        }
                    }
                    else
                    {
                        for (int b = 3; b > -1; --b)
                        {
                            x = Mathf.Lerp(xy[0].x, xy[2].x, oxy[b].x);
                            y = Mathf.Lerp(xy[0].y, xy[2].y, oxy[b].y);

                            float u = Mathf.Lerp(uv[0].x, uv[2].x, ouv[b].x);
                            float v = Mathf.Lerp(uv[0].y, uv[2].y, ouv[b].y);

                            verts.Add(new Vector3(x, y, 0f));
                            uvs.Add(new Vector2(u, v));
                            cols.Add(color);
                        }
                    }
                }
            }
            return;
        }
        else if (fillDirection == FillDirection.Radial360)
        {
            float[] matrix = new float[]
            {
				// x0 y0  x1   y1
				0.5f, 1f, 0f, 0.5f, // quadrant 0
				0.5f, 1f, 0.5f, 1f, // quadrant 1
				0f, 0.5f, 0.5f, 1f, // quadrant 2
				0f, 0.5f, 0f, 0.5f, // quadrant 3
			};

            Vector2[] oxy = new Vector2[4];
            Vector2[] ouv = new Vector2[4];

            for (int i = 0; i < 4; ++i)
            {
                oxy[0] = new Vector2(0f, 0f);
                oxy[1] = new Vector2(0f, 1f);
                oxy[2] = new Vector2(1f, 1f);
                oxy[3] = new Vector2(1f, 0f);

                ouv[0] = new Vector2(0f, 0f);
                ouv[1] = new Vector2(0f, 1f);
                ouv[2] = new Vector2(1f, 1f);
                ouv[3] = new Vector2(1f, 0f);

                // Each quadrant must be rotated 90 degrees clockwise in order for it to fill properly
                if (mInvert)
                {
                    if (i > 0)
                    {
                        Rotate(oxy, i);
                        Rotate(ouv, i);
                    }
                }
                else if (i < 3)
                {
                    Rotate(oxy, 3 - i);
                    Rotate(ouv, 3 - i);
                }

                // Each quadrant must fill in only a quarter of the space
                for (int b = 0; b < 4; ++b)
                {
                    int index = (mInvert) ? (3 - i) * 4 : i * 4;

                    float fx0 = matrix[index];
                    float fy0 = matrix[index + 1];
                    float fx1 = matrix[index + 2];
                    float fy1 = matrix[index + 3];

                    oxy[b].x = Mathf.Lerp(fx0, fy0, oxy[b].x);
                    oxy[b].y = Mathf.Lerp(fx1, fy1, oxy[b].y);
                    ouv[b].x = Mathf.Lerp(fx0, fy0, ouv[b].x);
                    ouv[b].y = Mathf.Lerp(fx1, fy1, ouv[b].y);
                }

                float amount = (mFillAmount) * 4 - i;
                bool odd = (i % 2) == 1;

                if (AdjustRadial(oxy, ouv, amount, !odd))
                {
                    if (mInvert) odd = !odd;

                    // Add every other side in reverse order so they don't come out backface-culled due to rotation
                    if (odd)
                    {
                        for (int b = 0; b < 4; ++b)
                        {
                            float x = Mathf.Lerp(xy[0].x, xy[2].x, oxy[b].x);
                            float y = Mathf.Lerp(xy[0].y, xy[2].y, oxy[b].y);
                            float u = Mathf.Lerp(uv[0].x, uv[2].x, ouv[b].x);
                            float v = Mathf.Lerp(uv[0].y, uv[2].y, ouv[b].y);

                            verts.Add(new Vector3(x, y, 0f));
                            uvs.Add(new Vector2(u, v));
                            cols.Add(color);
                        }
                    }
                    else
                    {
                        for (int b = 3; b > -1; --b)
                        {
                            float x = Mathf.Lerp(xy[0].x, xy[2].x, oxy[b].x);
                            float y = Mathf.Lerp(xy[0].y, xy[2].y, oxy[b].y);
                            float u = Mathf.Lerp(uv[0].x, uv[2].x, ouv[b].x);
                            float v = Mathf.Lerp(uv[0].y, uv[2].y, ouv[b].y);

                            verts.Add(new Vector3(x, y, 0f));
                            uvs.Add(new Vector2(u, v));
                            cols.Add(color);
                        }
                    }
                }
            }
            return;
        }

        // Fill the buffer with the quad for the sprite
        for (int i = 0; i < 4; ++i)
        {
            verts.Add(xy[i]);
            uvs.Add(uv[i]);
            cols.Add(color);
        }
    }
}