using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Utilities
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

    public class CoroutineRunner
    {
        private List<CoroutineInfo> m_coroutines = new List<CoroutineInfo>();
        private List<CoroutineInfo> m_newCoroutines = new List<CoroutineInfo>();
        

        public CoroutineInfo StartCoroutine(IEnumerator coroutine, System.Action onFinishSuccess, System.Action<System.Exception> onFinishError)
        {
            var coroutineInfo = new CoroutineInfo(coroutine, onFinishSuccess, onFinishError);
            m_newCoroutines.Add(coroutineInfo);
            return coroutineInfo;
        }

        public void StopCoroutine(CoroutineInfo coroutineInfo)
        {
            if (null == coroutineInfo)
                return;

            int index = m_coroutines.IndexOf(coroutineInfo);
            if (index >= 0)
                m_coroutines[index] = null;

            m_newCoroutines.Remove(coroutineInfo);
        }

        public bool IsCoroutineRunning(CoroutineInfo coroutineInfo)
        {
            if (null == coroutineInfo)
                return false;

            return m_coroutines.Contains(coroutineInfo);
        }

        public void Update()
        {
            m_coroutines.RemoveAll(c => null == c);

            m_coroutines.AddRange(m_newCoroutines);
            m_newCoroutines.Clear();

            for (int i = 0; i < m_coroutines.Count; i++)
            {
                this.UpdateCoroutine(m_coroutines[i], i);
            }
            
        }

        void UpdateCoroutine(CoroutineInfo coroutine, int coroutineIndex)
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
                m_coroutines[coroutineIndex] = null;

                if (isSuccess)
                {
                    if (coroutine.onFinishSuccess != null)
                        F.RunExceptionSafe(coroutine.onFinishSuccess);
                }
                else
                {
                    if (coroutine.onFinishError != null)
                        F.RunExceptionSafe(() => coroutine.onFinishError(failureException));
                }
            }
        }
    }
}
