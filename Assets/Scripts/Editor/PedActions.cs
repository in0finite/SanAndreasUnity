using Fclp.Internals.Extensions;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class PedActions
{
    private const bool onStateChanged = false;

    static PedActions()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange currentMode)
    {
        if (onStateChanged && currentMode == PlayModeStateChange.EnteredEditMode)
            RemoveUnnamed();
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        RemoveUnnamed();
    }

    private static void RemoveUnnamed()
    {
        GameObject playerModel = GameObject.Find("PlayerModel");

        playerModel.GetComponents<FrameContainer>().ForEach(x => x.SafeDestroy());

        try
        {
            playerModel.GetComponentsInChildren<Component>().Where(x => x.GetType() != typeof(Transform) && x.transform.parent == playerModel.transform).Select(x => x.gameObject).ForEach(x => x.SafeDestroy());
        }
        catch { }

        EditorSceneManager.SaveOpenScenes();
    }
}