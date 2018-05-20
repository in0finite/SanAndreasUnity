//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector class used to edit UIPopupLists.
/// </summary>

[CustomEditor(typeof(UIPopupList))]
public class UIPopupListInspector : Editor
{
    private UIPopupList mList;

    private void RegisterUndo()
    {
        NGUIEditorTools.RegisterUndo("Popup List Change", mList);
    }

    private void OnSelectAtlas(MonoBehaviour obj)
    {
        RegisterUndo();
        mList.atlas = obj as UIAtlas;
    }

    private void OnSelectFont(MonoBehaviour obj)
    {
        RegisterUndo();
        mList.font = obj as UIFont;
    }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        mList = target as UIPopupList;

        ComponentSelector.Draw<UIAtlas>(mList.atlas, OnSelectAtlas);
        ComponentSelector.Draw<UIFont>(mList.font, OnSelectFont);

        UILabel lbl = EditorGUILayout.ObjectField("Text Label", mList.textLabel, typeof(UILabel), true) as UILabel;

        if (mList.textLabel != lbl)
        {
            RegisterUndo();
            mList.textLabel = lbl;
            if (lbl != null) lbl.text = mList.selection;
        }

        if (mList.atlas != null)
        {
            string bg = UISpriteInspector.SpriteField(mList.atlas, "Background", mList.backgroundSprite);
            string hl = UISpriteInspector.SpriteField(mList.atlas, "Highlight", mList.highlightSprite);

            if (mList.backgroundSprite != bg || mList.highlightSprite != hl)
            {
                RegisterUndo();
                mList.backgroundSprite = bg;
                mList.highlightSprite = hl;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUILayout.Label("Options");
            GUILayout.EndHorizontal();

            string text = "";
            foreach (string s in mList.items) text += s + "\n";

            GUILayout.Space(-22f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(84f);
            string modified = EditorGUILayout.TextArea(text, GUILayout.Height(100f));
            GUILayout.EndHorizontal();

            if (modified != text)
            {
                RegisterUndo();
                string[] split = modified.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                mList.items.Clear();
                foreach (string s in split) mList.items.Add(s);

                if (string.IsNullOrEmpty(mList.selection) || !mList.items.Contains(mList.selection))
                {
                    mList.selection = mList.items.Count > 0 ? mList.items[0] : "";
                }
            }

            string sel = NGUIEditorTools.DrawList("Selection", mList.items.ToArray(), mList.selection);

            if (mList.selection != sel)
            {
                RegisterUndo();
                mList.selection = sel;
            }

            UIPopupList.Position pos = (UIPopupList.Position)EditorGUILayout.EnumPopup("Position", mList.position);

            if (mList.position != pos)
            {
                RegisterUndo();
                mList.position = pos;
            }

            float ts = EditorGUILayout.FloatField("Text Scale", mList.textScale);
            Color tc = EditorGUILayout.ColorField("Text Color", mList.textColor);
            Color bc = EditorGUILayout.ColorField("Background", mList.backgroundColor);
            Color hc = EditorGUILayout.ColorField("Highlight", mList.highlightColor);

            GUILayout.BeginHorizontal();
            bool isLocalized = EditorGUILayout.Toggle("Localized", mList.isLocalized, GUILayout.Width(100f));
            bool isAnimated = EditorGUILayout.Toggle("Animated", mList.isAnimated);
            GUILayout.EndHorizontal();

            if (mList.textScale != ts ||
                mList.textColor != tc ||
                mList.highlightColor != hc ||
                mList.backgroundColor != bc ||
                mList.isLocalized != isLocalized ||
                mList.isAnimated != isAnimated)
            {
                RegisterUndo();
                mList.textScale = ts;
                mList.textColor = tc;
                mList.backgroundColor = bc;
                mList.highlightColor = hc;
                mList.isLocalized = isLocalized;
                mList.isAnimated = isAnimated;
            }

            NGUIEditorTools.DrawSeparator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(6f);
            GUILayout.Label("Padding", GUILayout.Width(76f));
            GUILayout.BeginVertical();
            GUILayout.Space(-12f);
            Vector2 padding = EditorGUILayout.Vector2Field("", mList.padding);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (mList.padding != padding)
            {
                RegisterUndo();
                mList.padding = padding;
            }

            EditorGUIUtility.LookLikeControls(100f);

            GameObject go = EditorGUILayout.ObjectField("Event Receiver", mList.eventReceiver,
                typeof(GameObject), true) as GameObject;

            string fn = EditorGUILayout.TextField("Function Name", mList.functionName);

            if (mList.eventReceiver != go || mList.functionName != fn)
            {
                RegisterUndo();
                mList.eventReceiver = go;
                mList.functionName = fn;
            }
        }
    }
}