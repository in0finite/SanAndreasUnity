using SanAndreasUnity.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class EditorWindowBase : EditorWindow
    {
        public class CoroutineInfo
        {
            private static long s_lastId = 0;
            public long Id { get; } = ++s_lastId;

            public IEnumerator coroutine { get; }
            public System.Action onFinishSuccess { get; }
            public System.Action<System.Exception> onFinishError { get; }

            public CoroutineInfo(IEnumerator coroutine, Action onFinishSuccess, Action<Exception> onFinishError)
            {
                this.coroutine = coroutine;
                this.onFinishSuccess = onFinishSuccess;
                this.onFinishError = onFinishError;
            }
        }

        private List<CoroutineInfo> m_coroutines = new List<CoroutineInfo>();
        private List<CoroutineInfo> m_newCoroutines = new List<CoroutineInfo>();
        private List<CoroutineInfo> m_coroutinesToRemove = new List<CoroutineInfo>();


        public EditorWindowBase()
        {
            EditorApplication.update -= this.EditorUpdate;
            EditorApplication.update += this.EditorUpdate;
        }

        protected CoroutineInfo StartCoroutine(IEnumerator coroutine, System.Action onFinishSuccess, System.Action<System.Exception> onFinishError)
        {
            var coroutineInfo = new CoroutineInfo(coroutine, onFinishSuccess, onFinishError);
            m_newCoroutines.Add(coroutineInfo);
            return coroutineInfo;
        }

        protected bool IsCoroutineRunning(CoroutineInfo coroutineInfo)
        {
            return m_coroutines.Contains(coroutineInfo);
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
