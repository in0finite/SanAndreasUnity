//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Allows dragging of the specified target panel's contents by mouse or touch.
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Drag Panel Contents")]
public class UIDragPanelContents : MonoBehaviour
{
    /// <summary>
    /// This panel's contents will be dragged by the script.
    /// </summary>
    public UIDraggablePanel draggablePanel;

    // Version 1.92 and earlier referenced the panel instead of UIDraggablePanel script.
    [HideInInspector] [SerializeField] private UIPanel panel;

    /// <summary>
    /// Backwards compatibility.
    /// </summary>
    private void Awake()
    {
        // Legacy functionality support for backwards compatibility
        if (panel != null)
        {
            if (draggablePanel == null)
            {
                draggablePanel = panel.GetComponent<UIDraggablePanel>();

                if (draggablePanel == null)
                {
                    draggablePanel = panel.gameObject.AddComponent<UIDraggablePanel>();
                }
            }
            panel = null;
        }
    }

    /// <summary>
    /// Automatically find the draggable panel if possible.
    /// </summary>
    private void Start()
    {
        if (draggablePanel == null)
        {
            draggablePanel = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
        }
    }

    /// <summary>
    /// Create a plane on which we will be performing the dragging.
    /// </summary>
    private void OnPress(bool pressed)
    {
        if (enabled && gameObject.active && draggablePanel != null)
        {
            draggablePanel.Press(pressed);
        }
    }

    /// <summary>
    /// Drag the object along the plane.
    /// </summary>
    private void OnDrag(Vector2 delta)
    {
        if (enabled && gameObject.active && draggablePanel != null)
        {
            draggablePanel.Drag(delta);
        }
    }

    /// <summary>
    /// If the object should support the scroll wheel, do it.
    /// </summary>
    private void OnScroll(float delta)
    {
        if (enabled && gameObject.active && draggablePanel != null)
        {
            draggablePanel.Scroll(delta);
        }
    }
}