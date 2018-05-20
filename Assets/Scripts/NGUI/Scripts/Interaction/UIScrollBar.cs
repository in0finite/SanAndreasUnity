//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright � 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Scroll bar functionality.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Scroll Bar")]
public class UIScrollBar : MonoBehaviour
{
    public enum Direction
    {
        Horizontal,
        Vertical,
    };

    public delegate void OnScrollBarChange(UIScrollBar sb);

    [HideInInspector] [SerializeField] private UISprite mBG;
    [HideInInspector] [SerializeField] private UISprite mFG;
    [HideInInspector] [SerializeField] private Direction mDir = Direction.Horizontal;
    [HideInInspector] [SerializeField] private bool mInverted = false;
    [HideInInspector] [SerializeField] private float mScroll = 0f;
    [HideInInspector] [SerializeField] private float mSize = 1f;

    private Transform mTrans;
    private bool mIsDirty = false;
    private Camera mCam;
    private Vector2 mScreenPos = Vector2.zero;

    /// <summary>
    /// Delegate triggered when the scroll bar has changed visibly.
    /// </summary>

    public OnScrollBarChange onChange;

    /// <summary>
    /// Cached for speed.
    /// </summary>

    public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

    /// <summary>
    /// Camera used to draw the scroll bar.
    /// </summary>

    public Camera cachedCamera { get { if (mCam == null) mCam = NGUITools.FindCameraForLayer(gameObject.layer); return mCam; } }

    /// <summary>
    /// Sprite used for the background.
    /// </summary>

    public UISprite background { get { return mBG; } set { if (mBG != value) { mBG = value; mIsDirty = true; } } }

    /// <summary>
    /// Sprite used for the foreground.
    /// </summary>

    public UISprite foreground { get { return mFG; } set { if (mFG != value) { mFG = value; mIsDirty = true; } } }

    /// <summary>
    /// The scroll bar's direction.
    /// </summary>

    public Direction direction
    {
        get
        {
            return mDir;
        }
        set
        {
            if (mDir != value)
            {
                mDir = value;
                mIsDirty = true;

                // Since the direction is changing, see if we need to swap width with height (for convenience)
                if (mBG != null)
                {
                    Transform t = mBG.cachedTransform;
                    Vector3 scale = t.localScale;

                    if ((mDir == Direction.Vertical && scale.x > scale.y) ||
                        (mDir == Direction.Horizontal && scale.x < scale.y))
                    {
                        float x = scale.x;
                        scale.x = scale.y;
                        scale.y = x;
                        t.localScale = scale;
                        ForceUpdate();

                        // Update the colliders as well
                        if (mBG.GetComponent<Collider>() != null) NGUITools.AddWidgetCollider(mBG.gameObject);
                        if (mFG.GetComponent<Collider>() != null) NGUITools.AddWidgetCollider(mFG.gameObject);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Whether the movement direction is flipped.
    /// </summary>

    public bool inverted { get { return mInverted; } set { if (mInverted != value) { mInverted = value; mIsDirty = true; } } }

    /// <summary>
    /// Modifiable value for the scroll bar, 0-1 range.
    /// </summary>

    public float scrollValue
    {
        get
        {
            return mScroll;
        }
        set
        {
            float val = Mathf.Clamp01(value);

            if (mScroll != val)
            {
                mScroll = val;
                mIsDirty = true;
                if (onChange != null) onChange(this);
            }
        }
    }

    /// <summary>
    /// The size of the foreground bar in percent (0-1 range).
    /// </summary>

    public float barSize
    {
        get
        {
            return mSize;
        }
        set
        {
            float val = Mathf.Clamp01(value);

            if (mSize != val)
            {
                mSize = val;
                mIsDirty = true;
                if (onChange != null) onChange(this);
            }
        }
    }

    /// <summary>
    /// Allows to easily change the scroll bar's alpha, affecting both the foreground and the background sprite at once.
    /// </summary>

    public float alpha
    {
        get
        {
            if (mFG != null) return mFG.alpha;
            if (mBG != null) return mBG.alpha;
            return 0f;
        }
        set
        {
            if (mFG != null)
            {
                mFG.alpha = value;
                mFG.gameObject.active = mFG.alpha > 0.001f;
            }

            if (mBG != null)
            {
                mBG.alpha = value;
                mBG.gameObject.active = mBG.alpha > 0.001f;
            }
        }
    }

    /// <summary>
    /// Move the scroll bar to be centered on the specified position.
    /// </summary>

    private void CenterOnPos(Vector2 localPos)
    {
        if (mBG == null || mFG == null) return;

        // Background's bounds
        Bounds bg = NGUIMath.CalculateRelativeInnerBounds(cachedTransform, mBG);
        Bounds fg = NGUIMath.CalculateRelativeInnerBounds(cachedTransform, mFG);

        if (mDir == Direction.Horizontal)
        {
            float size = bg.size.x - fg.size.x;
            float offset = size * 0.5f;
            float min = bg.center.x - offset;
            float val = (size > 0f) ? (localPos.x - min) / size : 0f;
            scrollValue = mInverted ? 1f - val : val;
        }
        else
        {
            float size = bg.size.y - fg.size.y;
            float offset = size * 0.5f;
            float min = bg.center.y - offset;
            float val = (size > 0f) ? 1f - (localPos.y - min) / size : 0f;
            scrollValue = mInverted ? 1f - val : val;
        }
    }

    /// <summary>
    /// Drag the scroll bar by the specified on-screen amount.
    /// </summary>

    private void Reposition(Vector2 screenPos)
    {
        // Create a plane
        Transform trans = cachedTransform;
        Plane plane = new Plane(trans.rotation * Vector3.back, trans.position);

        // If the ray doesn't hit the plane, do nothing
        float dist;
        Ray ray = cachedCamera.ScreenPointToRay(screenPos);
        if (!plane.Raycast(ray, out dist)) return;

        // Transform the point from world space to local space
        CenterOnPos(trans.InverseTransformPoint(ray.GetPoint(dist)));
    }

    /// <summary>
    /// Position the scroll bar to be under the current touch.
    /// </summary>

    private void OnPressBackground(GameObject go, bool isPressed)
    {
        mCam = UICamera.currentCamera;
        Reposition(UICamera.lastTouchPosition);
    }

    /// <summary>
    /// Position the scroll bar to be under the current touch.
    /// </summary>

    private void OnDragBackground(GameObject go, Vector2 delta)
    {
        mCam = UICamera.currentCamera;
        Reposition(UICamera.lastTouchPosition);
    }

    /// <summary>
    /// Save the position of the foreground on press.
    /// </summary>

    private void OnPressForeground(GameObject go, bool isPressed)
    {
        if (isPressed)
        {
            mCam = UICamera.currentCamera;
            Bounds b = NGUIMath.CalculateAbsoluteWidgetBounds(mFG.cachedTransform);
            mScreenPos = mCam.WorldToScreenPoint(b.center);
        }
    }

    /// <summary>
    /// Drag the scroll bar in the specified direction.
    /// </summary>

    private void OnDragForeground(GameObject go, Vector2 delta)
    {
        mCam = UICamera.currentCamera;
        Reposition(mScreenPos + UICamera.currentTouch.totalDelta);
    }

    /// <summary>
    /// Register the event listeners.
    /// </summary>

    private void Start()
    {
        if (background != null && background.GetComponent<Collider>() != null)
        {
            UIEventListener listener = UIEventListener.Get(background.gameObject);
            listener.onPress += OnPressBackground;
            listener.onDrag += OnDragBackground;
        }

        if (foreground != null && foreground.GetComponent<Collider>() != null)
        {
            UIEventListener listener = UIEventListener.Get(foreground.gameObject);
            listener.onPress += OnPressForeground;
            listener.onDrag += OnDragForeground;
        }
        ForceUpdate();
    }

    /// <summary>
    /// Update the value of the scroll bar if necessary.
    /// </summary>

    private void Update()
    { if (mIsDirty) ForceUpdate(); }

    /// <summary>
    /// Update the value of the scroll bar.
    /// </summary>

    public void ForceUpdate()
    {
        mIsDirty = false;

        if (mBG != null && mFG != null)
        {
            mSize = Mathf.Clamp01(mSize);
            mScroll = Mathf.Clamp01(mScroll);

            Vector4 bg = mBG.border;
            Vector4 fg = mFG.border;

            // Space available for the background
            Vector2 bgs = new Vector2(
                Mathf.Max(0f, mBG.cachedTransform.localScale.x - bg.x - bg.z),
                Mathf.Max(0f, mBG.cachedTransform.localScale.y - bg.y - bg.w));

            float val = mInverted ? 1f - mScroll : mScroll;

            if (mDir == Direction.Horizontal)
            {
                Vector2 fgs = new Vector2(bgs.x * mSize, bgs.y);

                mFG.pivot = UIWidget.Pivot.Left;
                mBG.pivot = UIWidget.Pivot.Left;
                mBG.cachedTransform.localPosition = Vector3.zero;
                mFG.cachedTransform.localPosition = new Vector3(bg.x - fg.x + (bgs.x - fgs.x) * val, 0f, 0f);
                mFG.cachedTransform.localScale = new Vector3(fgs.x + fg.x + fg.z, fgs.y + fg.y + fg.w, 1f);
                if (val < 0.999f && val > 0.001f) mFG.MakePixelPerfect();
            }
            else
            {
                Vector2 fgs = new Vector2(bgs.x, bgs.y * mSize);

                mFG.pivot = UIWidget.Pivot.Top;
                mBG.pivot = UIWidget.Pivot.Top;
                mBG.cachedTransform.localPosition = Vector3.zero;
                mFG.cachedTransform.localPosition = new Vector3(0f, -bg.y + fg.y - (bgs.y - fgs.y) * val, 0f);
                mFG.cachedTransform.localScale = new Vector3(fgs.x + fg.x + fg.z, fgs.y + fg.y + fg.w, 1f);
                if (val < 0.999f && val > 0.001f) mFG.MakePixelPerfect();
            }
        }
    }
}