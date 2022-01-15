using SanAndreasUnity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class EditorWindowBase : EditorWindow
    {
        private class CoroutineInfo
        {
            public IEnumerator coroutine;
            public System.Action onFinishSuccess;
            public System.Action<System.Exception> onFinishError;
        }

        private List<CoroutineInfo> m_coroutines = new List<CoroutineInfo>();
        private List<CoroutineInfo> m_newCoroutines = new List<CoroutineInfo>();
        private List<CoroutineInfo> m_coroutinesToRemove = new List<CoroutineInfo>();


        public EditorWindowBase()
        {
            EditorApplication.update -= this.EditorUpdate;
            EditorApplication.update += this.EditorUpdate;
        }

        protected void StartCoroutine(IEnumerator coroutine, System.Action onFinishSuccess, System.Action<System.Exception> onFinishError)
        {
            m_newCoroutines.Add(new CoroutineInfo
            {
                coroutine = coroutine,
                onFinishSuccess = onFinishSuccess,
                onFinishError = onFinishError,
            });
        }

        void EditorUpdate()
        {
            m_coroutines.AddRange(m_newCoroutines);
            m_newCoroutines.Clear();

            foreach (var coroutine in m_coroutines)
            {
                F.RunExceptionSafe(() => this.UpdateCoroutine(coroutine));
            }

            m_coroutines.RemoveAll(c => m_coroutinesToRemove.Contains(c));
            m_coroutinesToRemove.Clear();
        }

        void UpdateCoroutine(CoroutineInfo coroutine)
        {
            bool isFinished = false;
            bool isSuccess = false;
            System.Exception failureException = null;

            try
            {
                if (!coroutine.coroutine.MoveNext())
                {
                    isFinished = true;
                    isSuccess = true;
                }
            }
            catch (System.Exception ex)
            {
                isFinished = true;
                isSuccess = false;
                failureException = ex;
                Debug.LogException(ex);
            }
            
            if (isFinished)
            {
                m_coroutinesToRemove.Add(coroutine);

                if (isSuccess)
                    F.RunExceptionSafe(coroutine.onFinishSuccess);
                else
                    F.RunExceptionSafe(() => coroutine.onFinishError(failureException));
            }
        }
    }
}
