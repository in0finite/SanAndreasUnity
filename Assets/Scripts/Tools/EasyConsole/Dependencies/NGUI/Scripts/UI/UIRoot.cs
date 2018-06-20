//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a script used to keep the game object scaled to 2/(Screen.height).
/// If you use it, be sure to NOT use UIOrthoCamera at the same time.
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Root")]
public class UIRoot : MonoBehaviour
{
    private static List<UIRoot> mRoots = new List<UIRoot>();

    public bool automatic = true;
    public int manualHeight = 800;

    private Transform mTrans;

    private void Awake()
    { mRoots.Add(this); }

    private void OnDestroy()
    { mRoots.Remove(this); }

    private void Start()
    {
        mTrans = transform;

        UIOrthoCamera oc = GetComponentInChildren<UIOrthoCamera>();

        if (oc != null)
        {
            Debug.LogWarning("UIRoot should not be active at the same time as UIOrthoCamera. Disabling UIOrthoCamera.", oc);
            Camera cam = oc.gameObject.GetComponent<Camera>();
            oc.enabled = false;
            if (cam != null) cam.orthographicSize = 1f;
        }
    }

    private void Update()
    {
        manualHeight = Mathf.Max(2, automatic ? Screen.height : manualHeight);

        float size = 2f / manualHeight;
        Vector3 ls = mTrans.localScale;

        if (!Mathf.Approximately(ls.x, size) ||
            !Mathf.Approximately(ls.y, size) ||
            !Mathf.Approximately(ls.z, size))
        {
            mTrans.localScale = new Vector3(size, size, size);
        }
    }

    /// <summary>
    /// Broadcast the specified message to the entire UI.
    /// </summary>
    static public void Broadcast(string funcName)
    {
        for (int i = 0, imax = mRoots.Count; i < imax; ++i)
        {
            UIRoot root = mRoots[i];
            if (root != null) root.BroadcastMessage(funcName, SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// Broadcast the specified message to the entire UI.
    /// </summary>
    static public void Broadcast(string funcName, object param)
    {
        if (param == null)
        {
            // More on this: http://answers.unity3d.com/questions/55194/suggested-workaround-for-sendmessage-bug.html
            Debug.LogError("SendMessage is bugged when you try to pass 'null' in the parameter field. It behaves as if no parameter was specified.");
        }
        else
        {
            for (int i = 0, imax = mRoots.Count; i < imax; ++i)
            {
                UIRoot root = mRoots[i];
                if (root != null) root.BroadcastMessage(funcName, param, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}