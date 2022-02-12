using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public class CoroutineManager : StartupSingleton<CoroutineManager>
    {
        private static CoroutineRunner m_coroutineRunner = new CoroutineRunner();


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void InitOnLoad()
        {
            UnityEditor.EditorApplication.update -= EditorUpdate;
            UnityEditor.EditorApplication.update += EditorUpdate;
        }

        static void EditorUpdate()
        {
            // note: this will also update coroutines in play mode, and also while the Editor is paused.
            // if coroutines wish to avoid that, we need to add a flag for every coroutine
            m_coroutineRunner.Update();
        }
#endif

        void Update()
        {
            if (!Application.isEditor)
                m_coroutineRunner.Update();
        }

        public static CoroutineInfo Start(IEnumerator coroutine)
        {
            return m_coroutineRunner.StartCoroutine(coroutine, null, null);
        }

        public static CoroutineInfo Start(
            IEnumerator coroutine,
            System.Action onFinishSuccess,
            System.Action<System.Exception> onFinishError)
        {
            return m_coroutineRunner.StartCoroutine(coroutine, onFinishSuccess, onFinishError);
        }

        public static void Stop(CoroutineInfo coroutineInfo)
        {
            m_coroutineRunner.StopCoroutine(coroutineInfo);
        }

        public static bool IsRunning(CoroutineInfo coroutineInfo)
        {
            return m_coroutineRunner.IsCoroutineRunning(coroutineInfo);
        }
    }
}
