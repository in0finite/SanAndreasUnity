//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Allows dragging of the camera object and restricts camera's movement to be within bounds of the area created by the rootForBounds colliders.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Drag Camera")]
public class UIDragCamera : IgnoreTimeScale
{
    /// <summary>
    /// Target object that will be dragged.
    /// </summary>

    public UIDraggableCamera draggableCamera;

    // Version 1.92 and earlier referenced the target and had properties specified on every drag script.
    [HideInInspector] [SerializeField] private Component target;

    /// <summary>
    /// Automatically find the draggable camera if possible.
    /// </summary>

    private void Awake()
    {
        // Legacy functionality support for backwards compatibility
        if (target != null)
        {
            if (draggableCamera == null)
            {
                draggableCamera = target.GetComponent<UIDraggableCamera>();

                if (draggableCamera == null)
                {
                    draggableCamera = target.gameObject.AddComponent<UIDraggableCamera>();
                }
            }
            target = null;
        }
        else if (draggableCamera == null)
        {
            draggableCamera = NGUITools.FindInParents<UIDraggableCamera>(gameObject);
        }
    }

    /// <summary>
    /// Forward the press event to the draggable camera.
    /// </summary>

    private void OnPress(bool isPressed)
    {
        if (enabled && gameObject.active && draggableCamera != null)
        {
            draggableCamera.Press(isPressed);
        }
    }

    /// <summary>
    /// Forward the drag event to the draggable camera.
    /// </summary>

    private void OnDrag(Vector2 delta)
    {
        if (enabled && gameObject.active && draggableCamera != null)
        {
            draggableCamera.Drag(delta);
        }
    }

    /// <summary>
    /// Forward the scroll event to the draggable camera.
    /// </summary>

    private void OnScroll(float delta)
    {
        if (enabled && gameObject.active && draggableCamera != null)
        {
            draggableCamera.Scroll(delta);
        }
    }
}