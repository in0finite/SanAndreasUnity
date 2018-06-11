using UnityEngine;

public class xGUILayout
{
    public delegate void DoubleClickCallback(int index);

    public static int SelectionList(int selected, GUIContent[] list)
    {
        return SelectionList(selected, list, "List Item", null);
    }

    public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle)
    {
        return SelectionList(selected, list, elementStyle, null);
    }

    public static int SelectionList(int selected, GUIContent[] list, DoubleClickCallback callback)
    {
        return SelectionList(selected, list, "List Item", callback);
    }

    public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle, DoubleClickCallback callback)
    {
        for (int i = 0; i < list.Length; ++i)
        {
            Rect elementRect = GUILayoutUtility.GetRect(list[i], elementStyle);
            bool hover = elementRect.Contains(Event.current.mousePosition);
            if (hover && Event.current.type == EventType.MouseDown && Event.current.clickCount == 1) // added " && Event.current.clickCount == 1"
            {
                selected = i;
                Event.current.Use();
            }
            else if (hover && callback != null && Event.current.type == EventType.MouseDown && Event.current.clickCount == 2) //Changed from MouseUp to MouseDown
            {
                Debug.Log("Works !");
                callback(i);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.Repaint)
            {
                elementStyle.Draw(elementRect, list[i], hover, false, i == selected, false);
            }
        }
        return selected;
    }

    public static int SelectionList(int selected, string[] list)
    {
        return SelectionList(selected, list, "List Item", null);
    }

    public static int SelectionList(int selected, string[] list, GUIStyle elementStyle)
    {
        return SelectionList(selected, list, elementStyle, null);
    }

    public static int SelectionList(int selected, string[] list, DoubleClickCallback callback)
    {
        return SelectionList(selected, list, "List Item", callback);
    }

    public static int SelectionList(int selected, string[] list, GUIStyle elementStyle, DoubleClickCallback callback)
    {
        for (int i = 0; i < list.Length; ++i)
        {
            Rect elementRect = GUILayoutUtility.GetRect(new GUIContent(list[i]), elementStyle);
            bool hover = elementRect.Contains(Event.current.mousePosition);
            if (hover && Event.current.type == EventType.MouseDown)
            {
                selected = i;
                Event.current.Use();
            }
            else if (hover && callback != null && Event.current.type == EventType.MouseUp && Event.current.clickCount == 2)
            {
                callback(i);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.Repaint)
            {
                elementStyle.Draw(elementRect, list[i], hover, false, i == selected, false);
            }
        }
        return selected;
    }
}