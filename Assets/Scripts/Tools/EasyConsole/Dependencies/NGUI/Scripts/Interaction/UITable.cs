//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All children added to the game object with this script will be arranged into a table
/// with rows and columns automatically adjusting their size to fit their content
/// (think "table" tag in HTML).
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Table")]
public class UITable : MonoBehaviour
{
    public delegate void OnReposition();

    public enum Direction
    {
        Down,
        Up,
    }

    public int columns = 0;
    public Direction direction = Direction.Down;
    public Vector2 padding = Vector2.zero;
    public bool sorted = false;
    public bool hideInactive = true;
    public bool repositionNow = false;
    public bool keepWithinPanel = false;
    public OnReposition onReposition;

    private UIPanel mPanel;
    private UIDraggablePanel mDrag;
    private bool mStarted = false;

    /// <summary>
    /// Function that sorts items by name.
    /// </summary>
    static public int SortByName(Transform a, Transform b)
    {
        return string.Compare(a.name, b.name);
    }

    /// <summary>
    /// Positions the grid items, taking their own size into consideration.
    /// </summary>
    private void RepositionVariableSize(List<Transform> children)
    {
        float xOffset = 0;
        float yOffset = 0;

        int cols = columns > 0 ? children.Count / columns + 1 : 1;
        int rows = columns > 0 ? columns : children.Count;

        Bounds[,] bounds = new Bounds[cols, rows];
        Bounds[] boundsRows = new Bounds[rows];
        Bounds[] boundsCols = new Bounds[cols];

        int x = 0;
        int y = 0;

        for (int i = 0, imax = children.Count; i < imax; ++i)
        {
            Transform t = children[i];
            Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t);
            Vector3 scale = t.localScale;
            b.min = Vector3.Scale(b.min, scale);
            b.max = Vector3.Scale(b.max, scale);
            bounds[y, x] = b;

            boundsRows[x].Encapsulate(b);
            boundsCols[y].Encapsulate(b);

            if (++x >= columns && columns > 0)
            {
                x = 0;
                ++y;
            }
        }

        x = 0;
        y = 0;

        for (int i = 0, imax = children.Count; i < imax; ++i)
        {
            Transform t = children[i];
            Bounds b = bounds[y, x];
            Bounds br = boundsRows[x];
            Bounds bc = boundsCols[y];

            Vector3 pos = t.localPosition;
            pos.x = xOffset + b.extents.x - b.center.x;
            pos.x += b.min.x - br.min.x + padding.x;

            if (direction == Direction.Down)
            {
                pos.y = -yOffset - b.extents.y - b.center.y;
                pos.y += (b.max.y - b.min.y - bc.max.y + bc.min.y) * 0.5f - padding.y;
            }
            else
            {
                pos.y = yOffset + b.extents.y - b.center.y;
                pos.y += (b.max.y - b.min.y - bc.max.y + bc.min.y) * 0.5f - padding.y;
            }

            xOffset += br.max.x - br.min.x + padding.x * 2f;

            t.localPosition = pos;

            if (++x >= columns && columns > 0)
            {
                x = 0;
                ++y;

                xOffset = 0f;
                yOffset += bc.size.y + padding.y * 2f;
            }
        }
    }

    /// <summary>
    /// Recalculate the position of all elements within the table, sorting them alphabetically if necessary.
    /// </summary>
    public void Reposition()
    {
        if (mStarted)
        {
            Transform myTrans = transform;
            List<Transform> children = new List<Transform>();

            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform child = myTrans.GetChild(i);
                if (child && (!hideInactive || child.gameObject.active)) children.Add(child);
            }
            if (sorted) children.Sort(SortByName);
            if (children.Count > 0) RepositionVariableSize(children);
            if (mPanel != null && mDrag == null) mPanel.ConstrainTargetToBounds(myTrans, true);
            if (mDrag != null) mDrag.UpdateScrollbars(true);
        }
        else repositionNow = true;
    }

    /// <summary>
    /// Position the grid's contents when the script starts.
    /// </summary>
    private void Start()
    {
        mStarted = true;

        if (keepWithinPanel)
        {
            mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
            mDrag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
        }
        Reposition();
    }

    /// <summary>
    /// Is it time to reposition? Do so now.
    /// </summary>
    private void LateUpdate()
    {
        if (repositionNow)
        {
            repositionNow = false;
            Reposition();
            if (onReposition != null) onReposition();
        }
    }
}