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
            return EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#else
            return false;
#endif
        }
    }
}
