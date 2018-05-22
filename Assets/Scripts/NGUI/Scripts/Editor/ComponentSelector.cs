//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// EditorGUILayout.ObjectField doesn't support custom components, so a custom wizard saves the day.
/// Unfortunately this tool only shows components that are being used by the scene, so it's a "recently used" selection tool.
/// </summary>
public class ComponentSelector : ScriptableWizard
{
    public delegate void OnSelectionCallback(MonoBehaviour obj);

    private System.Type mType;
    private OnSelectionCallback mCallback;
    private MonoBehaviour[] mObjects;

    /// <summary>
    /// Draw a button + object selection combo filtering specified types.
    /// </summary>
    static public void Draw<T>(string buttonName, T obj, OnSelectionCallback cb, params GUILayoutOption[] options) where T : MonoBehaviour
    {
        GUILayout.BeginHorizontal();
        bool show = GUILayout.Button(buttonName, GUILayout.Width(76f));
#if !UNITY_3_4
        GUILayout.BeginVertical();
        GUILayout.Space(5f);
#endif
        T o = EditorGUILayout.ObjectField(obj, typeof(T), false, options) as T;
#if !UNITY_3_4
        GUILayout.EndVertical();
#endif
        GUILayout.EndHorizontal();
        if (show) Show<T>(cb);
        else if (o != obj) cb(o);
    }

    /// <summary>
    /// Draw a button + object selection combo filtering specified types.
    /// </summary>
    static public void Draw<T>(T obj, OnSelectionCallback cb, params GUILayoutOption[] options) where T : MonoBehaviour
    {
        Draw<T>(NGUITools.GetName<T>(), obj, cb, options);
    }

    /// <summary>
    /// Show the selection wizard.
    /// </summary>
    private static void Show<T>(OnSelectionCallback cb) where T : MonoBehaviour
    {
        System.Type type = typeof(T);
        ComponentSelector comp = ScriptableWizard.DisplayWizard<ComponentSelector>("Select " + type.ToString());
        comp.mType = type;
        comp.mCallback = cb;
        comp.mObjects = Resources.FindObjectsOfTypeAll(type) as MonoBehaviour[];
    }

    /// <summary>
    /// Draw the custom wizard.
    /// </summary>
    private void OnGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);

        if (mObjects.Length == 0)
        {
            GUILayout.Label("No recently used " + mType.ToString() + " components found.\nTry drag & dropping one instead.");
        }
        else
        {
            GUILayout.Label("List of recently used components:");
            NGUIEditorTools.DrawSeparator();

            MonoBehaviour sel = null;

            foreach (MonoBehaviour o in mObjects)
            {
                if (DrawObject(o))
                {
                    sel = o;
                }
            }

            if (sel != null)
            {
                mCallback(sel);
                Close();
            }
        }
    }

    /// <summary>
    /// Draw details about the specified monobehavior in column format.
    /// </summary>
    private bool DrawObject(MonoBehaviour mb)
    {
        bool retVal = false;

        GUILayout.BeginHorizontal();
        {
            if (EditorUtility.IsPersistent(mb.gameObject))
            {
                GUILayout.Label("Prefab", GUILayout.Width(80f));
            }
            else
            {
                GUI.color = Color.grey;
                GUILayout.Label("Object", GUILayout.Width(80f));
            }

            GUILayout.Label(NGUITools.GetHierarchy(mb.gameObject));
            GUI.color = Color.white;
            retVal = GUILayout.Button("Select", GUILayout.Width(60f));
        }
        GUILayout.EndHorizontal();
        return retVal;
    }
}