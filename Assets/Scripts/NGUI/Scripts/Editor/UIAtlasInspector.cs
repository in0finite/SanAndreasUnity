//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit the UIAtlas.
/// </summary>
[CustomEditor(typeof(UIAtlas))]
public class UIAtlasInspector : Editor
{
    private enum View
    {
        Atlas,
        Sprite,
    }

    private enum AtlasType
    {
        Normal,
        Reference,
    }

    private static View mView = View.Sprite;
    private static bool mUseShader = false;

    private UIAtlas mAtlas;
    private bool mConfirmDelete = false;
    private UIAtlas.Sprite mSprite;

    private AtlasType mType = AtlasType.Normal;
    private UIAtlas mReplacement = null;

    /// <summary>
    /// Convenience function -- mark all widgets using the sprite as changed.
    /// </summary>
    private void MarkSpriteAsDirty()
    {
        if (mSprite == null) return;

        UISprite[] sprites = NGUITools.FindActive<UISprite>();

        foreach (UISprite sp in sprites)
        {
            if (sp.spriteName == mSprite.name)
            {
                sp.atlas = null;
                sp.atlas = mAtlas;
                EditorUtility.SetDirty(sp);
            }
        }

        UILabel[] labels = NGUITools.FindActive<UILabel>();

        foreach (UILabel lbl in labels)
        {
            if (lbl.font != null && UIAtlas.CheckIfRelated(lbl.font.atlas, mAtlas) && lbl.font.spriteName == mSprite.name)
            {
                UIBaseFont font = lbl.font;
                lbl.font = null;
                lbl.font = font;
                EditorUtility.SetDirty(lbl);
            }
        }
    }

    /// <summary>
    /// Replacement atlas selection callback.
    /// </summary>
    private void OnSelectAtlas(MonoBehaviour obj)
    {
        if (mReplacement != obj)
        {
            // Undo doesn't work correctly in this case... so I won't bother.
            //NGUIEditorTools.RegisterUndo("Atlas Change");
            //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);

            mAtlas.replacement = obj as UIAtlas;
            mReplacement = mAtlas.replacement as UIAtlas;
            UnityEditor.EditorUtility.SetDirty(mAtlas);
            if (mReplacement == null) mType = AtlasType.Normal;
        }
    }

    /// <summary>
    /// Draw the inspector widget.
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        mAtlas = target as UIAtlas;

        NGUIEditorTools.DrawSeparator();

        if (mAtlas.replacement != null)
        {
            mType = AtlasType.Reference;
            mReplacement = mAtlas.replacement as UIAtlas;
        }

        AtlasType after = (AtlasType)EditorGUILayout.EnumPopup("Atlas Type", mType);

        if (mType != after)
        {
            if (after == AtlasType.Normal)
            {
                OnSelectAtlas(null);
            }
            else
            {
                mType = AtlasType.Reference;
            }
        }

        if (mType == AtlasType.Reference)
        {
            ComponentSelector.Draw<UIAtlas>(mAtlas.replacement as UIAtlas, OnSelectAtlas);

            NGUIEditorTools.DrawSeparator();
            GUILayout.Label("You can have one atlas simply point to\n" +
                "another one. This is useful if you want to be\n" +
                "able to quickly replace the contents of one\n" +
                "atlas with another one, for example for\n" +
                "swapping an SD atlas with an HD one, or\n" +
                "replacing an English atlas with a Chinese\n" +
                "one. All the sprites referencing this atlas\n" +
                "will update their references to the new one.");

            if (mReplacement != mAtlas && mAtlas.replacement != mReplacement)
            {
                NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                mAtlas.replacement = mReplacement;
                UnityEditor.EditorUtility.SetDirty(mAtlas);
            }
            return;
        }

        if (!mConfirmDelete)
        {
            NGUIEditorTools.DrawSeparator();
            Material mat = EditorGUILayout.ObjectField("Material", mAtlas.spriteMaterial, typeof(Material), false) as Material;

            if (mAtlas.spriteMaterial != mat)
            {
                NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                mAtlas.spriteMaterial = mat;

                // Ensure that this atlas has valid import settings
                if (mAtlas.texture != null) NGUIEditorTools.ImportTexture(mAtlas.texture, false, false);

                mAtlas.MarkAsDirty();
                mConfirmDelete = false;
            }

            if (mat != null)
            {
                TextAsset ta = EditorGUILayout.ObjectField("TP Import", null, typeof(TextAsset), false) as TextAsset;

                if (ta != null)
                {
                    // Ensure that this atlas has valid import settings
                    if (mAtlas.texture != null) NGUIEditorTools.ImportTexture(mAtlas.texture, false, false);

                    NGUIEditorTools.RegisterUndo("Import Sprites", mAtlas);
                    NGUIJson.LoadSpriteData(mAtlas, ta);
                    if (mSprite != null) mSprite = mAtlas.GetSprite(mSprite.name);
                    mAtlas.MarkAsDirty();
                }

                UIAtlas.Coordinates coords = (UIAtlas.Coordinates)EditorGUILayout.EnumPopup("Coordinates", mAtlas.coordinates);

                if (coords != mAtlas.coordinates)
                {
                    NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                    mAtlas.coordinates = coords;
                    mConfirmDelete = false;
                }

                float pixelSize = EditorGUILayout.FloatField("Pixel Size", mAtlas.pixelSize);

                if (pixelSize != mAtlas.pixelSize)
                {
                    NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                    mAtlas.pixelSize = pixelSize;
                    mConfirmDelete = false;
                }
            }
        }

        if (mAtlas.spriteMaterial != null)
        {
            Color blue = new Color(0f, 0.7f, 1f, 1f);
            Color green = new Color(0.4f, 1f, 0f, 1f);

            if (mConfirmDelete)
            {
                if (mSprite != null)
                {
                    // Show the confirmation dialog
                    NGUIEditorTools.DrawSeparator();
                    GUILayout.Label("Are you sure you want to delete '" + mSprite.name + "'?");
                    NGUIEditorTools.DrawSeparator();

                    GUILayout.BeginHorizontal();
                    {
                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("Cancel")) mConfirmDelete = false;
                        GUI.backgroundColor = Color.red;

                        if (GUILayout.Button("Delete"))
                        {
                            NGUIEditorTools.RegisterUndo("Delete Sprite", mAtlas);
                            mAtlas.spriteList.Remove(mSprite);
                            mConfirmDelete = false;
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    GUILayout.EndHorizontal();
                }
                else mConfirmDelete = false;
            }
            else
            {
                if (mSprite == null && mAtlas.spriteList.Count > 0)
                {
                    string spriteName = EditorPrefs.GetString("NGUI Selected Sprite");
                    if (!string.IsNullOrEmpty(spriteName)) mSprite = mAtlas.GetSprite(spriteName);
                    if (mSprite == null) mSprite = mAtlas.spriteList[0];
                }

                GUI.backgroundColor = Color.green;

                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PrefixLabel("Add/Delete");

                    if (GUILayout.Button("New Sprite"))
                    {
                        NGUIEditorTools.RegisterUndo("Add Sprite", mAtlas);
                        UIAtlas.Sprite newSprite = new UIAtlas.Sprite();

                        if (mSprite != null)
                        {
                            newSprite.name = "Copy of " + mSprite.name;
                            newSprite.outer = mSprite.outer;
                            newSprite.inner = mSprite.inner;
                        }
                        else
                        {
                            newSprite.name = "New Sprite";
                        }

                        mAtlas.spriteList.Add(newSprite);
                        mSprite = newSprite;
                    }

                    // Show the delete button
                    GUI.backgroundColor = Color.red;

                    if (mSprite != null && GUILayout.Button("Delete", GUILayout.Width(55f)))
                    {
                        mConfirmDelete = true;
                    }
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.EndHorizontal();

                if (!mConfirmDelete && mSprite != null)
                {
                    NGUIEditorTools.DrawSeparator();

                    string spriteName = UISpriteInspector.SpriteField(mAtlas, mSprite.name);

                    if (spriteName != mSprite.name)
                    {
                        mSprite = mAtlas.GetSprite(spriteName);
                        EditorPrefs.SetString("NGUI Selected Sprite", spriteName);
                    }

                    if (mSprite == null) return;

                    Texture2D tex = mAtlas.spriteMaterial.mainTexture as Texture2D;

                    if (tex != null)
                    {
                        Rect inner = mSprite.inner;
                        Rect outer = mSprite.outer;

                        string name = EditorGUILayout.TextField("Edit Name", mSprite.name);

                        if (mSprite.name != name && !string.IsNullOrEmpty(name))
                        {
                            bool found = false;

                            foreach (UIAtlas.Sprite sp in mAtlas.spriteList)
                            {
                                if (sp.name == name)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                NGUIEditorTools.RegisterUndo("Edit Sprite Name", mAtlas);
                                mSprite.name = name;
                            }
                        }

                        if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
                        {
                            GUI.backgroundColor = green;
                            outer = NGUIEditorTools.IntRect("Dimensions", mSprite.outer);

                            Vector4 border = new Vector4(
                                mSprite.inner.xMin - mSprite.outer.xMin,
                                mSprite.inner.yMin - mSprite.outer.yMin,
                                mSprite.outer.xMax - mSprite.inner.xMax,
                                mSprite.outer.yMax - mSprite.inner.yMax);

                            GUI.backgroundColor = blue;
                            border = NGUIEditorTools.IntPadding("Border", border);
                            GUI.backgroundColor = Color.white;

                            inner.xMin = mSprite.outer.xMin + border.x;
                            inner.yMin = mSprite.outer.yMin + border.y;
                            inner.xMax = mSprite.outer.xMax - border.z;
                            inner.yMax = mSprite.outer.yMax - border.w;
                        }
                        else
                        {
                            // Draw the inner and outer rectangle dimensions
                            GUI.backgroundColor = green;
                            outer = EditorGUILayout.RectField("Outer Rect", mSprite.outer);
                            GUI.backgroundColor = blue;
                            inner = EditorGUILayout.RectField("Inner Rect", mSprite.inner);
                            GUI.backgroundColor = Color.white;
                        }

                        if (outer.xMax < outer.xMin) outer.xMax = outer.xMin;
                        if (outer.yMax < outer.yMin) outer.yMax = outer.yMin;

                        if (outer != mSprite.outer)
                        {
                            float x = outer.xMin - mSprite.outer.xMin;
                            float y = outer.yMin - mSprite.outer.yMin;

                            inner.x += x;
                            inner.y += y;
                        }

                        // Sanity checks to ensure that the inner rect is always inside the outer
                        inner.xMin = Mathf.Clamp(inner.xMin, outer.xMin, outer.xMax);
                        inner.xMax = Mathf.Clamp(inner.xMax, outer.xMin, outer.xMax);
                        inner.yMin = Mathf.Clamp(inner.yMin, outer.yMin, outer.yMax);
                        inner.yMax = Mathf.Clamp(inner.yMax, outer.yMin, outer.yMax);

                        EditorGUILayout.Separator();

                        // Padding is mainly meant to be used by the 'trimmed' feature of TexturePacker
                        if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
                        {
                            int left = Mathf.RoundToInt(mSprite.paddingLeft * mSprite.outer.width);
                            int right = Mathf.RoundToInt(mSprite.paddingRight * mSprite.outer.width);
                            int top = Mathf.RoundToInt(mSprite.paddingTop * mSprite.outer.height);
                            int bottom = Mathf.RoundToInt(mSprite.paddingBottom * mSprite.outer.height);

                            NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair("Padding", "Left", "Top", left, top);
                            NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Right", "Bottom", right, bottom);

                            if (a.x != left || a.y != top || b.x != right || b.y != bottom)
                            {
                                NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                                mSprite.paddingLeft = a.x / mSprite.outer.width;
                                mSprite.paddingTop = a.y / mSprite.outer.width;
                                mSprite.paddingRight = b.x / mSprite.outer.height;
                                mSprite.paddingBottom = b.y / mSprite.outer.height;
                                MarkSpriteAsDirty();
                            }
                        }
                        else
                        {
                            // Create a button that can make the coordinates pixel-perfect on click
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Correction", GUILayout.Width(75f));

                                Rect corrected0 = outer;
                                Rect corrected1 = inner;

                                if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
                                {
                                    corrected0 = NGUIMath.MakePixelPerfect(corrected0);
                                    corrected1 = NGUIMath.MakePixelPerfect(corrected1);
                                }
                                else
                                {
                                    corrected0 = NGUIMath.MakePixelPerfect(corrected0, tex.width, tex.height);
                                    corrected1 = NGUIMath.MakePixelPerfect(corrected1, tex.width, tex.height);
                                }

                                if (corrected0 == mSprite.outer && corrected1 == mSprite.inner)
                                {
                                    GUI.color = Color.grey;
                                    GUILayout.Button("Make Pixel-Perfect");
                                    GUI.color = Color.white;
                                }
                                else if (GUILayout.Button("Make Pixel-Perfect"))
                                {
                                    outer = corrected0;
                                    inner = corrected1;
                                    GUI.changed = true;
                                }
                            }
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.BeginHorizontal();
                        {
                            mView = (View)EditorGUILayout.EnumPopup("Show", mView);
                            GUILayout.Label("Shader", GUILayout.Width(45f));

                            if (mUseShader != EditorGUILayout.Toggle(mUseShader, GUILayout.Width(20f)))
                            {
                                mUseShader = !mUseShader;

                                if (mUseShader && mView == View.Sprite)
                                {
                                    // TODO: Remove this when Unity fixes the bug with DrawPreviewTexture not being affected by BeginGroup
                                    Debug.LogWarning("There is a bug in Unity that prevents the texture from getting clipped properly.\n" +
                                        "Until it's fixed by Unity, your texture may spill onto the rest of the Unity's GUI while using this mode.");
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (mSprite.outer != outer || mSprite.inner != inner)
                        {
                            NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                            mSprite.outer = outer;
                            mSprite.inner = inner;
                            mConfirmDelete = false;
                            MarkSpriteAsDirty();
                        }

                        Rect uv0 = outer;
                        Rect uv1 = inner;

                        if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
                        {
                            uv0 = NGUIMath.ConvertToTexCoords(uv0, tex.width, tex.height);
                            uv1 = NGUIMath.ConvertToTexCoords(uv1, tex.width, tex.height);
                        }

                        // Draw the atlas
                        EditorGUILayout.Separator();
                        Material m = mUseShader ? mAtlas.spriteMaterial : null;
                        Rect rect = (mView == View.Atlas) ? NGUIEditorTools.DrawAtlas(tex, m) : NGUIEditorTools.DrawSprite(tex, uv0, m);

                        // Draw the sprite outline
                        NGUIEditorTools.DrawOutline(rect, uv0, uv1);
                        EditorGUILayout.Separator();
                    }
                }
            }
        }
    }
}