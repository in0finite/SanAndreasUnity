//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

#if !UNITY_FLASH
#endif

/// <summary>
/// UI Panel is responsible for collecting, sorting and updating widgets in addition to generating widgets' geometry.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Panel")]
public class UIPanel : UIBasePanel
{
    /// <summary>
    /// Helper function that recursively sets all childrens' game objects layers to the specified value, stopping when it hits another UIBasePanel.
    /// </summary>

    private static void SetChildLayer(Transform t, int layer)
    {
        for (int i = 0; i < t.childCount; ++i)
        {
            Transform child = t.GetChild(i);

            if (child.GetComponent<UIPanel>() == null)
            {
                child.gameObject.layer = layer;
                SetChildLayer(child, layer);
            }
        }
    }

    /// <summary>
    /// Find the UIBasePanel responsible for handling the specified transform.
    /// </summary>

    new static public UIPanel Find(Transform trans, bool createIfMissing)
    {
        Transform origin = trans;
        UIPanel panel = null;

        while (panel == null && trans != null)
        {
            panel = trans.GetComponent<UIPanel>();
            if (panel != null) break;
            if (trans.parent == null) break;
            trans = trans.parent;
        }

        if (createIfMissing && panel == null && trans != origin)
        {
            panel = trans.gameObject.AddComponent<UIPanel>();
            SetChildLayer(panel.cachedTransform, panel.gameObject.layer);
        }
        return panel;
    }

    /// <summary>
    /// Find the UIBasePanel responsible for handling the specified transform, creating a new one if necessary.
    /// </summary>

    new static public UIPanel Find(Transform trans)
    {
        return Find(trans, true);
    }
}