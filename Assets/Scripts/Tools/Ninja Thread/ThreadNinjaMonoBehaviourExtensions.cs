using System;
using System.Collections;
using UnityEngine;

namespace CielaSpike
{
    public static class ThreadNinjaMonoBehaviourExtensions
    {
        /// <summary>
        /// Start a co-routine on a background thread.
        /// </summary>
        /// <param name="task">Gets a task object with more control on the background thread.</param>
        /// <returns></returns>
        public static Coroutine StartCoroutineAsync(
            this MonoBehaviour behaviour, IEnumerator routine, 
            out Task task)
        {
            task = new Task(routine);
            return behaviour.StartCoroutine(task);
        }

        /// <summary>
        /// Start a co-routine on a background thread.
        /// </summary>
        public static Coroutine StartCoroutineAsync(
            this MonoBehaviour behaviour, IEnumerator routine)
        {
            Task t;
            return StartCoroutineAsync(behaviour, routine, out t);
        }
    }
}