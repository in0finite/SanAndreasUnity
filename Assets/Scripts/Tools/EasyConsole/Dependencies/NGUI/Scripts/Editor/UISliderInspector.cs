//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UISlider))]
public class UISliderInspector : Editor
{
    private bool mShowWarning = false;

    private void ValidatePivot(Transform fg, string name, UISlider.Direction dir)
    {
        if (fg != null)
        {
            UIWidget widget = fg.GetComponent<UIWidget>();

            if (widget != null && !(widget is UIFilledSprite))
            {
                if (dir == UISlider.Direction.Horizontal)
                {
                    if (widget.pivot != UIWidget.Pivot.Left &&
                        widget.pivot != UIWidget.Pivot.TopLeft &&
                        widget.pivot != UIWidget.Pivot.BottomLeft)
                    {
                        GUI.color = new Color(1f, 0.7f, 0f);
                        GUILayout.Label(name + " should use a Left pivot");
                        GUI.color = Color.white;
                    }
                }
                else if (widget.pivot != UIWidget.Pivot.BottomLeft &&
                         widget.pivot != UIWidget.Pivot.Bottom &&
                         widget.pivot != UIWidget.Pivot.BottomRight)
                {
                    GUI.color = new Color(1f, 0.7f, 0f);
                    GUILayout.Label(name + " should use a Bottom pivot");
                    GUI.color = Color.white;
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        UISlider slider = target as UISlider;

        NGUIEditorTools.DrawSeparator();

        float sliderValue = EditorGUILayout.Slider("Value", slider.sliderValue, 0f, 1f);

        if (slider.sliderValue != sliderValue)
        {
            NGUIEditorTools.RegisterUndo("Slider Change", slider);
            slider.sliderValue = sliderValue;
            UnityEditor.EditorUtility.SetDirty(slider);
        }

        int steps = EditorGUILayout.IntSlider("Steps", slider.numberOfSteps, 0, 11);

        if (slider.numberOfSteps != steps)
        {
            NGUIEditorTools.RegisterUndo("Slider Change", slider);
            slider.numberOfSteps = steps;
            slider.ForceUpdate();
            UnityEditor.EditorUtility.SetDirty(slider);
        }

        NGUIEditorTools.DrawSeparator();

        Vector2 size = slider.fullSize;

        GUILayout.Label(" Size");
        GUILayout.Space(-36f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(66f);
        size = EditorGUILayout.Vector2Field("", size);
        GUILayout.Space(18f);
        GUILayout.EndHorizontal();

        if (mShowWarning && slider.foreground != null)
        {
            UIWidget widget = slider.foreground.GetComponent<UIWidget>();

            if (widget != null && !(widget is UIFilledSprite))
            {
                GUI.color = new Color(1f, 0.7f, 0f);
                GUILayout.Label("Don't forget to adjust the background as well");
                GUILayout.Label("(the slider doesn't know what it is)");
                GUI.color = Color.white;
            }
        }

        Transform fg = EditorGUILayout.ObjectField("Foreground", slider.foreground, typeof(Transform), true) as Transform;
        Transform tb = EditorGUILayout.ObjectField("Thumb", slider.thumb, typeof(Transform), true) as Transform;
        UISlider.Direction dir = (UISlider.Direction)EditorGUILayout.EnumPopup("Direction", slider.direction);

        // If we're using a sprite for the foreground, ensure it's using a proper pivot.
        ValidatePivot(fg, "Foreground sprite", dir);

        NGUIEditorTools.DrawSeparator();

        GameObject er = EditorGUILayout.ObjectField("Event Recv.", slider.eventReceiver, typeof(GameObject), true) as GameObject;

        GUILayout.BeginHorizontal();
        string fn = EditorGUILayout.TextField("Function", slider.functionName);
        GUILayout.Space(18f);
        GUILayout.EndHorizontal();

        if (slider.foreground != fg ||
            slider.thumb != tb ||
            slider.direction != dir ||
            slider.fullSize != size ||
            slider.eventReceiver != er ||
            slider.functionName != fn)
        {
            if (slider.fullSize != size) mShowWarning = true;

            NGUIEditorTools.RegisterUndo("Slider Change", slider);
            slider.foreground = fg;
            slider.thumb = tb;
            slider.direction = dir;
            slider.fullSize = size;
            slider.eventReceiver = er;
            slider.functionName = fn;

            if (slider.thumb != null)
            {
                slider.thumb.localPosition = Vector3.zero;
                slider.sliderValue = -1f;
                slider.sliderValue = sliderValue;
            }
            else slider.ForceUpdate();

            UnityEditor.EditorUtility.SetDirty(slider);
        }
    }
}