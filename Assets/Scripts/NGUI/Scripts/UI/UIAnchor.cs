//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script can be used to anchor an object to the side of the screen,
/// or scale an object to always match the dimensions of the screen.
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Anchor")]
public class UIAnchor : MonoBehaviour
{
    public enum Side
    {
        BottomLeft,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        Center,
    }

    public Camera uiCamera = null;
    public Side side = Side.Center;
    public bool halfPixelOffset = true;
    public float depthOffset = 0f;
    public Vector2 relativeOffset = Vector2.zero;

    // Stretching is now done by a separate script -- UIStretch, as of version 1.90.
    [HideInInspector]
    [SerializeField]
    private bool stretchToFill = false;

    private Transform mTrans;
    private bool mIsWindows = false;

    /// <summary>
    /// Cache the transform.
    /// </summary>
    private void Awake()
    { mTrans = transform; }

    /// <summary>
    /// Legacy support.
    /// </summary>
    private void Start()
    {
        if (stretchToFill)
        {
            stretchToFill = false;

            UIStretch stretch = gameObject.AddComponent<UIStretch>();
            stretch.style = UIStretch.Style.Both;
            stretch.uiCamera = uiCamera;
        }
    }

    /// <summary>
    /// Automatically find the camera responsible for drawing the widgets under this object.
    /// </summary>
    private void OnEnable()
    {
        mIsWindows = (Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WebGLPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor);

        if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
    }

    /// <summary>
    /// Anchor the object to the appropriate point.
    /// </summary>
    private void Update()
    {
        if (uiCamera != null)
        {
            Rect rect = uiCamera.pixelRect;
            float cx = (rect.xMin + rect.xMax) * 0.5f;
            float cy = (rect.yMin + rect.yMax) * 0.5f;
            Vector3 v = new Vector3(cx, cy, depthOffset);

            if (side != Side.Center)
            {
                if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
                {
                    v.x = rect.xMax;
                }
                else if (side == Side.Top || side == Side.Center || side == Side.Bottom)
                {
                    v.x = cx;
                }
                else
                {
                    v.x = rect.xMin;
                }

                if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
                {
                    v.y = rect.yMax;
                }
                else if (side == Side.Left || side == Side.Center || side == Side.Right)
                {
                    v.y = cy;
                }
                else
                {
                    v.y = rect.yMin;
                }
            }

            float screenWidth = rect.width;
            float screenHeight = rect.height;

            v.x += relativeOffset.x * screenWidth;
            v.y += relativeOffset.y * screenHeight;

            if (uiCamera.orthographic)
            {
                v.x = Mathf.RoundToInt(v.x);
                v.y = Mathf.RoundToInt(v.y);

                if (halfPixelOffset && mIsWindows)
                {
                    v.x -= 0.5f;
                    v.y += 0.5f;
                }
            }

            // Convert from screen to world coordinates, since the two may not match (UIRoot set to manual size)
            v = uiCamera.ScreenToWorldPoint(v);

            // Wrapped in an 'if' so the scene doesn't get marked as 'edited' every frame
            if (mTrans.position != v) mTrans.position = v;
        }
    }
}