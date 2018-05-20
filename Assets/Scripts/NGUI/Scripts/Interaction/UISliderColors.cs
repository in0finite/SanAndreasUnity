//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script automatically changes the color of the specified sprite based on the value of the slider.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UISlider))]
[AddComponentMenu("NGUI/Examples/Slider Colors")]
public class UISliderColors : MonoBehaviour
{
    public UISprite sprite;

    public Color[] colors = new Color[] { Color.red, Color.yellow, Color.green };

    private UISlider mSlider;

    private void Start()
    { mSlider = GetComponent<UISlider>(); Update(); }

    private void Update()
    {
        if (sprite == null || colors.Length == 0) return;

        float val = mSlider.sliderValue;
        val *= (colors.Length - 1);
        int startIndex = Mathf.FloorToInt(val);

        Color c = colors[0];

        if (startIndex >= 0)
        {
            if (startIndex + 1 < colors.Length)
            {
                float factor = (val - startIndex);
                c = Color.Lerp(colors[startIndex], colors[startIndex + 1], factor);
            }
            else if (startIndex < colors.Length)
            {
                c = colors[startIndex];
            }
            else c = colors[colors.Length - 1];
        }

        c.a = sprite.color.a;
        sprite.color = c;
    }
}