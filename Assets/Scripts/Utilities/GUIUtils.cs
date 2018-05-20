using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScreenCorner { TopRight, TopLeft, BottomRight, BottomLeft }

public static class GUIUtils
{
    public static Rect GetCornerRect(ScreenCorner corner, Vector2 size, Vector2? padding = null)
    {
        return GetCornerRect(corner, size.x, size.y, padding);
    }

    public static Rect GetCornerRect(ScreenCorner corner, float width, float height, Vector2? padding = null)
    {
        float padX = 0,
              padY = 0;

        if (padding != null)
        {
            padX = padding.Value.x;
            padY = padding.Value.y;
        }

        switch (corner)
        {
            case ScreenCorner.TopLeft:
                return new Rect(padX, padY, width, height);

            case ScreenCorner.TopRight:
                return new Rect(Screen.width - (width + padX), padY, width, height);

            case ScreenCorner.BottomLeft:
                return new Rect(padX, Screen.height - (height + padY), width, height);

            case ScreenCorner.BottomRight:
                return new Rect(Screen.width - (width + padX), Screen.height - (height + padY), width, height);
        }

        return default(Rect);
    }

    public static void drawString(string text, Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        UnityEditor.Handles.EndGUI();
    }
}