using System;
using System.Collections.Generic;
using UnityEngine;

namespace CI.TaskParallel.Core
{
    public class UnityDispatcher : MonoBehaviour
    {
        private readonly Queue<Action> _queue = new Queue<Action>();
        private readonly object _lock = new object();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            lock (_lock)
            {
                while (_queue.Count > 0)
                {
                    _queue.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _queue.Enqueue(action);
            }
        }
    }
}