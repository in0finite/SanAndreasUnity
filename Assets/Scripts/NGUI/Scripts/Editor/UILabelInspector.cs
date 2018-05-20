//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
    private UILabel mLabel;

    /// <summary>
    /// Register an Undo command with the Unity editor.
    /// </summary>

    private void RegisterUndo()
    { NGUIEditorTools.RegisterUndo("Label Change", mLabel); }

    /// <summary>
    /// Font selection callback.
    /// </summary>

    private void OnSelectFont(MonoBehaviour obj)
    {
        if (mLabel != null)
        {
            NGUIEditorTools.RegisterUndo("Font Selection", mLabel);
            bool resize = (mLabel.font == null);
            mLabel.font = obj as UIFont;
            if (resize) mLabel.MakePixelPerfect();
        }
    }

    override protected void OnInit()
    {
        mAllowPreview = false;
    }

    override protected bool OnDrawProperties()
    {
        mLabel = mWidget as UILabel;
        ComponentSelector.Draw<UIFont>(mLabel.font as UIFont, OnSelectFont);
        if (mLabel.font == null) return false;

        GUI.skin.textArea.wordWrap = true;
        string text = string.IsNullOrEmpty(mLabel.text) ? "" : mLabel.text;
        text = EditorGUILayout.TextArea(mLabel.text, GUI.skin.textArea, GUILayout.Height(100f));
        if (!text.Equals(mLabel.text)) { RegisterUndo(); mLabel.text = text; }

        GUILayout.BeginHorizontal();
        {
            int len = EditorGUILayout.IntField("Line Width", mLabel.lineWidth, GUILayout.Width(120f));
            if (len != mLabel.lineWidth) { RegisterUndo(); mLabel.lineWidth = len; }

            bool multi = EditorGUILayout.Toggle("Multi-line", mLabel.multiLine, GUILayout.Width(100f));
            if (multi != mLabel.multiLine) { RegisterUndo(); mLabel.multiLine = multi; }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        bool password = EditorGUILayout.Toggle("Password", mLabel.password, GUILayout.Width(120f));
        if (password != mLabel.password) { RegisterUndo(); mLabel.password = password; }

        bool encoding = EditorGUILayout.Toggle("Encoding", mLabel.supportEncoding, GUILayout.Width(100f));
        if (encoding != mLabel.supportEncoding) { RegisterUndo(); mLabel.supportEncoding = encoding; }

        GUILayout.EndHorizontal();

        if (encoding)
        {
            UIFont.SymbolStyle sym = (UIFont.SymbolStyle)EditorGUILayout.EnumPopup("Symbols", mLabel.symbolStyle, GUILayout.Width(170f));
            if (sym != mLabel.symbolStyle) { RegisterUndo(); mLabel.symbolStyle = sym; }
        }

        GUILayout.BeginHorizontal();
        {
            UILabel.Effect effect = (UILabel.Effect)EditorGUILayout.EnumPopup("Effect", mLabel.effectStyle, GUILayout.Width(170f));
            if (effect != mLabel.effectStyle) { RegisterUndo(); mLabel.effectStyle = effect; }

            if (effect != UILabel.Effect.None)
            {
                Color c = EditorGUILayout.ColorField(mLabel.effectColor);
                if (mLabel.effectColor != c) { RegisterUndo(); mLabel.effectColor = c; }
            }
        }
        GUILayout.EndHorizontal();
        return true;
    }

    override protected void OnDrawTexture()
    {
        Texture2D tex = mLabel.mainTexture as Texture2D;

        if (tex != null)
        {
            // Draw the atlas
            EditorGUILayout.Separator();
            NGUIEditorTools.DrawSprite(tex, mLabel.font.uvRect, mUseShader ? mLabel.font.material : null);

            // Sprite size label
            Rect rect = GUILayoutUtility.GetRect(Screen.width, 18f);
            EditorGUI.DropShadowLabel(rect, "Font Size: " + mLabel.font.size);
        }
    }
}