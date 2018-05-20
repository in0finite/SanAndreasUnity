//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UIWidgets.
/// </summary>

[CustomEditor(typeof(UIWidget))]
public class UIWidgetInspector : Editor
{
    protected UIWidget mWidget;
    static protected bool mUseShader = false;

    private bool mInitialized = false;
    protected bool mAllowPreview = true;

    /// <summary>
    /// Register an Undo command with the Unity editor.
    /// </summary>

    private void RegisterUndo()
    {
        NGUIEditorTools.RegisterUndo("Widget Change", mWidget);
    }

    /// <summary>
    /// Draw the inspector widget.
    /// </summary>

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        mWidget = target as UIWidget;

        if (!mInitialized)
        {
            mInitialized = true;
            OnInit();
        }

        NGUIEditorTools.DrawSeparator();

        // Check to see if we can draw the widget's default properties to begin with
        if (OnDrawProperties())
        {
            // Draw all common properties next
            DrawCommonProperties();
        }
    }

    /// <summary>
    /// All widgets have depth, color and make pixel-perfect options
    /// </summary>

    protected void DrawCommonProperties()
    {
#if UNITY_3_4
		PrefabType type = EditorUtility.GetPrefabType(mWidget.gameObject);
#else
        PrefabType type = PrefabUtility.GetPrefabType(mWidget.gameObject);
#endif

        NGUIEditorTools.DrawSeparator();

        // Depth navigation
        if (type != PrefabType.Prefab)
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Depth");

                int depth = mWidget.depth;
                if (GUILayout.Button("Back")) --depth;
                depth = EditorGUILayout.IntField(depth, GUILayout.Width(40f));
                if (GUILayout.Button("Forward")) ++depth;

                if (mWidget.depth != depth)
                {
                    NGUIEditorTools.RegisterUndo("Depth Change", mWidget);
                    mWidget.depth = depth;
                }
            }
            GUILayout.EndHorizontal();
        }

        Color color = EditorGUILayout.ColorField("Color Tint", mWidget.color);

        if (mWidget.color != color)
        {
            NGUIEditorTools.RegisterUndo("Color Change", mWidget);
            mWidget.color = color;
        }

        // Depth navigation
        if (type != PrefabType.Prefab)
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Correction");

                if (GUILayout.Button("Make Pixel-Perfect"))
                {
                    NGUIEditorTools.RegisterUndo("Make Pixel-Perfect", mWidget.transform);
                    mWidget.MakePixelPerfect();
                }
            }
            GUILayout.EndHorizontal();
        }

        UIWidget.Pivot pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", mWidget.pivot);

        if (mWidget.pivot != pivot)
        {
            NGUIEditorTools.RegisterUndo("Pivot Change", mWidget);
            mWidget.pivot = pivot;
        }

        if (mAllowPreview && mWidget.mainTexture != null)
        {
            GUILayout.BeginHorizontal();
            {
                UISettings.texturePreview = EditorGUILayout.Toggle("Preview", UISettings.texturePreview, GUILayout.Width(100f));

                /*if (UISettings.texturePreview)
				{
					if (mUseShader != EditorGUILayout.Toggle("Use Shader", mUseShader))
					{
						mUseShader = !mUseShader;

						if (mUseShader)
						{
							// TODO: Remove this when Unity fixes the bug with DrawPreviewTexture not being affected by BeginGroup
							Debug.LogWarning("There is a bug in Unity that prevents the texture from getting clipped properly.\n" +
								"Until it's fixed by Unity, your texture may spill onto the rest of the Unity's GUI while using this mode.");
						}
					}
				}*/
            }
            GUILayout.EndHorizontal();

            // Draw the texture last
            if (UISettings.texturePreview) OnDrawTexture();
        }
    }

    /// <summary>
    /// Any and all derived functionality.
    /// </summary>

    protected virtual void OnInit()
    {
    }

    protected virtual bool OnDrawProperties()
    {
        return true;
    }

    protected virtual void OnDrawTexture()
    {
    }
}