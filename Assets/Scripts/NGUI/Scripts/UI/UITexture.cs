//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// If you don't have or don't wish to create an atlas, you can simply use this script to draw a texture.
/// Keep in mind though that this will create an extra draw call with each UITexture present, so it's
/// best to use it only for backgrounds or temporary visible widgets.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Texture")]
public class UITexture : UIWidget
{
    /// <summary>
    /// UI textures should keep the material reference.
    /// </summary>

    public override bool keepMaterial { get { return true; } }

    /// <summary>
    /// Adjust the scale of the widget to make it pixel-perfect.
    /// </summary>

    override public void MakePixelPerfect()
    {
        Texture tex = mainTexture;

        if (tex != null)
        {
            Vector3 scale = cachedTransform.localScale;
            scale.x = tex.width;
            scale.y = tex.height;
            scale.z = 1f;
            cachedTransform.localScale = scale;
        }
        base.MakePixelPerfect();
    }

    /// <summary>
    /// Virtual function called by the UIScreen that fills the buffers.
    /// </summary>

    override public void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
    {
        verts.Add(new Vector3(1f, 0f, 0f));
        verts.Add(new Vector3(1f, -1f, 0f));
        verts.Add(new Vector3(0f, -1f, 0f));
        verts.Add(new Vector3(0f, 0f, 0f));

        uvs.Add(Vector2.one);
        uvs.Add(new Vector2(1f, 0f));
        uvs.Add(Vector2.zero);
        uvs.Add(new Vector2(0f, 1f));

        cols.Add(color);
        cols.Add(color);
        cols.Add(color);
        cols.Add(color);
    }
}