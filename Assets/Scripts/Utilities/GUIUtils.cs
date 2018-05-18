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
}