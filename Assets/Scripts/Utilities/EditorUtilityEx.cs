#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public static class EditorUtilityEx
    {
        public static void MarkObjectAsDirty(Object obj)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(obj);
#endif
        }

        public static bool MarkActiveSceneAsDirty()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) // exception will be thrown if we attempt this in play mode
                return false;
            return EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#else
            return false;
#endif
        }

        public static bool IsAsset(Object obj)
        {
#if UNITY_EDITOR
            return AssetDatabase.Contains(obj);
#else
            return false;
#endif
        }
    }
}
