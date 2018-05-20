//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script should be attached to each camera that's used to draw the objects with
/// UI components on them. This may mean only one camera (main camera or your UI camera),
/// or multiple cameras if you happen to have multiple viewports. Failing to attach this
/// script simply means that objects drawn by this camera won't receive UI notifications:
///
/// - OnHover (isOver) is sent when the mouse hovers over a collider or moves away.
/// - OnPress (isDown) is sent when a mouse button gets pressed on the collider.
/// - OnSelect (selected) is sent when a mouse button is released on the same object as it was pressed on.
/// - OnClick (int button) is sent with the same conditions as OnSelect, with the added check to see if the mouse has not moved much.
/// - OnDoubleClick (int button) is sent when the click happens twice within a fourth of a second.
/// - OnDrag (delta) is sent when a mouse or touch gets pressed on a collider and starts dragging it.
/// - OnDrop (gameObject) is sent when the mouse or touch get released on a different collider than the one that was being dragged.
/// - OnInput (text) is sent when typing (after selecting a collider by clicking on it).
/// - OnTooltip (show) is sent when the mouse hovers over a collider for some time without moving.
/// - OnScroll (float delta) is sent out when the mouse scroll wheel is moved.
/// - OnKey (KeyCode key) is sent when keyboard or controller input is used.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Camera")]
[RequireComponent(typeof(Camera))]
public class UICamera : UIBaseCamera
{
}