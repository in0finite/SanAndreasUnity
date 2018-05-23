using Fclp.Internals.Extensions;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PedActions
{
    static PedActions()
    {
        Debug.Log("SingleEntryPoint. Up and running");
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange currentMode)
    {
        if (currentMode == PlayModeStateChange.ExitingPlayMode)
        {
            GameObject playerModel = GameObject.Find("PlayerModel");

            var frames = playerModel.GetComponents<FrameContainer>();

            Debug.LogFormat("FrameContainer Count: {0}", frames.Count());

            if (frames != null)
                frames.ForEach(x => x.SafeDestroy());

            IEnumerable<Component> unnamedChilds = playerModel.GetComponents<Component>().Where(x => x.GetType() != typeof(Transform) && x.transform.parent == playerModel.transform);
            if (unnamedChilds != null)
                unnamedChilds.ForEach(x => x.SafeDestroyGameObject());
        }
    }
}