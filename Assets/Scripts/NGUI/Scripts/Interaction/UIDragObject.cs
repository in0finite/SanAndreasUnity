//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Allows dragging of the specified target object by mouse or touch, optionally limiting it to be within the UIPanel's clipped rectangle.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Drag Object")]
public class UIDragObject : IgnoreTimeScale
{
    public enum DragEffect
    {
        None,
        Momentum,
        MomentumAndSpring,
    }

    /// <summary>
    /// Target object that will be dragged.
    /// </summary>

    public Transform target;

    /// <summary>
    /// Scale value applied to the drag delta. Set X or Y to 0 to disallow dragging in that direction.
    /// </summary>

    public Vector3 scale = Vector3.one;

    /// <summary>
    /// Effect the scroll wheel will have on the momentum.
    /// </summary>

    public float scrollWheelFactor = 0f;

    /// <summary>
    /// Whether the dragging will be restricted to be within the parent panel's bounds.
    /// </summary>

    public bool restrictWithinPanel = false;

    /// <summary>
    /// Effect to apply when dragging.
    /// </summary>

    public DragEffect dragEffect = DragEffect.MomentumAndSpring;

    /// <summary>
    /// How much momentum gets applied when the press is released after dragging.
    /// </summary>

    public float momentumAmount = 35f;

    private Plane mPlane;
    private Vector3 mLastPos;
    private UIPanel mPanel;
    private bool mPressed = false;
    private Vector3 mMomentum = Vector3.zero;
    private float mScroll = 0f;
    private Bounds mBounds;

    /// <summary>
    /// Find the panel responsible for this object.
    /// </summary>

    private void FindPanel()
    {
        mPanel = (target != null) ? UIPanel.Find(target.transform, false) : null;
        if (mPanel == null) restrictWithinPanel = false;
    }

    /// <summary>
    /// Create a plane on which we will be performing the dragging.
    /// </summary>

    private void OnPress(bool pressed)
    {
        if (enabled && gameObject.active && target != null)
        {
            mPressed = pressed;

            if (pressed)
            {
                if (restrictWithinPanel && mPanel == null) FindPanel();

                // Calculate the bounds
                if (restrictWithinPanel) mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);

                // Remove all momentum on press
                mMomentum = Vector3.zero;
                mScroll = 0f;

                // Disable the spring movement
                SpringPosition sp = target.GetComponent<SpringPosition>();
                if (sp != null) sp.enabled = false;

                // Remember the hit position
                mLastPos = UICamera.lastHit.point;

                // Create the plane to drag along
                Transform trans = UICamera.currentCamera.transform;
                mPlane = new Plane((mPanel != null ? mPanel.cachedTransform.rotation : trans.rotation) * Vector3.back, mLastPos);
            }
            else if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None && dragEffect == DragEffect.MomentumAndSpring)
            {
                mPanel.ConstrainTargetToBounds(target, ref mBounds, false);
            }
        }
    }

    /// <summary>
    /// Drag the object along the plane.
    /// </summary>

    private void OnDrag(Vector2 delta)
    {
        if (enabled && gameObject.active && target != null)
        {
            UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

            Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
            float dist = 0f;

            if (mPlane.Raycast(ray, out dist))
            {
                Vector3 currentPos = ray.GetPoint(dist);
                Vector3 offset = currentPos - mLastPos;
                mLastPos = currentPos;

                if (offset.x != 0f || offset.y != 0f)
                {
                    offset = target.InverseTransformDirection(offset);
                    offset.Scale(scale);
                    offset = target.TransformDirection(offset);
                }

                // Adjust the momentum
                mMomentum = Vector3.Lerp(mMomentum, mMomentum + offset * (0.01f * momentumAmount), 0.67f);

                // We want to constrain the UI to be within bounds
                if (restrictWithinPanel)
                {
                    // Adjust the position and bounds
                    Vector3 localPos = target.localPosition;
                    target.position += offset;
                    mBounds.center = mBounds.center + (target.localPosition - localPos);

                    // Constrain the UI to the bounds, and if done so, eliminate the momentum
                    if (dragEffect != DragEffect.MomentumAndSpring && mPanel.clipping != UIDrawCall.Clipping.None &&
                        mPanel.ConstrainTargetToBounds(target, ref mBounds, true))
                    {
                        mMomentum = Vector3.zero;
                        mScroll = 0f;
                    }
                }
                else
                {
                    // Adjust the position
                    target.position += offset;
                }
            }
        }
    }

    /// <summary>
    /// Apply the dragging momentum.
    /// </summary>

    private void LateUpdate()
    {
        float delta = UpdateRealTimeDelta();
        if (target == null) return;

        if (mPressed)
        {
            // Disable the spring movement
            SpringPosition sp = target.GetComponent<SpringPosition>();
            if (sp != null) sp.enabled = false;
            mScroll = 0f;
        }
        else
        {
            mMomentum += scale * (-mScroll * 0.05f);
            mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

            if (mMomentum.magnitude > 0.0001f)
            {
                // Apply the momentum
                if (mPanel == null) FindPanel();

                if (mPanel != null)
                {
                    target.position += NGUIMath.SpringDampen(ref mMomentum, 9f, delta);

                    if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
                    {
                        mBounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, target);

                        if (!mPanel.ConstrainTargetToBounds(target, ref mBounds, dragEffect == DragEffect.None))
                        {
                            SpringPosition sp = target.GetComponent<SpringPosition>();
                            if (sp != null) sp.enabled = false;
                        }
                    }
                    return;
                }
            }
            else mScroll = 0f;
        }

        // Dampen the momentum
        NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
    }

    /// <summary>
    /// If the object should support the scroll wheel, do it.
    /// </summary>

    private void OnScroll(float delta)
    {
        if (enabled && gameObject.active)
        {
            if (Mathf.Sign(mScroll) != Mathf.Sign(delta)) mScroll = 0f;
            mScroll += delta * scrollWheelFactor;
        }
    }
}